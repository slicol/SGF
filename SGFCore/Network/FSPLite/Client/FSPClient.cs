/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
 * 帧同步模块
 * Frame synchronization module
 * 
 * Licensed under the MIT License (the "License"); 
 * you may not use this file except in compliance with the License. 
 * You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, 
 * software distributed under the License is distributed on an "AS IS" BASIS, 
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. 
 * See the License for the specific language governing permissions and limitations under the License.
*/

using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using SGF.Codec;
using SGF.Extension;
using SGF.Network.Core;
using SGF.Time;
using SGF.Utils;

namespace SGF.Network.FSPLite.Client
{
    public class FSPClient
    {
        private bool m_IsRunning = false;


        //基础通讯部分
        private string m_ip;
        private int m_port;
        private uint m_sid;
        private Socket m_SystemSocket;
        private Thread m_ThreadRecv;
        private IPEndPoint m_RemoteEndPoint;
        private bool m_WaitForReconnect = false;
        private uint m_lastRecvTimestamp = 0;


        //KCP部分
        private KCP m_Kcp;
        private SwitchQueue<byte[]> m_RecvBufQueue = new SwitchQueue<byte[]>(128);
        private bool m_NeedKcpUpdateFlag = false;
        private uint m_NextKcpUpdateTime = 0;


        private int m_authId;

        //接收逻辑
        private Action<FSPFrame> m_RecvListener;
        private byte[] m_RecvBufferTemp = new byte[4096];

        //发送逻辑
        private FSPDataC2S m_TempSendData = new FSPDataC2S();
        private byte[] m_SendBufferTemp = new byte[4096];


        //------------------------------------------------------------
        #region 构造相关
        public void Init(uint sid)
        {
            Debuger.Log("sid:{0}", sid);
            m_sid = sid;
            m_TempSendData.sid = sid;
            m_TempSendData.msgs.Add(new FSPMessage());

            m_Kcp = new KCP(m_sid, HandleKcpSend);
            m_Kcp.NoDelay(1, 10, 2, 1);
            m_Kcp.WndSize(128, 128);
        }


        public void Clean()
        {
            Debuger.Log();
            if (m_Kcp != null)
            {
                m_Kcp.Dispose();
                m_Kcp = null;
            }
            m_RecvListener = null;
            Close();
        }

        #endregion
        
        //------------------------------------------------------------
        #region 设置参数
        public void SetFSPAuthInfo(int authId)
        {
            Debuger.Log(authId);
            m_authId = authId;
        }

        public void SetFSPListener(Action<FSPFrame> listener)
        {
            Debuger.Log();
            m_RecvListener = listener;

        }


        public void VerifyAuth()
        {
            Debuger.Log();
            SendFSP(0, FSPBasicCmd.AUTH, m_authId);
        }


        #endregion

        //------------------------------------------------------------

        #region 连接相关逻辑

        public bool IsRunning { get { return m_IsRunning; } }

        public bool Connect(string ip, int port)
        {
            if (m_SystemSocket != null)
            {
                Debuger.LogError("无法建立连接，需要先关闭上一次连接！");
                return false;
            }

            Debuger.Log("建立基础连接， host = {0}, port = {1}", ip, port);
            m_ip = ip;
            m_port = port;
            m_lastRecvTimestamp = (uint)TimeUtils.GetTotalMillisecondsSince1970();

            try
            {
                m_RemoteEndPoint = IPUtils.GetHostEndPoint(m_ip, m_port);
                if (m_RemoteEndPoint == null)
                {
                    Debuger.LogError("无法将Host解析为IP！");
                    Close();
                    return false;
                }
                Debuger.Log("HostEndPoint = {0}", m_RemoteEndPoint.ToString());

                //创建Socket
                Debuger.Log("创建Socket, AddressFamily = {0}", m_RemoteEndPoint.AddressFamily);
                m_SystemSocket = new Socket(m_RemoteEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                m_SystemSocket.Bind(IPUtils.GetIPEndPointAny(AddressFamily.InterNetwork, 0));


                m_IsRunning = true;

                m_ThreadRecv = new Thread(Thread_Recv) { IsBackground = true };
                m_ThreadRecv.Start();

            }
            catch (Exception e)
            {
                Debuger.LogError(e.Message + e.StackTrace);
                Close();
                return false;
            }

            return true;
        }


        private void Close()
        {
            Debuger.Log();

            m_IsRunning = false;

            if (m_ThreadRecv != null)
            {
                m_ThreadRecv.Interrupt();
                m_ThreadRecv = null;
            }

            if (m_SystemSocket != null)
            {
                try
                {
                    m_SystemSocket.Shutdown(SocketShutdown.Both);

                }
                catch (Exception e)
                {
                    Debuger.LogWarning(e.Message + e.StackTrace);
                }

                m_SystemSocket.Close();
                m_SystemSocket = null;
            }
        }


        private void Reconnect()
        {
            m_WaitForReconnect = false;
            Close();
            Connect(m_ip, m_port);
            VerifyAuth();
        }

        #endregion

        //------------------------------------------------------------







        private void Thread_Recv()
        {
            while (m_IsRunning)
            {
                try
                {
                    DoReceiveInThread();
                }
                catch (SocketException se)
                {
                    Debuger.LogWarning("SocketErrorCode:{0}, {1}", se.SocketErrorCode, se.Message);
                    Thread.Sleep(1);
                }
                catch (Exception e)
                {
                    Debuger.LogWarning(e.Message + "\n" + e.StackTrace);
                    Thread.Sleep(1);
                }
            }
        }


        private void DoReceiveInThread()
        {
            EndPoint remotePoint = IPUtils.GetIPEndPointAny(AddressFamily.InterNetwork, 0);
            int cnt = m_SystemSocket.ReceiveFrom(m_RecvBufferTemp, m_RecvBufferTemp.Length, SocketFlags.None, ref remotePoint);

            if (cnt > 0)
            {
                if (!m_RemoteEndPoint.Equals(remotePoint))
                {
                    Debuger.LogError("收到非目标服务器的数据！");
                    return;
                }

                byte[] dst = new byte[cnt];
                Buffer.BlockCopy(m_RecvBufferTemp, 0, dst, 0, cnt);
                m_RecvBufQueue.Push(dst);
                
            }


        }


        private void DoReceiveInMain()
        {
            m_RecvBufQueue.Switch();
            while (!m_RecvBufQueue.Empty())
            {
                var recvBufferRaw = m_RecvBufQueue.Pop();
                int ret = m_Kcp.Input(recvBufferRaw, recvBufferRaw.Length);

                //收到的不是一个正确的KCP包
                if (ret < 0)
                {
                    Debuger.LogError("收到不正确的KCP包, Ret:{0}", ret);
                    return;
                }

                m_NeedKcpUpdateFlag = true;

                for (int size = m_Kcp.PeekSize(); size > 0; size = m_Kcp.PeekSize())
                {
                    var recvBuffer = new byte[size];
                    if (m_Kcp.Recv(recvBuffer) > 0)
                    {
                        m_lastRecvTimestamp = (uint)TimeUtils.GetTotalMillisecondsSince1970();

                        var data = PBSerializer.NDeserialize<FSPDataS2C>(recvBuffer);

                        if (m_RecvListener != null)
                        {
                            for (int i = 0; i < data.frames.Count; i++)
                            {
                                m_RecvListener(data.frames[i]);
                            }
                        }

                    }
                }

            }


        }


        //------------------------------------------------------------
        #region 发送相关逻辑

        private void HandleKcpSend(byte[] buffer, int size)
        {
            m_SystemSocket.SendTo(buffer, 0, size, SocketFlags.None, m_RemoteEndPoint);
        }


        public bool SendFSP(int clientFrameId, int cmd, int arg)
        {
            return SendFSP(clientFrameId, cmd, new int[] {arg});
        }


        public bool SendFSP(int clientFrameId, int cmd, int[] args)
        {
            Debuger.Log("clientFrameId:{0}, cmd:{1}, args:{2}", clientFrameId, cmd, args.ToListString());

            if (m_IsRunning)
            {

                FSPMessage msg = m_TempSendData.msgs[0];
                msg.cmd = cmd;
                msg.clientFrameId = clientFrameId;
                msg.args = args;

                m_TempSendData.msgs.Clear();
                m_TempSendData.msgs.Add(msg);
                

                int len = PBSerializer.NSerialize(m_TempSendData, m_SendBufferTemp);
                m_Kcp.Send(m_SendBufferTemp, len);
                return len > 0;
            }

            return false;
        }


        #endregion

        //------------------------------------------------------------

        private void CheckTimeout()
        {
            uint current = (uint)TimeUtils.GetTotalMillisecondsSince1970();
            var dt = current - m_lastRecvTimestamp;
            if (dt > 5000)
            {
                m_WaitForReconnect = true;
            }
            
        }


        public void Tick()
        {
            if (!m_IsRunning)
            {
                return;
            }

            DoReceiveInMain();

            uint current = (uint)TimeUtils.GetTotalMillisecondsSince1970();
            if (m_NeedKcpUpdateFlag || current >= m_NextKcpUpdateTime)
            {
                if (m_Kcp != null)
                {
                    m_Kcp.Update(current);
                    m_NextKcpUpdateTime = m_Kcp.Check(current);
                    m_NeedKcpUpdateFlag = false;

                }
            }


            if (m_WaitForReconnect)
            {
                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    Reconnect();
                }
                else
                {
                    Debuger.Log("等待重连，但是网络不可用！");
                }
            }
            

            CheckTimeout();
        }



        public string ToDebugString()
        {
            return string.Format("ip:{0}, port:{1}", m_ip, m_port);
            
        }

    }
}
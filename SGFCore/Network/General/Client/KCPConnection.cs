/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
 * 基于KCP的可靠UDP连接类（客户端）
 * KCP-based reliable UDP connection class (client)
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
using System.Net.Sockets;
using System.Threading;
using SGF.SEvent;
using SGF.Network.Core;
using SGF.Utils;
using SGF.Network.General.Proto;
using System.Collections.Generic;
using SGF.Codec;

namespace SGF.Network.General.Client
{
    public class KcpConnection:IConnection, ILogTag
    {
        public string LOG_TAG { get; protected set; }

        //==========================================================
        //事件
        //==========================================================
        public Signal<IConnection, int, string> onSendError { get; private set; }
        public Signal<IConnection, int, string> onReceiveError { get; private set; }
        public Signal<IConnection, int, string> onServerError { get; private set; }

        /// <summary>
        /// 当收到数据时发出信号
        /// 参数：字节数组，长度
        /// </summary>
        public Signal<NetMessage> onReceive { get; private set; }

        //==========================================================
        //公共成员变量
        //==========================================================
        public uint Id { get; protected set; }
        public IPEndPoint LocalEndPoint { get; private set; }
        public IPEndPoint RemoteEndPoint { get; private set; }
        public ushort Ping { get; set; }
        public bool IsActived { get { return Thread.VolatileRead(ref this.m_actived) == 1; } }
        private int m_actived = 0;


        //==========================================================
        //私有成员变量
        //==========================================================
        private Socket m_socket;
        private Thread m_ThreadRecv;
        private byte[] m_RecvBufferTemp = new byte[4096];
        
        private List<IPEndPoint> m_listRemoteEndPoint;
        private int m_currRemoteEndPointIndex = -1;
        private int m_localPort;
        
        private bool m_waitReconnect = false;

        //KCP
        private KCP m_Kcp;
        private SwitchQueue<byte[]> m_RecvBufQueue = new SwitchQueue<byte[]>();
        private uint m_NextKcpUpdateTime = 0;
        private bool m_NeedKcpUpdateFlag = false;

        //发送相关
        private NetBufferWriter m_bufferSend;
        private NetBufferWriter m_bufferReceive = null;


        //==========================================================
        //构造函数
        //==========================================================
        public KcpConnection(int localPort)
        {
            LOG_TAG = "KcpConnection[" + 0 + "," + localPort + "]";
            this.Log("connId:{0}, localPort:{1}", 0, localPort);

            onReceive = new Signal<NetMessage>();
            onSendError = new Signal<IConnection, int, string>();
            onReceiveError = new Signal<IConnection, int, string>();
            onServerError = new Signal<IConnection, int, string>();

            m_listRemoteEndPoint = new List<IPEndPoint>();
            m_currRemoteEndPointIndex = -1;

            m_bufferSend = new NetBufferWriter(new byte[0]);
            m_bufferReceive = new NetBufferWriter(new byte[NetDefine.ReceiveBufferMinSize]);

            m_localPort = localPort;
        }


        public void Clean()
        {
            this.Log();

            onReceive.RemoveAllListeners();
            onReceiveError.RemoveAllListeners();
            onSendError.RemoveAllListeners();
            onServerError.RemoveAllListeners();

            Close();
        }

        //======================================================================
        //连接与断开连接
        //======================================================================

        public void Connect(string remoteIP, int remotePort)
        {
            this.Log("{0}:{1}", remoteIP, remotePort);

            IPAddress ipaddr = null;
            if (!IPAddress.TryParse(remoteIP, out ipaddr))
            {
                this.LogError("无法解析为有效的IPAddress:{0}", remoteIP);
                return;
            }

            m_listRemoteEndPoint.Clear();
            m_listRemoteEndPoint.Add(new IPEndPoint(ipaddr, remotePort));
            m_currRemoteEndPointIndex = 0;
            
            ConnectInternal(m_listRemoteEndPoint[m_currRemoteEndPointIndex]);
        }

        public void Connect(IPEndPoint[] listRemoteEndPoint)
        {

            if (listRemoteEndPoint.Length == 0)
            {
                this.LogError("参数错误：listRemoteEndPoint.Length = 0");
                return;
            }

            m_listRemoteEndPoint.Clear();
            m_listRemoteEndPoint.AddRange(listRemoteEndPoint);
            m_currRemoteEndPointIndex = 0;

            ConnectInternal(m_listRemoteEndPoint[m_currRemoteEndPointIndex]);
        }

        public void Close()
        {
            this.Log();
            this.m_actived = 0;

            if (m_Kcp != null)
            {
                m_Kcp.Dispose();
                m_Kcp = null;
            }


            if (m_ThreadRecv != null)
            {
                m_ThreadRecv.Interrupt();
                m_ThreadRecv = null;
            }

            if (m_socket != null)
            {
                try { m_socket.Shutdown(SocketShutdown.Both); }
                catch (Exception ex) { this.LogWarning(ex.Message); }

                m_socket.Close();
                m_socket = null;
            }
        }

        private void TryReconnect()
        {
            Debuger.LogWarning("");

            if (!IsActived)
            {
                m_currRemoteEndPointIndex++;
                m_currRemoteEndPointIndex = m_currRemoteEndPointIndex % m_listRemoteEndPoint.Count;
                ConnectInternal(m_listRemoteEndPoint[m_currRemoteEndPointIndex]);
            }
            else
            {
                this.LogWarning("当前连接还在，不能重连！");
            }
        }


        private void ConnectInternal(IPEndPoint remoteEndPoint)
        {
            this.Log();
            m_waitReconnect = false;

            var socket = new Socket(remoteEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            var localEndPoint = new IPEndPoint(IPAddress.Any, m_localPort);
            try { socket.Bind(localEndPoint); }
            catch (Exception exception) { this.LogWarning("指定的Port无法绑定:{0}", localEndPoint); }

            Active(socket, (IPEndPoint)remoteEndPoint);

            LOG_TAG = "KcpConnection[" + Id + "," + LocalEndPoint.Port + "]";
            this.LogVerbose("连接成功！");
        }

        public void Active(Socket socket, IPEndPoint remoteEndPoint)
        {
            if (Interlocked.CompareExchange(ref this.m_actived, 1, 0) == 0)
            {
                m_socket = socket;

                socket.ReceiveBufferSize = NetDefine.SocketBufferSize;
                socket.SendBufferSize = NetDefine.SocketBufferSize;

                RemoteEndPoint = (IPEndPoint)remoteEndPoint;
                LocalEndPoint = (IPEndPoint)socket.LocalEndPoint;

                ResetKcp();

                m_ThreadRecv = new Thread(Thread_Recv) { IsBackground = true };
                m_ThreadRecv.Start();
            }
        }

        private void ResetKcp()
        {
            m_Kcp = new KCP(0, HandleKcpSend);
            m_Kcp.NoDelay(1, 10, 2, 1);
            m_Kcp.WndSize(128, 128);
        }

        //======================================================================
        //发送数据
        //======================================================================
        public bool Send(NetMessage msg)
        {
            //主线程
            this.LogVerbose();

            msg.head.sid = Id;

            m_bufferSend.Attach(new byte[msg.Length], 0);
            msg.Serialize(m_bufferSend);

            var bytes = m_bufferSend.GetBytes();
            var len = m_bufferSend.Length;

            return m_Kcp.Send(bytes, len) > 0;
        }


        private void HandleKcpSend(byte[] bytes, int len)
        {
            //主线程
            m_hasSocketException = false;
            m_socket.SendTo(bytes, 0, len, SocketFlags.None, RemoteEndPoint);
        }

        //======================================================================
        //接收数据
        //======================================================================
        protected void DoReceiveInThread()
        {
            //子线程
            this.LogVerbose();

            bool result = false;
            try
            {
                EndPoint remotePoint = IPUtils.GetIPEndPointAny(AddressFamily.InterNetwork, 0);
                int cnt = m_socket.ReceiveFrom(m_RecvBufferTemp, m_RecvBufferTemp.Length, SocketFlags.None, ref remotePoint);

                if (cnt > 0)
                {
                    if (!RemoteEndPoint.Equals(remotePoint))
                    {
                        Debuger.LogVerbose("收到非目标服务器的数据！");
                        return;
                    }

                    m_bufferReceive.Attach(m_RecvBufferTemp, cnt);
                    byte[] m_32b = new byte[4];
                    m_bufferReceive.ReadBytes(m_32b, 0, 4);
                    uint sid = BitConverter.ToUInt32(m_32b, 0);
                    
                    if(sid == 0)
                    {
                        //Session过期了
                        HandleServerError((int)NetErrorCode.SessionExpire);
                        return;
                    }

                    byte[] dst = new byte[cnt];
                    Buffer.BlockCopy(m_RecvBufferTemp, 0, dst, 0, cnt);
                    
                    m_RecvBufQueue.Push(dst);
                }
            }
            catch (Exception ex)
            {
                this.LogWarning("接收数据出错：{0}", ex.Message);
                onReceiveError.InvokeSafe(this, (int)NetErrorCode.UnkownError, ex.Message);
            }
        }

        private void DoReceiveInMain()
        {
            //主线程
            m_RecvBufQueue.Switch();

            while (!m_RecvBufQueue.Empty())
            {
                var recvBufferRaw = m_RecvBufQueue.Pop();
                int ret = m_Kcp.Input(recvBufferRaw, recvBufferRaw.Length);

                //收到的不是一个正确的KCP包
                if (ret < 0)
                {
                    Debuger.LogError("收到不正确的KCP包!Ret:{0}", ret);
                    return;
                }

                m_NeedKcpUpdateFlag = true;

                for (int size = m_Kcp.PeekSize(); size > 0; size = m_Kcp.PeekSize())
                {
                    var recvBuffer = new byte[size];
                    if (m_Kcp.Recv(recvBuffer) > 0)
                    {
                        NetMessage msg = new NetMessage();
                        msg.Deserialize(recvBuffer, size);


                        if (msg.head.sid == 0)
                        {
                            var errmsg = PBSerializer.NDeserialize<NetErrorMessage>(msg.content);
                            this.LogWarning("服务器返回错误:{0},{1}", errmsg.code, errmsg.info);

                            HandleServerError(errmsg.code);

                            onServerError.InvokeSafe(this, errmsg.code, errmsg.info);
                        }
                        else
                        {
                            //更新SessionId
                            if (Id != msg.head.sid)
                            {
                                Id = msg.head.sid;
                                LOG_TAG = "KcpConnection[" + Id + "," + m_localPort + "]";
                                this.LogWarning("SessionId发生变化：{0}", Id);
                            }

                            onReceive.InvokeSafe(msg);
                        }
                    }
                }

            }

        }

        private void HandleServerError(int errcode)
        {
            if (errcode == (int)NetErrorCode.SessionExpire)
            {
                Id = 0;
                LOG_TAG = "KcpConnection[" + Id + "," + m_localPort + "]";
                this.LogWarning("Session过期!");
            }

            onServerError.InvokeSafe(this, (int)NetErrorCode.UnkownError, "");
        }

        //=================================================================================
        //接收线程 
        //=================================================================================
        private bool m_hasSocketException = false;
        private void Thread_Recv()
        {
            while (IsActived)
            {
                try
                {
                    DoReceiveInThread();

                    if (m_hasSocketException)
                    {
                        m_hasSocketException = false;
                        Debuger.LogWarning("连接异常已经恢复");
                    }
                }
                catch (SocketException se)
                {
                    if (!m_hasSocketException)
                    {
                        m_hasSocketException = true;
                    }

                    Debuger.LogWarning("SocketErrorCode:{0}, {1}", se.SocketErrorCode, se.Message);
                    Thread.Sleep(1);
                }
                catch (Exception e)
                {
                    Debuger.LogWarning(e.Message + "\n" + e.StackTrace);
                    Thread.Sleep(1);
                }
                
            }

            Debuger.LogWarning("End!");
        }




        public void Tick()
        {
            if (IsActived)
            {
                DoReceiveInMain();

                uint current = (uint) TimeUtils.GetTotalMillisecondsSince1970();

                if (m_NeedKcpUpdateFlag || current >= m_NextKcpUpdateTime)
                {
                    if (m_Kcp != null)
                    {
                        m_Kcp.Update(current);
                        m_NextKcpUpdateTime = m_Kcp.Check(current);
                        m_NeedKcpUpdateFlag = false;
                    }
                }
            }
            else
            {
                if (m_waitReconnect)
                {
                    TryReconnect();
                }
            }
        }


    }
}

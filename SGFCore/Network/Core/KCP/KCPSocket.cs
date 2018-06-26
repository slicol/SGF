/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
 * 将KCP与Socket封装在一起，以提供可靠UDP通讯功能
 * KCP:https://github.com/skywind3000/kcp
 * KCP CSharp Port:https://github.com/limpo1989/kcp-csharp
 * Package KCP with Socket to provide reliable UDP communication
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
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace SGF.Network.Core
{
    public class KCPSocket
    {
        private static readonly DateTime UTCTimeBegin = new DateTime(1970, 1, 1);

        public static UInt32 GetClockMS()
        {
            return (UInt32)(Convert.ToInt64(DateTime.UtcNow.Subtract(UTCTimeBegin).TotalMilliseconds) & 0xffffffff);
        }

        public delegate void KCPReceiveListener(byte[] buff, int size, IPEndPoint remotePoint);



        public string LOG_TAG = "KCPSocket";

        private bool m_IsRunning = false;
        private Socket m_SystemSocket;
        private IPEndPoint m_LocalEndPoint;
        private AddressFamily m_AddrFamily;
        private Thread m_ThreadRecv;
        private byte[] m_RecvBufferTemp = new byte[4096];
        
        //KCP参数
        private List<KCPProxy> m_ListKcp; 
        private uint m_KcpKey = 0;
        private KCPReceiveListener m_AnyEPListener;

        //=================================================================================
        #region 构造和析构

        public KCPSocket(int bindPort, uint kcpKey, AddressFamily family = AddressFamily.InterNetwork)
        {
            m_AddrFamily = family;
            m_KcpKey = kcpKey;
            m_ListKcp = new List<KCPProxy>();

            m_SystemSocket = new Socket(m_AddrFamily, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint ipep = IPUtils.GetIPEndPointAny(m_AddrFamily, bindPort);
            m_SystemSocket.Bind(ipep);

            bindPort = (m_SystemSocket.LocalEndPoint as IPEndPoint).Port;
            LOG_TAG = "KCPSocket[" + bindPort + "-" + kcpKey + "]";

            m_IsRunning = true;
            m_ThreadRecv = new Thread(Thread_Recv) {IsBackground = true};
            m_ThreadRecv.Start();



#if UNITY_EDITOR_WIN
            uint IOC_IN = 0x80000000;
            uint IOC_VENDOR = 0x18000000;
            uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
            m_SystemSocket.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);
#endif

#if UNITY_EDITOR
            UnityEditor.EditorApplication.playmodeStateChanged -= OnEditorPlayModeChanged;
            UnityEditor.EditorApplication.playmodeStateChanged += OnEditorPlayModeChanged;
#endif
        }


#if UNITY_EDITOR
        private void OnEditorPlayModeChanged()
        {
            if (UnityEngine.Application.isPlaying == false)
            {
                UnityEditor.EditorApplication.playmodeStateChanged -= OnEditorPlayModeChanged;
                Dispose();
            }
        }
#endif


        public void Dispose()
        {
            m_IsRunning = false;
            m_AnyEPListener = null;

            if (m_ThreadRecv != null)
            {
                m_ThreadRecv.Interrupt();
                m_ThreadRecv = null;
            }

            int cnt = m_ListKcp.Count;
            for (int i = 0; i < cnt; i++)
            {
                m_ListKcp[i].Dispose();
            }
            m_ListKcp.Clear();
            
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

        public void Connect(IPEndPoint remotePoint)
        {
            m_SystemSocket.Connect(remotePoint);
        }

        public int SelfPort
        {
            get{return (m_SystemSocket.LocalEndPoint as IPEndPoint).Port;}
        }

        public string SelfIP
        {
			get { return IPUtils.SelfIP; }
        }

        public IPEndPoint SelfEndPoint
        {
            get
            {
                if (m_LocalEndPoint == null || 
					m_LocalEndPoint.Address.ToString() != IPUtils.SelfIP)
                {
                    IPAddress ip = IPAddress.Parse(SelfIP);
                    m_LocalEndPoint = new IPEndPoint(ip, SelfPort);
                }

                return m_LocalEndPoint;
            }
        }

        public Socket SystemSocket { get { return m_SystemSocket; } }
        
        #endregion

        //=================================================================================

        public bool EnableBroadcast
        {
            get { return m_SystemSocket.EnableBroadcast; }
            set { m_SystemSocket.EnableBroadcast = value; }
        }

        //=================================================================================
        #region 管理KCP

        private bool IsValidKcpIPEndPoint(IPEndPoint ipep)
        {
            if (ipep == null || ipep.Port == 0 || 
                ipep.Address.Equals(IPAddress.Any) || 
                ipep.Address.Equals(IPAddress.IPv6Any))
            {
                return false;
            }
            return true;
        }

        private KCPProxy GetKcp(IPEndPoint ipep, bool autoAdd = true)
        {
            if (!IsValidKcpIPEndPoint(ipep))
            {
                return null;
            }

            KCPProxy proxy = null;
            int cnt = m_ListKcp.Count;
            for (int i = 0; i < cnt; i++)
            {
                proxy = m_ListKcp[i];
                if (proxy.RemotePoint.Equals(ipep))
                {
                    return proxy;
                }
            }

            if (autoAdd)
            {
                proxy = new KCPProxy(m_KcpKey, ipep, m_SystemSocket);
                proxy.AddReceiveListener(OnReceiveAny);
                m_ListKcp.Add(proxy);
            }
            return proxy;
        }


        public void CloseKcp(IPEndPoint ipep)
        {
            KCPProxy proxy = null;
            int cnt = m_ListKcp.Count;
            for (int i = 0; i < cnt; i++)
            {
                proxy = m_ListKcp[i];
                if (proxy.RemotePoint.Equals(ipep))
                {
                    m_ListKcp.RemoveAt(i);
                    break;
                }
            }
        }

        #endregion

        //=================================================================================
        #region 发送逻辑
        public bool SendTo(byte[] buffer, int size, IPEndPoint remotePoint)
        {
            if (remotePoint.Address == IPAddress.Broadcast)
            {
                int cnt = m_SystemSocket.SendTo(buffer, size, SocketFlags.None, remotePoint);
                return cnt > 0;
            }
            else
            {
                KCPProxy proxy = GetKcp(remotePoint);
                if (proxy != null)
                {
                    return proxy.DoSend(buffer, size);
                }
            }

            return false;
        }

        public bool SendTo(string message, IPEndPoint remotePoint)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            return SendTo(buffer, buffer.Length, remotePoint);
        }

        #endregion


        //=================================================================================
        #region 主线程驱动

        public void Update()
        {
            if (m_IsRunning)
            {
                //获取时钟
                uint current = GetClockMS();

                int cnt = m_ListKcp.Count;
                for (int i = 0; i < cnt; i++)
                {
                    KCPProxy proxy = m_ListKcp[i];
                    proxy.Update(current);
                }
            }
        }

        #endregion

        //=================================================================================
        #region 接收逻辑

        public void AddReceiveListener(IPEndPoint remotePoint, KCPReceiveListener listener)
        {
            KCPProxy proxy = GetKcp(remotePoint);
            if (proxy != null)
            {
                proxy.AddReceiveListener(listener);
            }
            else
            {
                m_AnyEPListener += listener;
            }
        }

        public void RemoveReceiveListener(IPEndPoint remotePoint, KCPReceiveListener listener)
        {
            KCPProxy proxy = GetKcp(remotePoint);
            if (proxy != null)
            {
                proxy.RemoveReceiveListener(listener);
            }
            else
            {
                m_AnyEPListener -= listener;
            }
        }

        public void AddReceiveListener(KCPReceiveListener listener)
        {
            m_AnyEPListener += listener;
        }

        public void RemoveReceiveListener(KCPReceiveListener listener)
        {
            m_AnyEPListener -= listener;
        }


        private void OnReceiveAny(byte[] buffer, int size,   IPEndPoint remotePoint)
        {
            if (m_AnyEPListener != null)
            {
                m_AnyEPListener(buffer, size, remotePoint);
            }
        }

        #endregion

        //=================================================================================
        #region 接收线程

        private void Thread_Recv()
        {
            Debuger.Log("Thread_Recv() Begin ......");

            while (m_IsRunning)
            {
                try
                {
                    DoReceive();
                }
                catch (Exception e)
                {
                    Debuger.LogWarning("Thread_Recv() " + e.Message + "\n" + e.StackTrace);
                    Thread.Sleep(10);
                }
            }

            Debuger.Log("Thread_Recv() End!");
        }

        private void DoReceive()
        {
            EndPoint remotePoint = new IPEndPoint(IPAddress.Any, 0);
            int cnt = m_SystemSocket.ReceiveFrom(m_RecvBufferTemp, m_RecvBufferTemp.Length,
                SocketFlags.None, ref remotePoint);

            if (cnt > 0)
            {
                KCPProxy proxy = GetKcp((IPEndPoint)remotePoint);
                if (proxy != null)
                {
                    proxy.DoReceiveInThread(m_RecvBufferTemp, cnt);
                }    
            }
            
        }

        #endregion
    }




    class KCPProxy
    {
        
        private KCP m_Kcp;
        private bool m_NeedKcpUpdateFlag = false;
        private uint m_NextKcpUpdateTime = 0;
        private SwitchQueue<byte[]> m_RecvQueue = new SwitchQueue<byte[]>(128);

        private IPEndPoint m_RemotePoint;
        private Socket m_Socket;
        private KCPSocket.KCPReceiveListener m_Listener;

        public IPEndPoint RemotePoint { get { return m_RemotePoint; } }


        public KCPProxy(uint key, IPEndPoint remotePoint, Socket socket)
        {
            m_Socket = socket;
            m_RemotePoint = remotePoint;

            m_Kcp = new KCP(key, HandleKcpSend);
            m_Kcp.NoDelay(1, 10, 2, 1);
            m_Kcp.WndSize(128, 128);

        }

        public void Dispose()
        {
            m_Socket = null;

            NetDebuger.WeakNetCancel(this);
            
            if (m_Kcp != null)
            {
                m_Kcp.Dispose();
                m_Kcp = null;
            }

            m_Listener = null;
        }

        //---------------------------------------------
        private void HandleKcpSend(byte[] buff, int size)
        {
            if (NetDebuger.EnableWeakNet)
            {
                NetDebuger.WeakNetSimulate(this, buff, size, HandleKcpSend_Hook);
            }
            else
            {
                if (m_Socket != null)
                {
                    m_Socket.SendTo(buff, 0, size, SocketFlags.None, m_RemotePoint);
                }    
            }
        }

        private void HandleKcpSend_Hook(byte[] buff, int size)
        {
            if (m_Socket != null)
            {
                m_Socket.SendTo(buff, 0, size, SocketFlags.None, m_RemotePoint);
            }    
        }

        public bool DoSend(byte[] buff, int size)
        {
            m_NeedKcpUpdateFlag = true;
            return m_Kcp.Send(buff, size) >= 0;
        }

        //---------------------------------------------

        public void AddReceiveListener(KCPSocket.KCPReceiveListener listener)
        {
            m_Listener += listener;
        }

        public void RemoveReceiveListener(KCPSocket.KCPReceiveListener listener)
        {
            m_Listener -= listener;
        }



        public void DoReceiveInThread(byte[] buffer, int size)
        {
            if (NetDebuger.IsPacketLoss())
            {
                return;
            }

            byte[] dst = new byte[size];
            Buffer.BlockCopy(buffer, 0, dst, 0, size);
            m_RecvQueue.Push(dst);
        }

        private void HandleRecvQueue()
        {
            m_RecvQueue.Switch();
            while (!m_RecvQueue.Empty())
            {
                var recvBufferRaw = m_RecvQueue.Pop();
                int ret = m_Kcp.Input(recvBufferRaw, recvBufferRaw.Length);

                //收到的不是一个正确的KCP包
                if (ret < 0)
                {
                    if (m_Listener != null)
                    {
                        m_Listener(recvBufferRaw, recvBufferRaw.Length, m_RemotePoint);
                    }
                    return;
                }

                m_NeedKcpUpdateFlag = true;
                KCP kcp = m_Kcp;

                for (int size = kcp.PeekSize(); size > 0; size = kcp.PeekSize())
                {
                    var recvBuffer = new byte[size];
                    if (kcp.Recv(recvBuffer) > 0)
                    {
                        if (m_Listener != null)
                        {
                            m_Listener(recvBuffer, size, m_RemotePoint);
                        }
                    }
                }
            }
        }

        //---------------------------------------------
        public void Update(uint currentTimeMS)
        {
            HandleRecvQueue();

            if (m_NeedKcpUpdateFlag || currentTimeMS >= m_NextKcpUpdateTime)
            {
                if (m_Kcp != null)
                {
                    m_Kcp.Update(currentTimeMS);
                    m_NextKcpUpdateTime = m_Kcp.Check(currentTimeMS);
                    m_NeedKcpUpdateFlag = false;
                }
            }
        }

        //---------------------------------------------

    }
}

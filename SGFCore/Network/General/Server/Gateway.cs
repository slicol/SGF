using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SGF.Common;
using SGF.Network.Core;
using SGF.Utils;

namespace SGF.Network.General.Server
{
    public class Gateway
    {
        private MapList<uint, ISession> m_mapSession;
        private Socket m_SystemSocket;
        private bool m_IsRunning = false;
        private Thread m_ThreadRecv;
        private byte[] m_RecvBufferTemp = new byte[4096];
        private ISessionListener m_listener;
        private int m_port;
        private bool m_waitStart = false;

        public void Init(int port, ISessionListener listener)
        {
            Debuger.Log("port:{0}", port);
            m_port = port;
            m_listener = listener;
            m_mapSession = new MapList<uint, ISession>();

            Start();
        }

        public void Clean()
        {
            Debuger.Log();
            m_mapSession.Clear();
            Close();
        }

        public void Dump()
        {
            StringBuilder sb = new StringBuilder();

            var dic = m_mapSession.AsDictionary();
            foreach (var pair in dic)
            {
                ISession session = pair.Value;
                sb.AppendLine("\t" + session.ToString());
            }

            Debuger.LogWarning("\nGateway Sessions ({0}):\n{1}", m_mapSession.Count, sb);

        }

        private void Start()
        {
            Debuger.LogWarning("");

            m_waitStart = false;
            m_IsRunning = true;

            m_SystemSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            m_SystemSocket.Bind(IPUtils.GetIPEndPointAny(AddressFamily.InterNetwork, m_port));

            m_ThreadRecv = new Thread(Thread_Recv) { IsBackground = true };
            m_ThreadRecv.Start();
        }

        private void Close()
        {
            Debuger.LogWarning("");

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

        private void ReStart()
        {
            Debuger.LogWarning("");
            Close();
            m_waitStart = true;
        }

        public ISession GetSession(uint sid)
        {
            ISession session = null;
            lock (m_mapSession)
            {
                session = m_mapSession[sid];
            }
            return session;
        }


        //=================================================================================
        //接收线程 
        //=================================================================================
        private bool m_hasSocketException = false;

        private void Thread_Recv()
        {
            Debuger.LogWarning("Begin ......");

            while (m_IsRunning)
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
                    
                    Debuger.LogWarning("SocketErrorCode:{0}, {1}", se.SocketErrorCode,se.Message);
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

        private NetBufferReader m_RecvBufferTempReader = new NetBufferReader();

        private void DoReceiveInThread()
        {
            EndPoint remotePoint = IPUtils.GetIPEndPointAny(AddressFamily.InterNetwork, 0);
            int cnt = m_SystemSocket.ReceiveFrom(m_RecvBufferTemp, m_RecvBufferTemp.Length, SocketFlags.None, ref remotePoint);

            if (cnt > 0)
            {
                m_RecvBufferTempReader.Attach(m_RecvBufferTemp, cnt);
                byte[] m_32b = new byte[4];
                m_RecvBufferTempReader.ReadBytes(m_32b, 0, 4);
                uint sid = BitConverter.ToUInt32(m_32b, 0);
                //uint sid = m_RecvBufferTempReader.ReadUInt();
                

                lock (m_mapSession)
                {
                    ISession session = null;

                    if (sid == 0)
                    {
                        //来自Client的第1个包，只能是鉴权包
                        sid = SessionID.NewID();
                        session = new KCPSession(sid, HandleSessionSend, m_listener);
                        m_mapSession.Add(session.id, session);
                    }
                    else
                    {
                        session = m_mapSession[sid];
                    }


                    if (session != null)
                    {
                        session.Active(remotePoint as IPEndPoint);
                        session.DoReceiveInGateway(m_RecvBufferTemp, cnt);
                    }
                    else
                    {
                        Debuger.LogWarning("无效的包! sid:{0}", sid);
                    }
                }
            }
        }

        private void HandleSessionSend(ISession session, byte[] bytes, int len)
        {
            if (m_SystemSocket != null)
            {
                m_SystemSocket.SendTo(bytes, 0, len, SocketFlags.None, session.remoteEndPoint);
            }
            else
            {
                Debuger.LogError("Socket已经关闭！");
            }
        }


        private uint m_lastClearSessionTime = 0;
        public void Tick()
        {
            if (m_IsRunning)
            {
                lock (m_mapSession)
                {
                    uint current = (uint)TimeUtils.GetTotalMillisecondsSince1970();

                    if (current - m_lastClearSessionTime > KCPSession.ActiveTimeout * 1000 / 2)
                    {
                        m_lastClearSessionTime = current;
                        ClearNoActiveSession();
                    }

                    var list = m_mapSession.AsList();
                    int cnt = list.Count;
                    for (int i = 0; i < cnt; i++)
                    {
                        list[i].Tick(current);
                    }
                }
            }
            else
            {
                if (m_waitStart)
                {
                    Start();
                }
            }
        }

        private void ClearNoActiveSession()
        {
            var list = m_mapSession.AsList();
            var dir = m_mapSession.AsDictionary();
            int cnt = list.Count;

            for (int i = cnt - 1; i >= 0; i--)
            {
                var session = list[i];
                if (!session.IsActived())
                {
                    list.RemoveAt(i);
                    dir.Remove(session.id);
                }
            }
        }
    }
}
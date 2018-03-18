using System;
using System.Net;
using System.Text;
using SGF.Network.Core;
using SGF.Time;
using SGF.Utils;

namespace SGF.Network.General.Server
{
    public class KCPSession:ISession
    {
        public static int ActiveTimeout = 30;

        private uint m_id;
        private uint m_userId;
        private Action<ISession, byte[], int> m_sender;
        private ISessionListener m_listener;

        private KCP m_Kcp;
        private SwitchQueue<byte[]> m_RecvBufQueue = new SwitchQueue<byte[]>();

        public KCPSession(uint sid, Action<ISession, byte[], int> sender, ISessionListener listener)
        {
            Debuger.Log("sid:{0}", sid);

            m_id = sid;
            m_sender = sender;
            m_listener = listener;

            m_Kcp = new KCP(sid, HandleKcpSend);
            m_Kcp.NoDelay(1, 10, 2, 1);
            m_Kcp.WndSize(128, 128);


        }

        public string ToString(string prefix = "")
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("[{0}] UserId:{1}, Active:{2}, Ping:{3}, EndPoint:{4}", m_id, m_userId, m_active, ping, remoteEndPoint);
            return sb.ToString();
        }

        private void HandleKcpSend(byte[] bytes, int len)
        {
            m_sender(this, bytes, len);
        }


        public uint id { get { return m_id; } }
        public uint uid { get { return m_userId; } }
        public ushort ping { get; set; }
        public IPEndPoint remoteEndPoint { get; private set; }

        public bool IsAuth()
        {
            return m_userId > 0;

        }

        public void SetAuth(uint userId)
        {
            m_userId = userId;
        }


        private int m_lastActiveTime = 0;
        private bool m_active = false;
        public bool IsActived()
        {
            if (!m_active)
            {
                return false;
            }

            int dt = (int)SGFTime.GetTimeSinceStartup() - m_lastActiveTime;
            if (dt > ActiveTimeout)
            {
                m_active = false;
            }
            return m_active;
        }


        public void Active(IPEndPoint remoteEndPoint)
        {
            m_lastActiveTime = (int)SGFTime.GetTimeSinceStartup();
            m_active = true;

            this.remoteEndPoint = remoteEndPoint as IPEndPoint;

        }


        public bool Send(byte[] bytes, int len)
        {
            if (!IsActived())
            {
                Debuger.LogWarning("Session已经不活跃了！");
                return false;
            }

            return m_Kcp.Send(bytes, len) == 0;
        }


        public void DoReceiveInGateway(byte[] buffer, int size)
        {
            byte[] dst = new byte[size];
            Buffer.BlockCopy(buffer, 0, dst, 0, size);
            m_RecvBufQueue.Push(dst);
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
                    Debuger.LogError("收到不正确的KCP包!Ret:{0}", ret);
                    return;
                }

                m_NeedKcpUpdateFlag = true;

                for (int size = m_Kcp.PeekSize(); size > 0; size = m_Kcp.PeekSize())
                {
                    var recvBuffer = new byte[size];
                    if (m_Kcp.Recv(recvBuffer) > 0)
                    {
                        m_listener.OnReceive(this, recvBuffer, size);
                    }
                }

            }
        }


        private uint m_NextKcpUpdateTime = 0;
        private bool m_NeedKcpUpdateFlag = false;

        public void Tick(uint currentTimeMS)
        {
            DoReceiveInMain();

            uint current = currentTimeMS;

            if (m_NeedKcpUpdateFlag || current >= m_NextKcpUpdateTime)
            {
                m_Kcp.Update(current);
                m_NextKcpUpdateTime = m_Kcp.Check(current);
                m_NeedKcpUpdateFlag = false;
            }
        }


        
    }
}
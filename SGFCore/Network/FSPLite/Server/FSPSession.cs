////////////////////////////////////////////////////////////////////
//                            _ooOoo_                             //
//                           o8888888o                            //
//                           88" . "88                            //
//                           (| ^_^ |)                            //
//                           O\  =  /O                            //
//                        ____/`---'\____                         //
//                      .'  \\|     |//  `.                       //
//                     /  \\|||  :  |||//  \                      //
//                    /  _||||| -:- |||||-  \                     //
//                    |   | \\\  -  /// |   |                     //
//                    | \_|  ''\---/''  |   |                     //
//                    \  .-\__  `-`  ___/-. /                     //
//                  ___`. .'  /--.--\  `. . ___                   //
//                ."" '<  `.___\_<|>_/___.'  >'"".                //
//              | | :  `- \`.;`\ _ /`;.`/ - ` : | |               //
//              \  \ `-.   \_ __\ /__ _/   .-` /  /               //
//        ========`-.____`-.___\_____/___.-`____.-'========       //
//                             `=---='                            //
//        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^      //
//            佛祖保佑       无BUG        不修改                   //
////////////////////////////////////////////////////////////////////
/*
 * 描述：
 * 作者：slicol
*/
using System;
using System.Net;
using System.Text;
using SGF.Codec;
using SGF.Network.Core;
using SGF.Time;
using SGF.Utils;


namespace SGF.Network.FSPLite.Server
{
    public class FSPSession
    {
        public static int ActiveTimeout = 300;
        private int m_lastActiveTime;
        private bool m_active = false;
        

        private uint m_id;
        private ushort m_ping;
        private Action<IPEndPoint, byte[], int> m_sender;
        private Action<FSPDataC2S> m_listener;

        //RUDP
        private KCP m_Kcp;
        private SwitchQueue<byte[]> m_RecvBufQueue = new SwitchQueue<byte[]>();
        private uint m_NextKcpUpdateTime = 0;
        private bool m_NeedKcpUpdateFlag = false;

        //数据发送
        private byte[] m_SendBufferTemp = new byte[4096];
        private FSPDataS2C m_TempSendData = new FSPDataS2C();


        public FSPSession(uint sid, Action<IPEndPoint, byte[], int> sender)
        {
            Debuger.Log();
            m_id = sid;
            m_sender = sender;

            m_Kcp = new KCP(sid, HandleKcpSend);
            m_Kcp.NoDelay(1, 10, 2, 1);
            m_Kcp.WndSize(128, 128);

            m_active = true;
        }

        public string ToString(string prefix = "")
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("[{0}] Active:{1}, Ping:{2}, EndPoint:{3}", m_id, m_active, m_ping, RemoteEndPoint);
            return sb.ToString();
        }

        public void SetReceiveListener(Action<FSPDataC2S> listener)
        {
            Debuger.Log();
            m_listener = listener;
        }

        public uint id { get { return m_id; } }
        public ushort ping { get { return m_ping; } set { m_ping = value; } }
        public IPEndPoint RemoteEndPoint { get; private set; }
        public bool IsEndPointChanged { get; set; }


        public void Active(IPEndPoint remoteEndPoint)
        {
            m_lastActiveTime = (int)SGFTime.GetTimeSinceStartup();
            m_active = true;

            if (this.RemoteEndPoint == null || !this.RemoteEndPoint.Equals(remoteEndPoint))
            {
                IsEndPointChanged = true;
                this.RemoteEndPoint = remoteEndPoint;
            }
            
        }

        public void Active(bool value)
        {
            m_active = value;
        }

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


        //==================================================================
        //发送数据
        //==================================================================
        public bool Send(FSPFrame frame)
        {
            if (!IsActived())
            {
                Debuger.LogWarning("Session已经不活跃了！");
                return false;
            }

            m_TempSendData.frames.Clear();
            m_TempSendData.frames.Add(frame);
            int len = PBSerializer.NSerialize(m_TempSendData, m_SendBufferTemp);
            return m_Kcp.Send(m_SendBufferTemp, len) == 0;
        }

        private void HandleKcpSend(byte[] buffer, int size)
        {
            m_sender(RemoteEndPoint, buffer, size);
        }

        //==================================================================
        //接收数据
        //==================================================================
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
                        if (m_listener != null)
                        {
                            FSPDataC2S data = PBSerializer.NDeserialize<FSPDataC2S>(recvBuffer);
                            m_listener(data);
                        }
                        else
                        {
                            Debuger.LogError("找不到接收者！");
                        }
                    }
                }
            }
        }


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
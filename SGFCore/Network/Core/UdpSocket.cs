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
using System.Net.Sockets;



namespace SGF.Network.Core
{
    public class UdpSocket
    {


        //===========================================================
        public static string LOG_TAG = "UdpSocket";

        private bool m_IsActive;
        private Socket m_SystemSocket;
        private AddressFamily m_AddrFamily;
        private bool m_IsBroadcast;
        private bool m_EnableBlockOnRecv = false;//是否允许在接收数据时阻塞


        //===========================================================
        #region 构造与析构
        public UdpSocket(AddressFamily family, bool enableBlockOnRecv)
        {
            m_EnableBlockOnRecv = enableBlockOnRecv;
            m_AddrFamily = family;
            m_SystemSocket = new Socket(m_AddrFamily, SocketType.Dgram, ProtocolType.Udp);
        }

        public AddressFamily AddressFamily { get { return m_AddrFamily; } }

        public void Dispose()
        {
            Close();
        }


        public void Close()
        {
            if (m_SystemSocket != null)
            {
                try
                {
                    m_SystemSocket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception e)
                {
                    Debuger.LogWarning(LOG_TAG, "Close() " + e.Message + e.StackTrace);
                }

                m_SystemSocket.Close();
                m_SystemSocket = null;
            }

            m_IsActive = false;
            GC.SuppressFinalize(this);
        }

      

        public string SelfIP
        {
            get
            {
				return IPUtils.SelfIP; 
                
            }
        }

        public int SelfPort
        {
            get
            {
                IPEndPoint ipep = m_SystemSocket.LocalEndPoint as IPEndPoint;
                return ipep.Port;
            }
        }

        public Socket SystemSocket { get { return m_SystemSocket; } }

        #endregion

        //------------------------------------------------------------
#region 绑定端口函数
        public int Bind(int port = 0)
        {
            Debuger.Log(LOG_TAG, "Bind() port = " + port);
            if (m_SystemSocket == null)
            {
                return 0;
            }

            //如果Bind的端口为0，则会随机分配一个端口
            IPEndPoint ipep = IPUtils.GetIPEndPointAny(m_AddrFamily, port);
            m_SystemSocket.Bind(ipep);
            m_IsActive = true;
            return SelfPort;
        }


        #endregion


        //------------------------------------------------------------
#region ReceiveFrom和SendTo函数
        public int ReceiveFrom(byte[] buffer, int maxsize, ref IPEndPoint remoteEP)
        {
            int cnt = 0;

            EndPoint ip = null;

            if (!m_EnableBlockOnRecv)
            {
                if (m_SystemSocket.Available <= 0)
                {
                    return 0;
                }
            }


            if (m_AddrFamily == AddressFamily.InterNetwork)
            {
                //如果是IPv4环境，则(与Android的处理一样)
                ip = IPUtils.GetIPEndPointAny(AddressFamily.InterNetwork, 0);
                cnt = m_SystemSocket.ReceiveFrom(buffer, maxsize, SocketFlags.None, ref ip);

                if (cnt > 0 && remoteEP != null && !remoteEP.Equals(ip))
                {
                    Debuger.LogWarning(LOG_TAG, "ReceiveFrom() 收到一个自来陌生IP:Port(" + ip + ")的数据包！");
                    return 0;
                }
            }
            else
            {
                //如果是IPv6环境，则：
                ip = remoteEP;
                cnt = m_SystemSocket.ReceiveFrom(buffer, maxsize, SocketFlags.None, ref ip);
            }

            remoteEP = ip as IPEndPoint;

            if (NetDebuger.IsPacketLoss())
            {
                return 0;
            }

            return cnt;
        }




        public int SendTo(byte[] buffer, int size, IPEndPoint remoteEP)
        {
            int cnt = 0;

            if (NetDebuger.IsPacketLoss())
            {
                return 0;
            }

            //如果无法使用LocalSocket再使用SystemSocket
            if (cnt == 0)
            {
                cnt = m_SystemSocket.SendTo(buffer, 0, size, SocketFlags.None, remoteEP);
            }

            return cnt;
        }
        #endregion
    }
}


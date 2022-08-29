using SGF;
using SGF.Extension;
using SGF.Network.General;
using SGF.Network.General.Client;
using SGFAppDemo.Common.Data;
using SGFAppDemo.Common.Proto;

namespace SGFAppDemo.Services.Online
{
    public class ServerTestBot
    {
        private NetManager m_net;
        private bool m_connected = false;
        private UserData m_mainUserData = null;
        private HeartBeatHandler m_heartbeat;


        public void Init()
        {
            m_net = new NetManager();
            //m_net.Init(typeof(KcpConnection), (int)ConnID.ZoneServer,0);
            //m_net.RegisterRPCListener(this);

            m_net.Init(ConnectionType.RUDP, 0);
            m_net.Rpc.RegisterListener(this);

            m_heartbeat = new HeartBeatHandler();
            m_heartbeat.Init(m_net);
        }

        public void Connect()
        {
            if (!m_connected)
            {
                m_net.Connect("111.230.116.185", 4540);
                //m_net.Connect("127.0.0.1", 4540);
                //m_connected = m_net.Connected;
                m_connected = m_net.IsConnected;
            }
        }

        public void Update()
        {
            m_net.Tick();
        }

        public void Login(string username)
        {
            Connect();

            LoginReq req = new LoginReq();
            req.name = username;

            //m_net.Send<LoginReq, LoginRsp>(ProtoCmd.LoginReq, req, OnLoginRsp);
            m_net.Send<LoginRsp>(ProtoCmd.LoginReq, req, OnLoginRsp);
        }

        public void Logout()
        {
            m_heartbeat.Stop();
            if (m_mainUserData != null)
            {
                //m_net.Invoke("Logout");
                m_net.Rpc.Invoke("Logout");
            }
            m_mainUserData = null;
        }

        private void OnLogout()
        {
            Debuger.Log();
        }

        private void OnLoginRsp(uint index, LoginRsp rsp)
        {
            if (rsp.ret.code == 0)
            {
                Debuger.Log("UserData:{0}", rsp.userdata);
                m_mainUserData = rsp.userdata;
                m_heartbeat.Start();
            }
            else
            {
                Debuger.LogWarning(rsp.ret.info);
            }
        }
    }
}
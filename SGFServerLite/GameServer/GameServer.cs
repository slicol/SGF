using SGF.IPCWork;
using SGF.Network.General;
using SGF.Network.General.Server;
using SGF.Server;


namespace SGFServerDemo.GameServer
{
    public class GameServer:ServerModule
    {
        private NetManager m_net;
        private IPCManager m_ipc;

        public override void Start()
        {
            base.Start();

            m_net = new NetManager();
            m_net.Init(ConnectionType.RUDP, 2939);

            m_ipc = new IPCManager();
            m_ipc.Init(id);

        }


        public override void Tick()
        {
            m_net.Tick();
            m_ipc.Tick();
        }


 
    }
}
using System;
using SGF;
using SGF.IPCWork;
using SGF.Network.General;
using SGF.Network.General.Server;
using SGF.Server;
using SGF.Utils;

namespace SGFServerDemo.ZoneServer
{
    

    public class ZoneServer : ServerModule
    {
        private NetManager m_net;
        private IPCManager m_ipc;
        private ServerContext m_context;

        public override void Start()
        {
            base.Start();

            m_net = new NetManager();
            m_net.Init(ConnectionType.RUDP, 4540);

            m_ipc = new IPCManager();
            m_ipc.Init(id);

            m_context = new ServerContext();
            m_context.net = m_net;
            m_context.ipc = m_ipc;

            //业务逻辑初始化
            OnlineManager.Instance.Init(m_context);
            RoomManager.Instance.Init(m_context);

            ConsoleInput.onInputLine.AddListener(OnInputLine);
            ConsoleInput.onInputKey.AddListener(OnInputKey);
        }


        public override void Tick()
        {
            m_net.Tick();
            m_ipc.Tick();
        }

        private void OnInputKey(ConsoleKey key)
        {
            Debuger.Log(key);
            if (key == ConsoleKey.F1)
            {
                m_net.Dump();
            }
            else if (key == ConsoleKey.F2)
            {
                OnlineManager.Instance.Dump();
            }
            else if (key == ConsoleKey.F3)
            {
                RoomManager.Instance.Dump();
            }
            else if (key == ConsoleKey.F4)
            {
                m_net.Dump();
                OnlineManager.Instance.Dump();
                RoomManager.Instance.Dump();
            }
        }

        private void OnInputLine(string line)
        {
            Debuger.Log(line);
            if (line == "dump")
            {
                m_net.Dump();
                OnlineManager.Instance.Dump();
                RoomManager.Instance.Dump();
                
            }
        }
    }

}
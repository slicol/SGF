using System.Collections.Generic;

namespace SGFAppDemo.Services.Online
{
    public class ServerProfiler
    {
        private static List<ServerTestBot> m_listBot = new List<ServerTestBot>();
        private static bool m_start = false;
        private static bool m_inited = false;
        public static void Init()
        {
            if (m_inited)
            {
                return;
            }

            m_inited = true;

            GlobalEvent.onUpdate.AddListener(OnUpdate);
            if (m_listBot.Count == 0)
            {
                for (int i = 0; i < 1000; i++)
                {
                    var bot = new ServerTestBot();
                    m_listBot.Add(bot);
                    bot.Init();
                }
            }
        }

        public static void Start()
        {
            if (!m_inited)
            {
                return;
            }

            if (m_start)
            {
                return;
            }

            m_start = true;
            for (int i = 0; i < m_listBot.Count; i++)
            {
                m_listBot[i].Login("TestName#" + i);
            }
        }

        public static void Stop()
        {
            if (!m_inited)
            {
                return;
            }

            if (!m_start)
            {
                return;
            }

            m_start = false;
            for (int i = 0; i < m_listBot.Count; i++)
            {
                m_listBot[i].Logout();
            }
        }

        private static void OnUpdate(float dt)
        {
            if (!m_inited)
            {
                return;
            }

            for (int i = 0; i < m_listBot.Count; i++)
            {
                m_listBot[i].Update();
            }


        }
    }
}
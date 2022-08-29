using System;
using SGF;
using SGF.Utils;

namespace SGFAppDemo.Framework
{
    public class SGFGameMainLoop
    {
        private static bool m_run = false;

        public static void Run()
        {
            float deltaTimeMS = 0;
            long m_LogicLastTicks = DateTime.Now.Ticks;
            long nowticks = DateTime.Now.Ticks;
            long interval = 0;
            long fixedFrameInterval = 0;

            ConsoleInput.onInputKey.AddListener(OnInputKey);
            
            m_run = true;

            while (m_run)
            {
                ConsoleInput.Tick();
                

                fixedFrameInterval = SGFGameDefine.FRAME_TICK_INTERVAL;
                nowticks = DateTime.Now.Ticks;
                interval = nowticks - m_LogicLastTicks;

                if (interval >= fixedFrameInterval)
                {
                    deltaTimeMS = interval / 10000.0f;
                    SGFGameObject[] list;
                    int cnt = 0;

                    while (interval >= fixedFrameInterval)
                    {
                        interval -= fixedFrameInterval;
                        list = SGFGameObject.GetActivedGameObjects();
                        cnt = list.Length;
                        for (int i = 0; i < cnt; i++)
                        {
                            list[i].FixedUpdate();
                        }

                        GlobalEvent.onFixedUpdate.Invoke();
                    }

                    list = SGFGameObject.GetActivedGameObjects();
                    cnt = list.Length;
                    for (int i = 0; i < cnt; i++)
                    {
                        list[i].Update(deltaTimeMS);
                    }

                    GlobalEvent.onUpdate.Invoke(deltaTimeMS);

                    m_LogicLastTicks = nowticks - interval;
                }

                
            }
        }

        private static void OnInputKey(ConsoleKey key)
        {
            if (key == ConsoleKey.Escape)
            {
                m_run = false;
            }
        }
    }
}
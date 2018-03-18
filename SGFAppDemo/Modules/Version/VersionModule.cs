using System;
using SGF;
using SGF.Module;
using SGF.Time;

namespace SGFAppDemo.Modules
{
    public class VersionModule : GeneralModule
    {
        private float m_progress = 0;
        protected override void Show(object arg)
        {
            base.Show(arg);

            GlobalEvent.onUpdate.AddListener(OnUpdate);
        }

        private void OnUpdate(float deltaTime)
        {
            m_progress += 0.1f;
            if (m_progress > 1)
            {
                m_progress = 1;
            }

            Console.Write("模拟版本更新:" + (int)(m_progress * 100) + "%\r");

            if (m_progress >= 1)
            {
                Console.WriteLine();
                GlobalEvent.onUpdate.RemoveListener(OnUpdate);
                GlobalEvent.onVersionUpdateComplete.Invoke();
            }
        }
    }
}
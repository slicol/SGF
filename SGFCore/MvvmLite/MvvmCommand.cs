using SGF.Timers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SGF.MvvmLite
{
    public abstract class MvvmCommandBase
    {
        //====================================================================
        #region ////MvvmCommand Async////
        private static Timer m_timerAsync;
        private static List<MvvmCommandBase> m_listAsyncCmds = new List<MvvmCommandBase>();
        
        protected static void PumpAsyncCommand(MvvmCommandBase cmd)
        {
            m_listAsyncCmds.Add(cmd);
            PumpAsync();
        }

        private static void PumpAsync()
        {
            if (m_timerAsync == null)
            {
                m_timerAsync = new Timer();
                m_timerAsync.Interval = 1;
                m_timerAsync.AutoReset = false;
                m_timerAsync.Elapsed += (sender, e) => TickAsync();
            }

            m_timerAsync.Enabled = true;
        }

        private static void TickAsync()
        {
            var cmds = m_listAsyncCmds.ToArray();
            m_listAsyncCmds.Clear();

            for (int i = 0; i < cmds.Length; ++i)
            {
                cmds[i].Execute();
                Debuger.LogVerbose("{0} End----------------", cmds[i].GetType().Name);
            }
        }

        #endregion
        //====================================================================

        protected abstract void Execute();
    }

    public class MvvmCommand<T>: MvvmCommandBase where T:MvvmCommandBase, new ()
    {
        protected MvvmViewModel m_vm;
        

        public static void Send(MvvmViewModel vm)
        {
            MvvmCommand<T> cmd = (new T()) as MvvmCommand<T>;
            cmd.m_vm = vm;

            Debuger.LogVerbose("{0} Begin--------------", typeof(T).Name);
            cmd.Execute();
            Debuger.LogVerbose("{0} End----------------", typeof(T).Name);
        }

        public static void SendAsync(MvvmViewModel vm)
        {
            MvvmCommand<T> cmd = (new T()) as MvvmCommand<T>;
            cmd.m_vm = vm;

            Debuger.LogVerbose("{0} Begin--------------", typeof(T).Name);
            PumpAsyncCommand(cmd);
        }

        protected override void Execute()
        {
            throw new NotImplementedException();
        }
    }
}

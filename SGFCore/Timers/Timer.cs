using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace SGF.Timers
{
    public delegate void ElapsedEventHandler(object sender, ElapsedEventArgs e);

    public class ElapsedEventArgs : EventArgs
    {
        private DateTime signalTime;
        public DateTime SignalTime { get { return this.signalTime; } }

        internal ElapsedEventArgs()
        {
            signalTime = DateTime.Now;
        }
    }

    public class Timer:IDisposable
    {
        //=========================================================================
        #region ////Timer Manager////
        private static List<Timer> m_timers = new List<Timer>();
        private static long m_lastTicks = 0;
        private static void AddTimer(Timer timer)
        {
            if(!m_timers.Contains(timer))
            {
                m_timers.Add(timer);
            }
        }

        private static void RemoveTimer(Timer timer)
        {
            try
            {
                m_timers.Remove(timer);
            }
            catch { }
        }

        public static void Tick()
        {
            var now = DateTime.Now.Ticks;
            var dt = now - m_lastTicks;
            m_lastTicks = now;
            Tick(GetMillisecondsFromTicks(dt));
        }


        public static void Tick(double dt)
        {
            var timers = m_timers.ToArray();
            for(int i =0;i < timers.Length; ++i)
            {
                timers[i].TickInternal(dt);
            }
        }


        public static double GetMillisecondsFromTicks(long ticks)
        {
            return ticks / 10000.0f;
        }

        #endregion
        //=========================================================================

        private bool m_disposed = false;
        private double m_elapsed = 0;
        private double m_interval;
        private int m_count;

        private bool m_enabled;
        private bool m_autoReset = false;

        public ElapsedEventHandler onIntervalElapsed;
        
        public Timer()
        {
            m_interval = 100;
            m_enabled = false;
            m_autoReset = true;
        }

        public Timer(double interval)
        {
            if (interval <= 0)
                interval = 0;

            double roundedInterval = Math.Ceiling(interval);
            if (roundedInterval > Int32.MaxValue || roundedInterval <= 0)
            {
                roundedInterval = 0;
            }

            this.m_interval = (int)roundedInterval;
        }

        ~Timer()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (m_disposed) return;
            m_disposed = true;

            Close();
        }


        public void Close()
        {
            m_enabled = false;
            RemoveTimer(this);
        }



        public bool AutoReset
        {
            get
            {
                return this.m_autoReset;
            }

            set
            {
                if (this.m_autoReset != value)
                {
                    this.m_autoReset = value;
                    if(value)
                    {
                        Enabled = true;
                    }
                }
            }
        }


        public bool Enabled
        {
            get
            {
                return m_enabled;
            }

            set
            {
                if (m_enabled != value)
                {
                    m_enabled = value;

                    if (!value)
                    {
                        RemoveTimer(this);
                    }
                    else
                    {
                        AddTimer(this);
                    }
                }
            }
        }

        public double Interval
        {
            get
            {
                return this.m_interval;
            }

            set
            {
                m_interval = value;
            }
        }


        public event ElapsedEventHandler Elapsed
        {
            add
            {
                onIntervalElapsed += value;
            }
            remove
            {
                onIntervalElapsed -= value;
            }
        }


        public void Start()
        {
            Enabled = true;
        }

        public void Stop()
        {
            Enabled = false;
        }

        internal void TickInternal(double dt)
        {
            m_elapsed += dt;
            if(m_elapsed >= m_interval)
            {
                m_elapsed -= m_interval;

                if (!this.m_autoReset)
                {
                    Enabled = false;
                }

                try
                {
                    ElapsedEventHandler intervalElapsed = this.onIntervalElapsed;
                    if (intervalElapsed != null)
                    {
                        ElapsedEventArgs elapsedEventArgs = new ElapsedEventArgs();
                        intervalElapsed(this, elapsedEventArgs);
                    }
                }
                catch
                {

                }
            }
        }


    }
}



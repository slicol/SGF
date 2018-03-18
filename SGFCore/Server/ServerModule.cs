namespace SGF.Server
{
    public class ServerModule:ILogTag
    {
        private ServerModuleInfo m_info;

        public int id { get { return m_info.id; } }
        public int port { get { return m_info.port; } }


        internal void Create(ServerModuleInfo info)
        {
            m_info = info;
            LOG_TAG = this.GetType().Name + "[" + info.id + "," + info.port + "]";
            this.Log();
        }

        internal void Release()
        {
            this.Log();
        }

        public virtual void Start()
        {
            this.Log();
        }

        public virtual void Stop()
        {
            this.Log();
        }

        public virtual void Tick()
        {
            
        }

        public string LOG_TAG { get; private set; }
    }
}
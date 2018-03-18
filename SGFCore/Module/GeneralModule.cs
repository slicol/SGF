using System.Reflection;

namespace SGF.Module
{
    public class GeneralModule:ModuleBase, ILogTag
    {
        private string m_name = "";
        public string Name { get { return m_name; } }

        public string Title;

        public GeneralModule()
        {
            m_name = this.GetType().Name;
            LOG_TAG = m_name;
        }


        /// <summary>
        /// 调用它以创建模块
        /// </summary>
        /// <param name="args"></param>
        public virtual void Create(object args = null)
        {
            this.Log("args:{0}", args);

            
        }

        /// <summary>
        /// 调用它以释放模块
        /// </summary>
        public override void Release()
        {
            base.Release();
            this.Log();
        }


        /// <summary>
        /// 当模块收到消息后，对消息进行一些处理
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="args"></param>
        internal void HandleMessage(string msg, object[] args)
        {
            this.Log("msg:{0}, args:{1}", msg, args);

            var mi = this.GetType()
                .GetMethod(msg, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (mi != null)
            {
                mi.Invoke(this, BindingFlags.NonPublic, null, args, null);
            }
            else
            {
                OnModuleMessage(msg, args);
            }

        }


        /// <summary>
        /// 由派生类去实现，用于处理消息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="args"></param>
        protected virtual void OnModuleMessage(string msg, object[] args)
        {
            this.Log("msg:{0}, args{1}", msg, args);

        }


        protected virtual void Show(object arg)
        {
            this.Log("args:{0}", arg);
        }


        public string LOG_TAG { get; protected set; }
    }
}
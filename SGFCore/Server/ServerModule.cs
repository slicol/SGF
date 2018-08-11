/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
 * 服务器（模块）管理器
 * Server (module) manager
 * 
 * Licensed under the MIT License (the "License"); 
 * you may not use this file except in compliance with the License. 
 * You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, 
 * software distributed under the License is distributed on an "AS IS" BASIS, 
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. 
 * See the License for the specific language governing permissions and limitations under the License.
*/

using System.Reflection;

namespace SGF.Server
{
    public class ServerModule:ILogTag
    {
        private ServerModuleInfo m_info;
        private object m_msgHandler;

        public int id { get { return m_info.id; } }
        public int port { get { return m_info.port; } }


        internal void Create(ServerModuleInfo info)
        {
            m_info = info;
            m_msgHandler = this;

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

        public void BindModuleMessageHandler(object handler)
        {
            m_msgHandler = handler == null ? this : handler;
        }

        /// <summary>
        /// 当模块收到消息后，对消息进行一些处理
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="args"></param>
        internal void HandleMessage(string msg, object[] args)
        {
            this.Log("msg:{0}, args:{1}", msg, args);

            var mi = m_msgHandler.GetType().GetMethod(msg, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (mi != null)
            {
                mi.Invoke(m_msgHandler, BindingFlags.NonPublic, null, args, null);
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


        public string LOG_TAG { get; private set; }
    }
}
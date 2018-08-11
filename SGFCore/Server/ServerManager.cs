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

using System;
using System.Collections.Generic;
using SGF.Utils;


namespace SGF.Server
{
    public class ModuleMessageHandlerAttribute : Attribute
    {

    }

    public class ServerManager:Singleton<ServerManager>
    {
        class MessageObject
        {
            public int target;
            public string msg;
            public object[] args;
        }

        private MapList<int, ServerModule> m_mapModule = new MapList<int, ServerModule>();
        private Dictionary<int, List<MessageObject>> m_mapCacheMessage = new Dictionary<int, List<MessageObject>>();
        private Queue<MessageObject> m_queueMessage = new Queue<MessageObject>();

        public void Init(string _namespace)
        {
            ServerConfig.Namespace = _namespace;
        }

        public void StartAutoServer()
        {
            ServerConfig.Load();

            var list = ServerConfig.GetServerModuleInfoList();
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i].auto)
                {
                    StartServer(list[i]);
                }
            }
        }

        public void StartServer(int id)
        {
            Debuger.Log(id);

            ServerModuleInfo info = ServerConfig.GetServerModuleInfo(id);

            StartServer(info);
        }

        public bool StartServer(ServerModuleInfo info)
        {
            string fullName = ServerConfig.Namespace + "." + info.name + "." + info.name;
            Debuger.Log(fullName);

            try
            {
                Type type = Type.GetType(fullName + "," + info.assembly);
                
                var module = Activator.CreateInstance(type) as ServerModule;

                if (module != null)
                {
                    module.Create(info);
                    m_mapModule.Add(info.id, module);

                    module.Start();

                    ServerConfig.SetServerModuleInfo(info);

                    //处理缓存的消息
                    if (m_mapCacheMessage.ContainsKey(info.id))
                    {
                        List<MessageObject> list = m_mapCacheMessage[info.id];
                        for (int i = 0; i < list.Count; i++)
                        {
                            MessageObject msgobj = list[i];
                            module.HandleMessage(msgobj.msg, msgobj.args);
                        }
                        m_mapCacheMessage.Remove(info.id);
                    }

                    return true;
                }
            }
            catch (Exception e)
            {
                m_mapModule.Remove(info.id);
                Debuger.LogError("ServerModule[{0}] Create Or Start Error:{1}", info.name,
                    e.Message + "\n" + e.StackTrace);
            }
            return false;
        }

        public void StopServer(int id)
        {
            Debuger.Log(id);
            var moudule = m_mapModule[id];
            if (moudule != null)
            {
                moudule.Stop();
                moudule.Release();
                m_mapModule.Remove(id);
            }
        }

        public void StopAllServer()
        {
            Debuger.Log();
            var list = m_mapModule.AsList();
            int cnt = list.Count;
            for (int i = 0; i < cnt; i++)
            {
                list[i].Stop();
                list[i].Release();
            }
            m_mapModule.Clear();
        }

        public void Tick()
        {
            //Post 消息
            //不管消息 来自哪个线程，都应该由主线程处理@Slicol
            lock (m_queueMessage)
            {
                while (m_queueMessage.Count > 0)
                {
                    var obj = m_queueMessage.Dequeue();
                    var m = GetModule(obj.target);
                    if (m != null)
                    {
                        m.HandleMessage(obj.msg, obj.args);
                    }
                    else
                    {
                        Debuger.LogWarning("消息被丢弃! target:{0}, msg:{1}", obj.target, obj.msg);
                    }
                }
            }



            var list = m_mapModule.AsList();
            int cnt = list.Count;

            for (int i = 0; i < cnt; i++)
            {
                list[i].Tick();
            }
        }

        public bool HasModule(int id)
        {
            return m_mapModule.ContainsKey(id);
        }

        public ServerModule GetModule(int id)
        {
            ServerModule module = m_mapModule[id];
            return module;
        }

        //============================================================================
        //消息机制
        //============================================================================

        public void SendMessage(int target, string msg, params object[] args)
        {
            ServerModule module = GetModule(target);
            if (module != null)
            {
                module.HandleMessage(msg, args);
            }
            else
            {
                var list = GetCacheMessageList(target);
                MessageObject obj = new MessageObject();
                obj.msg = msg;
                obj.args = args;
                obj.target = target;
                list.Add(obj);

            }
        }

        public void PostMessage(int target, string msg, params object[] args)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(_ => PostMessageInternal(target, msg, args));
        }

        private void PostMessageInternal(int target, string msg, object[] args)
        {
            MessageObject obj = new MessageObject();
            obj.msg = msg;
            obj.args = args;
            obj.target = target;

            lock (m_queueMessage)
            {
                m_queueMessage.Enqueue(obj);
            }
        }

        

        private List<MessageObject> GetCacheMessageList(int target)
        {
            List<MessageObject> list = null;
            if (!m_mapCacheMessage.TryGetValue(target, out list))
            {
                list = new List<MessageObject>();
                m_mapCacheMessage.Add(target, list);
            }

            return list;
        }


    }
}
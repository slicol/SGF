using System;
using System.Collections.Generic;
using SGF.Common;

namespace SGF.Module
{
    public class ModuleManager:Singleton<ModuleManager>
    {
        class MessageObject
        {
            public string msg;
            public object[] args;
        }

        private Dictionary<string, GeneralModule> m_mapModules;

        private Dictionary<string, List<MessageObject>> m_mapCacheMessage;

        private List<IModuleActivator> m_listModuleActivators;


        public ModuleManager()
        {
            m_mapModules = new Dictionary<string, GeneralModule>();
            m_mapCacheMessage = new Dictionary<string, List<MessageObject>>();
            m_listModuleActivators = new List<IModuleActivator>();
        }

        public void Init()
        {
            Debuger.Log();
        }


        public void Clean()
        {
            Debuger.Log();

            m_mapCacheMessage.Clear();

            foreach (var pair in m_mapModules)
            {
                pair.Value.Release();
            }
            m_mapModules.Clear();

            m_listModuleActivators.Clear();
        }


        public void RegisterModuleActivator(IModuleActivator activator)
        {
            if (!m_listModuleActivators.Contains(activator))
            {
                m_listModuleActivators.Add(activator);
            }
            
        }


        public GeneralModule CreateModule(string name, object args = null)
        {
            Debuger.Log("name = " + name + ", args = " + args);

            if (HasModule(name))
            {
                Debuger.LogError("The Module<{0}> Has Existed!");
                return null;
            }

            GeneralModule module = null;

            for (int i = 0; i < m_listModuleActivators.Count; i++)
            {
                module = m_listModuleActivators[i].CreateInstance(name);
                if (module != null)
                {
                    break;
                }
            }
            

            if (module == null)
            {
                Debuger.LogError("模块实例化失败!");
                return null;
            }

            m_mapModules.Add(name, module);
            module.Create(args);

            //处理缓存的消息
            if (m_mapCacheMessage.ContainsKey(name))
            {
                List<MessageObject> list = m_mapCacheMessage[name];
                for (int i = 0; i < list.Count; i++)
                {
                    MessageObject msgobj = list[i];
                    module.HandleMessage(msgobj.msg, msgobj.args);
                }
                m_mapCacheMessage.Remove(name);
            }

            return module;
        }


        public bool HasModule(string name)
        {
            return m_mapModules.ContainsKey(name);
        }

        public GeneralModule GetModule(string name)
        {
            GeneralModule module = null;
            m_mapModules.TryGetValue(name, out module);
            return module;
        }



        //============================================================================
        //消息机制
        //============================================================================

        public void SendMessage(string target, string msg, params object[] args)
        {
            GeneralModule module = GetModule(target);
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
                list.Add(obj);
                
            }
        }

        private List<MessageObject> GetCacheMessageList(string target)
        {
            List<MessageObject> list = null;
            if (!m_mapCacheMessage.TryGetValue(target, out list))
            {
                list = new List<MessageObject>();
                m_mapCacheMessage.Add(target, list);
            }

            return list;
        }


        public void ShowModule(string target, object arg = null)
        {
            SendMessage(target, "Show", arg);
        }

    }
}
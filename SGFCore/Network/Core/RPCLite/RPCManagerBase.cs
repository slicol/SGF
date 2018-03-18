using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using SGF.Common;

namespace SGF.Network.Core.RPCLite
{

    public class RPCRequestAttribute : Attribute
    {

    }

    public class RPCResponseAttribute : Attribute
    {

    }

    public class RPCNotifyAttribute : Attribute
    {
        
    }


    public class RPCMethodHelper
    {
        public object listener;
        public MethodInfo method;

        
    }

    public class RPCManagerBase
    {
        protected List<object> m_listListener;
        protected DictionarySafe<string, RPCMethodHelper> m_mapMethodHelper;


        public void Init()
        {
            m_listListener = new List<object>();
            m_mapMethodHelper = new DictionarySafe<string, RPCMethodHelper>();
        }

        public void Clean()
        {
            m_listListener.Clear();

            foreach (var pair in m_mapMethodHelper)
            {
                pair.Value.listener = null;
                pair.Value.method = null;
            }
            m_mapMethodHelper.Clear();
        }

        public void Dump()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var pair in m_mapMethodHelper)
            {
                RPCMethodHelper helper = pair.Value;
                sb.AppendFormat("\t<name:{0}, \tmethod:{1}.{2}>\n", pair.Key,
                    helper.method.DeclaringType.Name, helper.method.Name);
            }

            Debuger.LogWarning("\nRPC Cached Methods ({0}):\n{1}", m_mapMethodHelper.Count, sb);
        }


        public void RegisterListener(object listener)
        {
            if (!m_listListener.Contains(listener))
            {
                Debuger.Log("{0}", listener.GetType().Name);
                m_listListener.Add(listener);
            }
        }

        public void UnRegisterListener(object listener)
        {
            if (m_listListener.Contains(listener))
            {
                Debuger.Log("{0}", listener.GetType().Name);
                m_listListener.Remove(listener);
            }

            List<string> listMethods = new List<string>();

            foreach (var helper in m_mapMethodHelper)
            {
                if (helper.Value.listener == listener)
                {
                    listMethods.Add(helper.Key);
                }
            }
            for (int i = 0; i < listMethods.Count; i++)
            {
                m_mapMethodHelper.Remove(listMethods[i]);
            }
        }
        public RPCMethodHelper GetMethodHelper(string name)
        {
            var helper = m_mapMethodHelper[name];
            if (helper == null)
            {
                MethodInfo mi = null;
                object listener = null;
                for (int i = 0; i < m_listListener.Count; i++)
                {
                    listener = m_listListener[i];
                    mi = listener.GetType().GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                    if (mi != null)
                    {
                        break;
                    }
                }

                if (mi != null)
                {
                    helper = new RPCMethodHelper();
                    helper.listener = listener;
                    helper.method = mi;
                    m_mapMethodHelper.Add(name, helper);
                }
            }

            return helper;
        }

    }
}
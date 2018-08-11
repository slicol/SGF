/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
 * 实现基于C#反射机制的RPC基础功能
 * 它需要结合具体的通讯模块使用
 * Implement RPC basic functions based on C# reflection mechanism
 * It needs to be used in conjunction with specific communication modules
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

using SGF.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;


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
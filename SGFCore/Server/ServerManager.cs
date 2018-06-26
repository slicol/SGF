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
using SGF.Common;
using SGF.Utils;


namespace SGF.Server
{
    public class ServerManager:Singleton<ServerManager>
    {
        private MapList<int, ServerModule> m_mapModule = new MapList<int, ServerModule>();

        public void Init(string _namespace)
        {
            ServerConfig.Namespace = _namespace;
        }

        public void StartServer(int id)
        {
            Debuger.Log(id);

            ServerModuleInfo info = ServerConfig.GetServerModuleInfo(id);
            string fullName = ServerConfig.Namespace + "." + info.name + "." + info.name;

            try
            {
                Type type = Type.GetType(fullName + "," + info.assembly);
                var module = Activator.CreateInstance(type) as ServerModule;

                if (module != null)
                {
                    module.Create(info);
                    m_mapModule.Add(module.id, module);

                    module.Start();
                }
            }
            catch (Exception e)
            {
                Debuger.LogError(e.Message);
            }
            
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
            var list = m_mapModule.AsList();
            int cnt = list.Count;

            for (int i = 0; i < cnt; i++)
            {
                list[i].Tick();
            }
        }

    }
}
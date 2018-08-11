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
using MiniJSON.Safe;
using SGF.Utils;

namespace SGF.Server
{
    public class ServerModuleInfo
    {
        public int id;
        public string name;
        public string assembly;
        public string ip;
        public int port;
        public bool auto;
    }

    public class ServerConfig
    {
        public static string Namespace = "SGF.Server";
        internal readonly static string DefaultPath = "./ServerConfig.json";

        private readonly static MapList<int, ServerModuleInfo> MapServerModuleInfo = new MapList<int, ServerModuleInfo>();

        public static void Load(string path = null)
        {
            MapServerModuleInfo.Clear();
            if (string.IsNullOrEmpty(path))
            {
                path = DefaultPath;
            }
            ReadConfig(path);
        }

        public static ServerModuleInfo GetServerModuleInfo(int id)
        {
            if (MapServerModuleInfo.Count == 0)
            {
                ReadConfig(DefaultPath);
            }

            return MapServerModuleInfo[id];
        }

        public static void SetServerModuleInfo(ServerModuleInfo info)
        {
            MapServerModuleInfo[info.id] = info;
        }

        public static ServerModuleInfo[] GetServerModuleInfoList()
        {
            return MapServerModuleInfo.ToArray();
        }

        private static void ReadConfig(string path)
        {
            Debuger.Log();
            string jsonStr = FileUtils.ReadString(path);
            List<object> obj = null;
            try
            {
                obj = Json.Deserialize(jsonStr) as List<object>;
            }
            catch (Exception e)
            {
                Debuger.LogError("文件加载失败：{0}", path);
                return;
            }
            
            for (int i = 0; i < obj.Count; i++)
            {
                var infoJson = obj[i] as Dictionary<string, object>;
                ServerModuleInfo info = new ServerModuleInfo();
                info.id = (int)infoJson["id"];
                info.name = (string)infoJson["name"];
                info.assembly = (string)infoJson["assembly"];
                info.ip = (string)infoJson["ip"];
                info.port = (int)infoJson["port"];
                info.auto = (bool)infoJson["auto"];
                MapServerModuleInfo.Add(info.id, info);
            }
        }

    }
}
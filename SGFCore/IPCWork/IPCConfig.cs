/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
 * 进程通讯模块。
 * Inter-process communication module.
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

using System.Collections.Generic;

using SGF.Utils;

namespace SGF.IPCWork
{

    public class IPCInfo
    {
        public int id;
        public int port;
    }

    public class IPCConfig
    {
        private readonly static string Path = "./IPCConfig.json";
        private readonly static MapList<int, IPCInfo> MapIPCInfo = new MapList<int, IPCInfo>();


        public static IPCInfo GetIPCInfo(int id)
        {
            if (MapIPCInfo.Count == 0)
            {
                ReadConfig();
            }

            return MapIPCInfo[id];
        }

        private static void ReadConfig()
        {
            Debuger.Log();
            string jsonStr = FileUtils.ReadString(Path);
            var obj = MiniJSON.Safe.Json.Deserialize(jsonStr) as List<object>;
            for (int i = 0; i < obj.Count; i++)
            {
                var infoJson = obj[i] as Dictionary<string, object>;
                IPCInfo info = new IPCInfo();
                info.id = (int)(long)infoJson["id"];
                info.port = (int)(long)infoJson["port"];
                MapIPCInfo.Add(info.id, info);
            }
        }
    }
}
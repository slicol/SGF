using System.Collections.Generic;
using SGF.Common;
using SGF.Utils;

namespace SGF.Server
{
    public class ServerModuleInfo
    {
        public int id;
        public string name;
        public string assembly;
        public int port;
    }

    public class ServerConfig
    {
        internal static string Namespace = "SGF.Server";
        internal readonly static string Path = "./ServerConfig.json";

        private readonly static MapList<int, ServerModuleInfo> MapServerModuleInfo = new MapList<int, ServerModuleInfo>();


        public static ServerModuleInfo GetServerModuleInfo(int id)
        {
            if (MapServerModuleInfo.Count == 0)
            {
                ReadConfig();
            }

            return MapServerModuleInfo[id];
        }

        private static void ReadConfig()
        {
            Debuger.Log();
            string jsonStr = FileUtils.ReadString(Path);
            var obj = MiniJSON.Json.Deserialize(jsonStr) as List<object>;
            for (int i = 0; i < obj.Count; i++)
            {
                var infoJson = obj[i] as Dictionary<string, object>;
                ServerModuleInfo info = new ServerModuleInfo();
                info.id = (int)(long)infoJson["id"];
                info.name = (string)infoJson["name"];
                info.assembly = (string)infoJson["assembly"];
                info.port = (int)(long)infoJson["port"];
                MapServerModuleInfo.Add(info.id, info);
            }
        }

    }
}
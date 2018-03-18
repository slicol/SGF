using System.Collections.Generic;
using SGF.Common;
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
            var obj = MiniJSON.Json.Deserialize(jsonStr) as List<object>;
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
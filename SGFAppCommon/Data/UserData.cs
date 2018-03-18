using ProtoBuf;
using SGF.Utils;

namespace SGFAppDemo.Common.Data
{
    [ProtoContract]
    public class UserData
    {
        public static int OnlineTimeout = 40;

        [ProtoMember(1)] public uint id;
        [ProtoMember(2)] public string name;
        [ProtoMember(3)] public string pwd;



        public UserServerData svrdata = new UserServerData();


        public override string ToString()
        {
            return string.Format("<id:{0}, name:{1}, online:{2}>", id, name, svrdata.online);

        }
    }

    public class UserServerData
    {
        public uint lastHeartBeatTime = 0;
        private bool m_online = false;
        public bool online
        {
            get
            {
                if (m_online)
                {
                    uint dt = (uint)TimeUtils.GetTotalSecondsSince1970() - lastHeartBeatTime;
                    if (dt > UserData.OnlineTimeout)
                    {
                        m_online = false;
                    }
                }
                return m_online;
            }
            set { m_online = value; }
        }

    }

}
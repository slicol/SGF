using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;
using SGF.Extension;


namespace SGFAppDemo.Common.Data
{
	[ProtoContract]
    public class RoomData
    {
		[ProtoMember(1)]
        public uint id;
        [ProtoMember(2)]
        public string name;
        [ProtoMember(3)]
        public uint owner;
        [ProtoMember(4)]
        public List<PlayerData> players = new List<PlayerData>();

        public override string ToString()
        {
            return string.Format("<id:{0}, name:{1}, owner:{2}, players:{3}>",id,name,owner,players.ToListString());
        }


    }

    [ProtoContract]
    public class RoomListData
    {
        [ProtoMember(1)]
        public List<RoomData> rooms = new List<RoomData>();
    }


}

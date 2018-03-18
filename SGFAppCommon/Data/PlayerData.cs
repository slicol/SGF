using ProtoBuf;

namespace SGFAppDemo.Common.Data
{
	[ProtoContract]
    public class PlayerData
    {
		[ProtoMember(1)]
        public uint id;
		[ProtoMember(2)]
        public string name;
		[ProtoMember(3)]
        public uint userId;
		[ProtoMember(4)]
        public uint sid;
		[ProtoMember(5)]
        public bool isReady;
        

		public override string ToString ()
		{
			return string.Format ("<id:{0}, name:{1}, userId:{2}, sid:{3}, isReady:{4}>", id, name, userId, sid, isReady);
		}
    }
}

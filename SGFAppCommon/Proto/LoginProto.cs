using ProtoBuf;
using SGF.Network.General.Proto;
using SGFAppDemo.Common.Data;

namespace SGFAppDemo.Common.Proto
{
    [ProtoContract]
    public class LoginReq
    {
        [ProtoMember(1)]
        public uint id;
        [ProtoMember(2)]
        public string name;
    }

    [ProtoContract]
    public class LoginRsp
    {
        [ProtoMember(1)]
        public ReturnCode ret;
        [ProtoMember(2)]
        public UserData userdata;
    }
}
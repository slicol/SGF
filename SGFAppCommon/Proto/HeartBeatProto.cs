using ProtoBuf;
using SGF.Network.General.Proto;
using SGFAppDemo.Common.Data;

namespace SGFAppDemo.Common.Proto
{
    /// <summary>
    /// 可以利用心跳协议完成网络测速
    /// </summary>
    [ProtoContract]
    public class HeartBeatReq
    {
        [ProtoMember(1)] public ushort ping;
        [ProtoMember(2)] public uint timestamp;
    }

    [ProtoContract]
    public class HeartBeatRsp
    {
        [ProtoMember(1)]public ReturnCode ret;
        [ProtoMember(2)]public uint timestamp;
    }
}
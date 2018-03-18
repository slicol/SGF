using ProtoBuf;
using SGF.Network.Core.RPCLite;

namespace SGF.IPCWork
{
    [ProtoContract]
    public class IPCMessage
    {
        [ProtoMember(1)] public int src;//源服务模块ID
        [ProtoMember(2)] public RPCMessage rpc;

    }
}
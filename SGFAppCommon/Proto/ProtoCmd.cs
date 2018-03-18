using ProtoBuf;

namespace SGFAppDemo.Common.Proto
{
    [ProtoContract]
    public class ReturnCode
    {
        public static ReturnCode Success = new ReturnCode();
        public static ReturnCode UnkownError = new ReturnCode(1,"UnkownError");

        public ReturnCode(int code, string info)
        {
            this.code = code;
            this.info = info;
        }

        public ReturnCode()
        {
            this.code = 0;
            this.info = "";
        }



        [ProtoMember(1)] public int code = 0;
        [ProtoMember(2)] public string info = "";
    }

    public class ProtoCmd
    {
        public static uint LoginReq = 1;
        public static uint LoginRsp = 2;
        public static uint HeartBeatReq = 3;
        public static uint HeartBeatRsp = 4;
    }
}
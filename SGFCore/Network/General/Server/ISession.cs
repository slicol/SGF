using System.Net;

namespace SGF.Network.General.Server
{

    public interface ISessionListener
    {
        void OnReceive(ISession session, byte[] bytes, int len);
    }

    public static class SessionID
    {
        private static uint ms_lastSid = 0;
        public static uint NewID()
        {
            return ++ms_lastSid;
        }
    }


    public interface ISession
    {
        uint id { get; }
        uint uid { get; }
        ushort ping { get; set; }
        void Active(IPEndPoint remotePoint);
        bool IsActived();
        bool IsAuth();
        void SetAuth(uint userId);
        bool Send(byte[] bytes, int len);
        IPEndPoint remoteEndPoint { get; }

        void Tick(uint currentTimeMS);
        void DoReceiveInGateway(byte[] buffer, int len);
    }
}
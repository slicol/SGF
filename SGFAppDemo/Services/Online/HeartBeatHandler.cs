using SGF;
using SGF.MathLite;
using SGF.Network.Core;
using SGF.Network.General.Client;
using SGF.SEvent;
using SGF.Time;
using SGF.Utils;
using SGFAppDemo.Common.Proto;

namespace SGFAppDemo.Services.Online
{
    public class HeartBeatHandler
    {
        private NetManager m_net;
        public Signal onTimeout = new Signal();
        private float m_lastHeartBeatTime = 0;
        private uint m_ping = 0;

        public void Init(NetManager net)
        {
            m_net = net;
        }

        public void Start()
        {
            m_lastHeartBeatTime = SGFTime.GetTimeSinceStartup() + SGFRandom.Default.Range(5.0f);
            GlobalEvent.onUpdate.AddListener(OnUpdate);
        }

        public void Stop()
        {
            GlobalEvent.onUpdate.RemoveListener(OnUpdate);
        }

        private void OnUpdate(float dt)
        {
            float current = SGFTime.GetTimeSinceStartup();
            if (current - m_lastHeartBeatTime > 5.0f)
            {
                m_lastHeartBeatTime = current;

                HeartBeatReq req = new HeartBeatReq();
                req.ping = (ushort)m_ping;
                req.timestamp = (uint)TimeUtils.GetTotalMillisecondsSince1970();
                //m_net.Send<HeartBeatReq, HeartBeatRsp>(ProtoCmd.HeartBeatReq, req, OnHeartBeatRsp,15, OnHeartBeatError);
                m_net.Send<HeartBeatRsp>(ProtoCmd.HeartBeatReq, req, OnHeartBeatRsp, 15, OnHeartBeatError);

            }
        }

        private void OnHeartBeatRsp(uint index, HeartBeatRsp rsp)
        {
            Debuger.Log();
            if (rsp.ret.code == 0)
            {
                uint current = (uint)TimeUtils.GetTotalMillisecondsSince1970();
                uint dt = current - rsp.timestamp;
                m_ping = dt / 2;
            }
        }

        private void OnHeartBeatError(NetErrorCode code)
        {
            if (code == NetErrorCode.Timeout)
            {
                Stop();
                onTimeout.Invoke();
            }
        }
    }
}

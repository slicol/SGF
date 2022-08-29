using System.Diagnostics;
using System.Text;
using SGF;
using SGF.Network.Core;
using SGF.Network.Core.RPCLite;
using SGF.Network.General.Proto;
using SGF.Network.General.Server;
using SGF.Time;
using SGF.Utils;
using SGFAppDemo.Common.Data;
using SGFAppDemo.Common.Proto;

namespace SGFServerDemo.ZoneServer
{
    /// <summary>
    /// 因为只是一个Demo，所以不需要注册，谁先Login，谁先占用UserName
    /// 如果玩家掉线后，有别的玩家使用该UserName登录，则UserName会被别的玩家占用
    /// </summary>
    public class OnlineManager:Singleton<OnlineManager>
    {


        private MapList<uint, UserData> m_mapUserData;
        
        
        private NetManager m_net;
        private uint m_lastCheckTimeoutStamp;

        public void Init(ServerContext context)
        {
            m_net = context.net;
            m_net.SetAuthCmd(ProtoCmd.LoginReq);
            m_net.AddListener<LoginReq>(ProtoCmd.LoginReq, OnLoginRequest);
            m_net.AddListener<HeartBeatReq>(ProtoCmd.HeartBeatReq, OnHeartBeatRequest);

            m_net.Rpc.RegisterListener(this);
            m_mapUserData = new MapList<uint, UserData>();
        }

        public void Dump()
        {
  
            StringBuilder sb = new StringBuilder();
            UserData[] list = m_mapUserData.ToArray();
            for (int i = 0; i < list.Length; i++)
            {
                sb.AppendLine("\t" + list[i].ToString());
            }

            Debuger.LogWarning("\nUser ({0}):\n{1}", m_mapUserData.Count, sb);
            
        }



        private void OnLoginRequest(ISession session, ProtocolHead head, LoginReq req)
        {
            Debuger.Log("session:{0}, index:{1}, name:{2}", session.Id, head.index, req.name);
            bool success = false;

            UserData ud = GetUserData(req.name);
            if (ud == null)
            {
                //正常登录
                //这里简单地使用SessionId作为UserId
                ud = CreateUserData(session.Id, req.name);
                ud.svrdata.online = true;
                ud.svrdata.lastHeartBeatTime = (uint)TimeUtils.GetTotalSecondsSince1970();
                //session.SetAuth(ud.id);
                session.AuthToken = ud.id;
                success = true;
            }
            else
            {
                if (req.id == ud.id)
                {
                    //重新登录
                    ud.svrdata.online = true;
                    ud.svrdata.lastHeartBeatTime = (uint)TimeUtils.GetTotalSecondsSince1970();
                    //session.SetAuth(ud.id);
                    session.AuthToken = ud.id;
                    success = true;

                }
                else
                {
                    //正常登录，但是尝试占用已有的名字
                    if (!ud.svrdata.online)
                    {
                        //如果该名字已经离线，则可以占用
                        ud.svrdata.online = true;
                        ud.svrdata.lastHeartBeatTime = (uint)TimeUtils.GetTotalSecondsSince1970();
                        //session.SetAuth(ud.id);
                        session.AuthToken = ud.id;
                        success = true;
                    }
                }
            }

            if (success)
            {
                LoginRsp rsp = new LoginRsp();
                rsp.ret = ReturnCode.Success;
                rsp.userdata = ud;

                m_net.Send(session, head, ProtoCmd.LoginRsp, rsp);
                
            }
            else
            {
                LoginRsp rsp = new LoginRsp();
                rsp.ret = new ReturnCode(1, "名字已经被占用了！");
                m_net.Send(session, head, ProtoCmd.LoginRsp, rsp);
            }
        }



        private void OnHeartBeatRequest(ISession session, ProtocolHead head, HeartBeatReq req)
        {
            UserData ud = GetUserData(session.Id);
            if (ud != null)
            {
                ud.svrdata.lastHeartBeatTime = (uint)TimeUtils.GetTotalSecondsSince1970();

                session.Ping = req.ping;
                HeartBeatRsp rsp = new HeartBeatRsp();
                rsp.ret = ReturnCode.Success;
                rsp.timestamp = req.timestamp;
                m_net.Send(session, head, ProtoCmd.HeartBeatRsp, rsp);
            }
            else
            {
                Debuger.LogWarning("找不到Session 对应的UserData! session:{0}", session);
            }
            
        }


        [RPCRequest]
        private void Logout(ISession session)
        {
            
            OnlineManager.Instance.ReleaseUserData(session.Id);
            m_net.Rpc.Return();
        }

        public UserData CreateUserData(uint id, string name)
        {
            UserData data = new UserData();
            data.name = name;
            data.id = id;
            data.pwd = "";
            m_mapUserData.Add(id, data);
            return data;
        }

        public void ReleaseUserData(uint id)
        {
            m_mapUserData.Remove(id);
        }

        public UserData GetUserData(string name)
        {
            int cnt = m_mapUserData.Count;
            var list = m_mapUserData.AsList();
            for (int i = 0; i < cnt; i++)
            {
                if (list[i].name == name)
                {
                    return list[i];
                }
            }

            return null;
        }

        public UserData GetUserData(uint id)
        {
            return m_mapUserData[id];
        }
    }
}
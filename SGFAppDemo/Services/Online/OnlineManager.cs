using System;
using System.Collections.Generic;
using SGF;
using SGF.Extension;
using SGF.Module;
using SGF.Network.Core;
using SGF.Network.Core.RPCLite;
using SGF.Network.General;
using SGF.Network.General.Client;
using SGF.Time;
using SGF.Utils;
using SGFAppDemo.Common.Data;
using SGFAppDemo.Common.Proto;
using SGFAppDemo.Services.Online;

namespace SGFAppDemo.Services
{
    public enum ConnID
    {
        ZoneServer = 1,
        GameServer = 2
    }

    public class OnlineManager:Singleton<OnlineManager>
    {
        private NetManager m_net;
        private bool m_connected = false;
        private UserData m_mainUserData = null;
        private HeartBeatHandler m_heartbeat;

        public void Init()
        {
            m_net = new NetManager();

            //m_net.Init(typeof(KCPConnection), (int)ConnID.ZoneServer,0);
            //m_net.RegisterRPCListener(this);
            m_net.Init(ConnectionType.RUDP, 0);
            m_net.Rpc.RegisterListener(this);

            GlobalEvent.onUpdate.AddListener(OnUpdate);
            ConsoleInput.onInputKey.AddListener(OnInputKey);

            m_heartbeat = new HeartBeatHandler();
            m_heartbeat.Init(m_net);
            m_heartbeat.onTimeout.AddListener(OnHeartBeatTimeout);
        }

        private void OnInputKey(ConsoleKey key)
        {
            Debuger.Log(key);
            if (key == ConsoleKey.F1)
            {
                m_net.Dump();
                ServerProfiler.Init();
            }
            else if(key == ConsoleKey.F2)
            {
                ServerProfiler.Start();
                Login("slicol");
            }
            else if (key == ConsoleKey.F3)
            {
                ServerProfiler.Stop();
                Logout();
            }
            
        }

        public NetManager Net { get { return m_net; }}
        public UserData MainUserData
        {
            get { return m_mainUserData; }
        }

        private void OnUpdate(float dt)
        {
            m_net.Tick();
        }

        private void OnHeartBeatTimeout()
        {
            Debuger.LogWarning("");
            CloseConnect();
            m_heartbeat.Stop();

            //转个菊花
            //执行断线重连策略

            ReLogin();
        }

        public void Connect()
        {
            if (!m_connected)
            {
                m_net.Connect("111.230.116.185", 4540);
                //m_net.Connect("127.0.0.1", 4540);
                //m_connected = m_net.Connected;
                m_connected = m_net.IsConnected;
            }
        }

        private void CloseConnect()
        {
            m_net.Close();
            m_connected = false;
        }

        public void Login(string username)
        {
            Connect();

            LoginReq req = new LoginReq();
            req.name = username;
            req.id = 0;

            //m_net.Send<LoginReq, LoginRsp>(ProtoCmd.LoginReq, req, OnLoginRsp);
            m_net.Send<LoginRsp>(ProtoCmd.LoginReq, req, OnLoginRsp);
        }

        private void ReLogin()
        {
            Connect();

            LoginReq req = new LoginReq();
            req.name = m_mainUserData.name;
            req.id = m_mainUserData.id;

            //m_net.Send<LoginReq, LoginRsp>(ProtoCmd.LoginReq, req, OnLoginRsp);
            m_net.Send<LoginRsp>(ProtoCmd.LoginReq, req, OnLoginRsp);
        }

        private void OnLoginRsp(uint index, LoginRsp rsp)
        {
            if (rsp.ret.code == 0)
            {
                Debuger.Log("UserData:{0}", rsp.userdata);
                m_mainUserData = rsp.userdata;

                m_heartbeat.Start();
            }
            else
            {
                Debuger.LogWarning(rsp.ret.info);
            }
        }
        



        
        public void Logout()
        {
            m_heartbeat.Stop();

            if (m_mainUserData != null)
            {
                //m_net.Invoke("Logout");
                m_net.Rpc.Invoke("Logout");
            }
            m_mainUserData = null;
        }

        [RPCResponse]
        private void OnLogout()
        {
            m_connected = false;
        }

    }
}
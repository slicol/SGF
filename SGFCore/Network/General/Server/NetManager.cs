/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
 * 网络模块管理器（服务器）
 * Network Module Manager (Server)
 * 
 * Licensed under the MIT License (the "License"); 
 * you may not use this file except in compliance with the License. 
 * You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, 
 * software distributed under the License is distributed on an "AS IS" BASIS, 
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. 
 * See the License for the specific language governing permissions and limitations under the License.
*/

using System;
using System.Reflection;
using System.Text;
using SGF.Codec;
using SGF.Network.Core.RPCLite;
using SGF.Network.General.Proto;
using SGF.SEvent;
using SGF.Utils;


namespace SGF.Network.General.Server
{
    public class NetManager : ISessionListener
    {
        public Signal<ISession> onDisconnected { get; private set; }
        private IGateway m_gateway;
        private uint m_authCmd = 0;
        private RPCManager m_rpc;

        public void Init(ConnectionType connType, int port)
        {
            Debuger.Log("connType:{0}, port:{1}", connType, port);

            if (connType == ConnectionType.TCP)
            {
                m_gateway = new TcpGateway(port, this);
            }
            else if (connType == ConnectionType.UDP)
            {
                m_gateway = new UdpGateway(port, this);
            }
            else if (connType == ConnectionType.RUDP)
            {
                m_gateway = new KcpGateway(port, this);
            }
            else
            {
                throw new ArgumentException("未实现该连接类型：" + connType);
            }

            onDisconnected = new Signal<ISession>();

            m_rpc = new RPCManager();
            m_rpc.Init();
        }

        public RPCManager Rpc { get { return m_rpc; } }

        public void Clean()
        {
            Debuger.Log();
            if (m_gateway != null)
            {
                m_gateway.Clean();
                m_gateway = null;
            }

            if (m_rpc != null)
            {
                m_rpc.Clean();
                m_rpc = null;
            }


            m_listMsgListener.Clear();
            onDisconnected.RemoveAllListeners();
        }


        public void Dump()
        {
            m_gateway.Dump();

            StringBuilder sb = new StringBuilder();

            foreach (var pair in m_listMsgListener)
            {
                ListenerHelper helper = pair.Value;
                sb.AppendFormat("\t<cmd:{0}, msg:{1}, \tlistener:{2}.{3}>\n", pair.Key, helper.TMsg.Name,
                    helper.onMsgMethod.DeclaringType.Name, helper.onMsgMethod.Name);
            }

            Debuger.LogWarning("\nNet Listeners ({0}):\n{1}", m_listMsgListener.Count, sb);

            m_rpc.Dump();
        }


        public void SetAuthCmd(uint cmd)
        {
            m_authCmd = cmd;
        }


        public void Tick()
        {
            m_gateway.Tick();
        }



        public void OnReceive(ISession session, NetMessage msg)
        {
            if (session.AuthToken != 0)
            {
                if (session.AuthToken == msg.head.token)
                {
                    if (msg.head.cmd == 0)
                    {
                        RPCMessage rpcmsg = PBSerializer.NDeserialize<RPCMessage>(msg.content);
                        m_rpc.OnReceive(session, rpcmsg);
                    }
                    else
                    {
                        HandlePBMessage(session, msg);
                    }
                }
                else
                {
                    Debuger.LogWarning("收到消息的Token与Session的Token不一致！session.token:{0}, msg.token:{1}", session.AuthToken, msg.head.token);
                }
            }
            else
            {
                if (m_authCmd == 0 || msg.head.cmd == m_authCmd)
                {
                    HandlePBMessage(session, msg);
                }
                else
                {
                    Debuger.LogWarning("收到未鉴权的消息! cmd:{0}", msg.head.cmd);
                }
            }
        }

        public void OnDisconnected(ISession session)
        {
            onDisconnected.Invoke(session);
        }


        //========================================================================
        //传统的协议处理方式
        //========================================================================

        class ListenerHelper
        {
            public Type TMsg;
            public Delegate onMsg0;
            public Delegate onMsg1;

            public MethodInfo onMsgMethod
            {
                get
                {
                    var onMsg = onMsg0 != null ? onMsg0 : (onMsg1 != null ? onMsg1 : null);
                    return onMsg.Method;
                }
            }
        }

        private DictionarySafe<uint, ListenerHelper> m_listMsgListener = new DictionarySafe<uint, ListenerHelper>();

        private void HandlePBMessage(ISession session, NetMessage msg)
        {
            Debuger.LogVerbose("msg.head:{0}", msg.head);

            var helper = m_listMsgListener[msg.head.cmd];
            if (helper != null)
            {
                object obj = null;

                try
                {
                    obj = PBSerializer.NDeserialize(msg.content, helper.TMsg);
                }
                catch (Exception e)
                {
                    Debuger.LogError("MsgName:{0}, msg.head:{0}", helper.TMsg.Name, msg.head);
                    Debuger.LogError("DeserializeError:" + e.Message);
                }

                if (obj != null)
                {
                    try
                    {
                        if (helper.onMsg0 != null)
                        {
                            helper.onMsg0.DynamicInvoke(session, msg.head.index, obj);
                        }
                        else if (helper.onMsg1 != null)
                        {
                            helper.onMsg1.DynamicInvoke(session, msg.head, obj);
                        }
                    }
                    catch (Exception e)
                    {
                        Debuger.LogError("MsgName:{0}, msg.head:{0}", helper.TMsg.Name, msg.head);
                        Debuger.LogError("BusinessError:" + e.Message + "\n" + e.StackTrace);
                    }
                }
            }
            else
            {
                Debuger.LogWarning("未找到对应的监听者! cmd:{0}", msg.head.cmd);
            }
        }


        public void Send<TMsg>(ISession session, ProtocolHead head, uint cmd, TMsg msg)
        {
            Debuger.LogVerbose("index:{0}, cmd:{1}", head.index, cmd);

            NetMessage msgobj = new NetMessage();
            msgobj.head = head;
            msgobj.head.cmd = cmd;
            msgobj.head.token = session.AuthToken;
            msgobj.content = PBSerializer.NSerialize(msg);
            msgobj.head.dataSize = (uint)msgobj.content.Length;

            session.Send(msgobj);
        }

        public void AddListener<TMsg>(uint cmd, Action<ISession, uint, TMsg> onMsg)
        {
            Debuger.Log("cmd:{0}, listener:{1}.{2}", cmd, onMsg.Method.DeclaringType.Name, onMsg.Method.Name);

            ListenerHelper helper = new ListenerHelper()
            {
                TMsg = typeof(TMsg),
                onMsg0 = onMsg
            };

            m_listMsgListener.Add(cmd, helper);
        }

        public void AddListener<TMsg>(uint cmd, Action<ISession, ProtocolHead, TMsg> onMsg)
        {
            Debuger.Log("cmd:{0}, listener:{1}.{2}", cmd, onMsg.Method.DeclaringType.Name, onMsg.Method.Name);

            ListenerHelper helper = new ListenerHelper()
            {
                TMsg = typeof(TMsg),
                onMsg1 = onMsg
            };

            m_listMsgListener.Add(cmd, helper);
        }

    }
}
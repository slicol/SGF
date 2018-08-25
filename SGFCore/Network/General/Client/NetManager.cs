/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
 * 网络模块管理器（客户端）
 * Network Module Manager (Client)
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
using System.Net;
using System.Reflection;
using System.Text;
using SGF.Codec;
using SGF.Network.Core;
using SGF.Network.Core.RPCLite;
using SGF.Network.General.Proto;
using SGF.SEvent;
using SGF.Time;
using SGF.Utils;


namespace SGF.Network.General.Client
{
    public class NetManager
    {
        public Signal onDisconnected { get; private set; }
        public Signal onConnectFailed { get; private set; }
        private IConnection m_conn;
        private uint m_token;
        private RPCManager m_rpc;

        public void Init(ConnectionType connType, int localPort = 0)
        {
            Debuger.Log("connType:{0}, localPort:{1}", connType, localPort);

            if (connType == ConnectionType.TCP)
            {
                var conn = new TcpConnection(localPort, null, 3);
                m_conn = conn;
                conn.onDisconnected.AddListener(OnDisconnected);
                conn.onConnectError.AddListener(OnConnectError);
            }
            else if (connType == ConnectionType.UDP)
            {
                m_conn = new UdpConnection(localPort, null);
            }
            else if(connType == ConnectionType.RUDP)
            {
                m_conn = new KcpConnection(localPort);
            }
            else
            {
                throw new ArgumentException("未实现该连接类型：" + connType);
            }

            onDisconnected = new Signal();
            onConnectFailed = new Signal();

            m_rpc = new RPCManager(m_conn);
            m_rpc.Init();
        }

        public RPCManager Rpc { get { return m_rpc; } }

        public void Clean()
        {
            Debuger.Log();
            if (m_conn != null)
            {
                m_conn.Clean();
                m_conn = null;
            }

            if (m_rpc != null)
            {
                m_rpc.Clean();
                m_rpc = null;
            }


            m_listNtfListener.Clear();
            m_listRspListener.Clear();

        }

        private void OnDisconnected(IConnection  conn)
        {
            onDisconnected.Invoke();
        }

        private void OnConnectError(IConnection conn, int code)
        {
            if(code == (int)NetErrorCode.ReconnectFailed)
            {
                onConnectFailed.Invoke();
            }
        }

        public void Dump()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var pair in m_listNtfListener)
            {
                ListenerHelper helper = pair.Value;
                sb.AppendFormat("\t<cmd:{0}, msg:{1}, \tlistener:{2}.{3}>\n", pair.Key, helper.TMsg.Name,
                    helper.onMsgMethod.DeclaringType.Name, helper.onMsgMethod.Name);
            }

            Debuger.LogWarning("\nNotify Listeners ({0}):\n{1}", m_listNtfListener.Count, sb);

            sb.Length = 0;
            var dic = m_listRspListener.AsDictionary();
            foreach (var pair in dic)
            {
                ListenerHelper helper = pair.Value;
                sb.AppendFormat("\t<index:{0}, msg:{1}, \tlistener:{2}.{3}>\n", pair.Key, helper.TMsg.Name,
                    helper.onMsgMethod.DeclaringType.Name, helper.onMsgMethod.Name);
            }

            Debuger.LogWarning("\nRespond Listeners ({0}):\n{1}", m_listRspListener.Count, sb);

            m_rpc.Dump();
        }

        public void SetToken(uint token)
        {
            Debuger.Log(token);
            m_token = token;
            m_rpc.SetToken(token);
        }


        public void Connect(string ip, int port)
        {
            Debuger.Log("ip:{0}, port:{1}", ip, port);

            if (m_conn.IsActived)
            {
                Debuger.Log("旧的连接还在，先关闭旧的连接");
                m_conn.Close();
            }

            m_conn.Connect(ip, port);
            m_conn.onReceive.AddListener(OnReceive);
        }

        public void Connect(IPEndPoint[] listEndPoints)
        {
            if (m_conn.IsActived)
            {
                Debuger.Log("旧的连接还在，先关闭旧的连接");
                m_conn.Close();
            }

            m_conn.Connect(listEndPoints);
            m_conn.onReceive.AddListener(OnReceive);
        }

        public bool IsConnected { get { return m_conn.IsActived; } }

        public void Close()
        {
            Debuger.Log();
            m_conn.Close();
        }


        public void Tick()
        {
            m_conn.Tick();
            CheckTimeout();
        }


        private void OnReceive(NetMessage msg)
        {
            if (msg.head.cmd == 0)
            {
                RPCMessage rpcmsg = PBSerializer.NDeserialize<RPCMessage>(msg.content);
                m_rpc.OnReceive(rpcmsg);
            }
            else
            {
                HandlePBMessage(msg);
            }
        }





        //========================================================================
        //传统的协议(Protobuf)处理方式
        //========================================================================


        class ListenerHelper
        {
            public uint cmd;
            public uint index;
            public Type TMsg;
            public Delegate onMsg0;
            public Delegate onMsg1;
            public Delegate onErr;
            public float timeout;
            public float timestamp;

            public MethodInfo onMsgMethod
            {
                get
                {
                    var onMsg_ = onMsg0 != null ? onMsg0 : (onMsg1 != null ? onMsg1 : null);
                    return onMsg_.Method;
                }
            }
        }

        static class MessageIndexGenerator
        {
            private static uint m_lastIndex;
            public static uint NewIndex()
            {
                return ++m_lastIndex;
            }
        }

        private DictionarySafe<uint, ListenerHelper> m_listNtfListener = new DictionarySafe<uint, ListenerHelper>();
        private MapList<uint, ListenerHelper> m_listRspListener = new MapList<uint, ListenerHelper>();


        /// <summary>
        /// 发送数据
        /// </summary>
        /// <typeparam name="TRsp"></typeparam>
        /// <param name="cmd"></param>
        /// <param name="req"></param>
        /// <param name="onRsp"></param>
        /// <param name="timeout"></param>
        /// <param name="onErr"></param>
        /// <returns>返回唯一的发送Index</returns>
        public uint Send<TRsp>(uint cmd, object req, Action<uint, TRsp> onRsp, float timeout = 30,
            Action<NetErrorCode> onErr = null)
        {
            Debuger.LogVerbose("cmd:{0}, timeout:{1}", cmd, timeout);
            uint index = MessageIndexGenerator.NewIndex();
            ListenerHelper helper = new ListenerHelper()
            {
                cmd = cmd,
                index = index,
                TMsg = typeof(TRsp),
                onErr = onErr,
                onMsg0 = onRsp,
                timeout = timeout,
                timestamp = SGFTime.GetTimeSinceStartup()
            };

            m_listRspListener.Add(index, helper);


            NetMessage msg = new NetMessage();
            msg.head.index = index;
            msg.head.cmd = cmd;
            msg.head.token = m_token;
            msg.content = PBSerializer.NSerialize(req);
            msg.head.dataSize = (uint)msg.content.Length;

            m_conn.Send(msg);
            return index;
        }


        public void Send<TReq>(uint cmd, TReq req)
        {
            Debuger.LogVerbose("cmd:{0}", cmd);

            NetMessage msg = new NetMessage();
            msg.head.index = 0;
            msg.head.cmd = cmd;
            msg.head.token = m_token;
            msg.content = PBSerializer.NSerialize(req);
            msg.head.dataSize = (uint)msg.content.Length;

            m_conn.Send(msg);
        }

        public void Send<TMsg>(ProtocolHead head, uint cmd, TMsg msg)
        {
            Debuger.LogVerbose("cmd:{0}", cmd);

            NetMessage msgobj = new NetMessage();
            msgobj.head = head;
            msgobj.head.cmd = cmd;
            msgobj.head.token = m_token;
            msgobj.content = PBSerializer.NSerialize(msg);
            msgobj.head.dataSize = (uint)msgobj.content.Length;

            m_conn.Send(msgobj);
        }


        public void AddListener<TNtf>(uint cmd, Action<ProtocolHead, TNtf> onNtf)
        {
            Debuger.Log("cmd:{0}, listener:{1}.{2}", cmd, onNtf.Method.DeclaringType.Name, onNtf.Method.Name);


            ListenerHelper helper = new ListenerHelper()
            {
                TMsg = typeof(TNtf),
                onMsg1 = onNtf
            };

            m_listNtfListener.Add(cmd, helper);
        }

        public void RemoveListener(uint cmd)
        {
            Debuger.Log("cmd:{0}", cmd);
            m_listNtfListener.Remove(cmd);
        }


        private void HandlePBMessage(NetMessage msg)
        {
            Debuger.LogVerbose("msg.head:{0}", msg.head);

            if (msg.head.index == 0)
            {
                var helper = m_listNtfListener[msg.head.cmd];
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
                            helper.onMsg1.DynamicInvoke(msg.head, obj);
                        }
                        catch (Exception e)
                        {
                            Debuger.LogError("MsgName:{0}, msg.head:{0}", helper.TMsg.Name, msg.head);
                            Debuger.LogError("BusinessError:" + e.Message + "\n" + e.StackTrace);
                        }

                    }
                    else
                    {
                        Debuger.LogError("协议格式错误！ cmd:{0}", msg.head.cmd);
                    }
                }
                else
                {
                    Debuger.LogError("未找到对应的监听者! cmd:{0}", msg.head.cmd);
                }
            }
            else
            {
                var helper = m_listRspListener[msg.head.index];
                if (helper != null)
                {
                    m_listRspListener.Remove(msg.head.index);

                    object obj = PBSerializer.NDeserialize(msg.content, helper.TMsg);
                    if (obj != null)
                    {
                        helper.onMsg0.DynamicInvoke(msg.head.index, obj);
                    }
                    else
                    {
                        Debuger.LogError("协议格式错误！ cmd:{0}, index:{0}", msg.head.cmd, msg.head.index);
                    }
                }
                else
                {
                    Debuger.LogError("未找到对应的监听者! cmd:{0}, index:{0}", msg.head.cmd, msg.head.index);
                }
            }
        }


        private float m_lastCheckTimeoutStamp = 0;

        private void CheckTimeout()
        {
            float curTime = SGFTime.GetTimeSinceStartup();

            if (curTime - m_lastCheckTimeoutStamp >= 5)
            {
                m_lastCheckTimeoutStamp = curTime;

                var list = m_listRspListener.ToArray();
                for (int i = 0; i < list.Length; i++)
                {
                    var helper = list[i];
                    float dt = curTime - helper.timestamp;
                    if (dt >= helper.timeout && helper.timeout > 0)
                    {
                        m_listRspListener.Remove(helper.index);
                        if (helper.onErr != null)
                        {
                            helper.onErr.DynamicInvoke(NetErrorCode.Timeout);
                        }

                        Debuger.LogWarning("cmd:{0} Is Timeout!", helper.cmd);
                    }
                }
            }

        }

    }
}
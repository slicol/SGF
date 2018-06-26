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
using System.Reflection;
using System.Text;
using SGF.Codec;
using SGF.Common;
using SGF.Network.Core;
using SGF.Network.Core.RPCLite;
using SGF.Time;

namespace SGF.Network.General.Client
{
    public class NetManager
    {
        private IConnection m_conn;
        private uint m_uid;
        private RPCManagerBase m_rpc;

        public void Init(Type connType, int connId, int bindPort)
        {
            Debuger.Log("connType:{0}, connId:{1}, bindPort:{2}", connType, connId, bindPort);

            m_conn = Activator.CreateInstance(connType) as IConnection;
            m_conn.Init(connId, bindPort);

            m_rpc = new RPCManagerBase();
            m_rpc.Init();

        }


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

        public void Dump()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var pair in m_listNtfListener)
            {
                ListenerHelper helper = pair.Value;
                sb.AppendFormat("\t<cmd:{0}, msg:{1}, \tlistener:{2}.{3}>\n", pair.Key, helper.TMsg.Name,
                    helper.onMsg.Method.DeclaringType.Name, helper.onMsg.Method.Name);
            }

            Debuger.LogWarning("\nNotify Listeners ({0}):\n{1}", m_listNtfListener.Count, sb);

            sb.Length = 0;
            var dic = m_listRspListener.AsDictionary();
            foreach (var pair in dic)
            {
                ListenerHelper helper = pair.Value;
                sb.AppendFormat("\t<index:{0}, msg:{1}, \tlistener:{2}.{3}>\n", pair.Key, helper.TMsg.Name,
                    helper.onMsg.Method.DeclaringType.Name, helper.onMsg.Method.Name);
            }

            Debuger.LogWarning("\nRespond Listeners ({0}):\n{1}", m_listRspListener.Count, sb);

            m_rpc.Dump();
        }


        public void SetUserId(uint uid)
        {
            m_uid = uid;
        }


        public void Connect(string ip, int port)
        {
            Debuger.Log("ip:{0}, port:{1}", ip, port);

            if (m_conn.Connected)
            {
                Debuger.Log("旧的连接还在，先关闭旧的连接");
                m_conn.Close();
            }

            m_conn.Connect(ip, port);
            m_conn.onReceive.AddListener(OnReceive);
        }

        public bool Connected { get { return m_conn.Connected; } }

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


        private void OnReceive(byte[] bytes, int len)
        {
            NetMessage msg = new NetMessage();
            msg.Deserialize(bytes, len);

            if (msg.head.cmd == 0)
            {
                RPCMessage rpcmsg = PBSerializer.NDeserialize<RPCMessage>(msg.content);
                HandleRPCMessage(rpcmsg);
            }
            else
            {
                HandlePBMessage(msg);
            }
        }





        //========================================================================
        //RPC的协议处理方式
        //========================================================================

        private string m_currInvokingName;

        public void RegisterRPCListener(object listener)
        {
            m_rpc.RegisterListener(listener);
        }

        public void UnRegisterRPCListener(object listener)
        {
            m_rpc.UnRegisterListener(listener);
        }



        private void HandleRPCMessage(RPCMessage rpcmsg)
        {
            Debuger.Log("Connection[{0}]-> {1}({2})", m_conn.id, rpcmsg.name, rpcmsg.args);

            var helper = m_rpc.GetMethodHelper(rpcmsg.name);
            if (helper != null)
            {
                object[] args = rpcmsg.args;
                var raw_args = rpcmsg.raw_args;

                var paramInfo = helper.method.GetParameters();

                if (raw_args.Count == paramInfo.Length)
                {
                    for (int i = 0; i < raw_args.Count; i++)
                    {
                        if (raw_args[i].type == RPCArgType.PBObject)
                        {
                            var type = paramInfo[i].ParameterType;
                            object arg = PBSerializer.NDeserialize(raw_args[i].raw_value, type);
                            args[i] = arg;
                        }
                    }

                    m_currInvokingName = rpcmsg.name;

                    try
                    {
                        helper.method.Invoke(helper.listener, BindingFlags.NonPublic, null, args, null);
                    }
                    catch (Exception e)
                    {
                        Debuger.LogError("RPC调用出错：{0}\n{1}", e.Message, e.StackTrace);
                    }

                    m_currInvokingName = null;
                }
                else
                {
                    Debuger.LogWarning("参数数量不一致！");
                }
                
            }
            else
            {
                Debuger.LogWarning("RPC不存在！");
            }
        }



        public void Invoke(string name, params object[] args)
        {
            Debuger.Log("->Connection[{0}] {1}({2})", m_conn.id, name, args);

            RPCMessage rpcmsg = new RPCMessage();
            rpcmsg.name = name;
            rpcmsg.args = args;
            byte[] buffer = PBSerializer.NSerialize(rpcmsg);

            NetMessage msg = new NetMessage();
            msg.head = new ProtocolHead();
            msg.head.uid = m_uid;
            msg.head.dataSize = (ushort)buffer.Length;
            msg.content = buffer;

            byte[] tmp = null;
            int len = msg.Serialize(out tmp);

            m_conn.Send(tmp, len);

        }

        public void Return(params object[] args)
        {
            if (m_conn != null)
            {
                var name = "On" + m_currInvokingName;
                Debuger.Log("->Connection[{0}] {1}({2})", m_conn.id, name, args);

                RPCMessage rpcmsg = new RPCMessage();
                rpcmsg.name = name;
                rpcmsg.args = args;
                byte[] buffer = PBSerializer.NSerialize(rpcmsg);

                NetMessage msg = new NetMessage();
                msg.head = new ProtocolHead();
                msg.head.uid = m_uid;
                msg.head.dataSize = (ushort)buffer.Length;
                msg.content = buffer;

                byte[] tmp = null;
                int len = msg.Serialize(out tmp);

                m_conn.Send(tmp, len);
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
            public Delegate onMsg;
            public Delegate onErr;
            public float timeout;
            public float timestamp;
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


        public void Send<TReq, TRsp>(uint cmd, TReq req, Action<TRsp> onRsp, float timeout = 30,
            Action<NetErrorCode> onErr = null)
        {
            NetMessage msg = new NetMessage();
            msg.head.index = MessageIndexGenerator.NewIndex();
            msg.head.cmd = cmd;
            msg.head.uid = m_uid;
            msg.content = PBSerializer.NSerialize(req);
            msg.head.dataSize = (ushort)msg.content.Length;

            byte[] temp;
            int len = msg.Serialize(out temp);
            m_conn.Send(temp, len);

            AddListener(cmd, typeof(TRsp), onRsp, msg.head.index, timeout, onErr);
        }


        private void AddListener(uint cmd, Type TRsp, Delegate onRsp, uint index, float timeout, Action<NetErrorCode> onErr)
        {
            ListenerHelper helper = new ListenerHelper()
            {
                cmd = cmd,
                index = index,
                TMsg = TRsp,
                onErr = onErr,
                onMsg = onRsp,
                timeout = timeout,
                timestamp = SGFTime.GetTimeSinceStartup()
            };

            m_listRspListener.Add(index, helper);
        }


        public void Send<TReq>(uint cmd, TReq req)
        {
            Debuger.Log("cmd:{0}", cmd);


            NetMessage msg = new NetMessage();
            msg.head.index = 0;
            msg.head.cmd = cmd;
            msg.head.uid = m_uid;
            msg.content = PBSerializer.NSerialize(req);
            msg.head.dataSize = (ushort)msg.content.Length;

            byte[] temp;
            int len = msg.Serialize(out temp);
            m_conn.Send(temp, len);
        }


        public void AddListener<TNtf>(uint cmd, Action<TNtf> onNtf)
        {
            Debuger.Log("cmd:{0}, listener:{1}.{2}", cmd, onNtf.Method.DeclaringType.Name, onNtf.Method.Name);


            ListenerHelper helper = new ListenerHelper()
            {
                TMsg = typeof(TNtf),
                onMsg = onNtf
            };

            m_listNtfListener.Add(cmd, helper);
        }






        private void HandlePBMessage(NetMessage msg)
        {
            if (msg.head.index == 0)
            {
                var helper = m_listNtfListener[msg.head.cmd];
                if (helper != null)
                {
                    object obj = PBSerializer.NDeserialize(msg.content, helper.TMsg);
                    if (obj != null)
                    {
                        helper.onMsg.DynamicInvoke(obj);
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
                        helper.onMsg.DynamicInvoke(obj);
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
                    if (dt >= helper.timeout)
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
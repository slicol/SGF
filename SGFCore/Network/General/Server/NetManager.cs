using System;
using System.Reflection;
using System.Text;
using SGF.Codec;
using SGF.Common;
using SGF.Network.Core;
using SGF.Network.Core.RPCLite;
using SGF.Extension;

namespace SGF.Network.General.Server
{
    public class NetManager:ISessionListener
    {
        private Gateway m_gateway;
        private RPCManagerBase m_rpc;
        private uint m_authCmd = 0;

        public void Init(int port)
        {
            Debuger.Log("port:{0}", port);

            m_gateway = new Gateway();
            m_gateway.Init(port, this);

            m_rpc = new RPCManagerBase();
            m_rpc.Init();
        }


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

        }


        public void Dump()
        {
            m_gateway.Dump();

            StringBuilder sb = new StringBuilder();

            foreach (var pair in m_listMsgListener)
            {
                ListenerHelper helper = pair.Value;
                sb.AppendFormat("\t<cmd:{0}, msg:{1}, \tlistener:{2}.{3}>\n", pair.Key, helper.TMsg.Name,
                    helper.onMsg.Method.DeclaringType.Name, helper.onMsg.Method.Name);
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



        public void OnReceive(ISession session, byte[] bytes, int len)
        {
            NetMessage msg = new NetMessage();
            msg.Deserialize(bytes, len);

            if (session.IsAuth())
            {
                if (msg.head.cmd == 0)
                {
                    RPCMessage rpcmsg = PBSerializer.NDeserialize<RPCMessage>(msg.content);
                    HandleRPCMessage(session,rpcmsg);
                }
                else
                {
                    HandlePBMessage(session, msg);
                }
            }
            else
            {
                if (msg.head.cmd == m_authCmd)
                {
                    HandlePBMessage(session, msg);
                }
                else
                {
                    Debuger.LogWarning("收到未鉴权的消息! cmd:{0}", msg.head.cmd);
                }
            }
        }


        //========================================================================
        //RPC的协议处理方式
        //========================================================================
        private ISession m_currInvokingSession;
        private string m_currInvokingName;

        public void RegisterRPCListener(object listener)
        {
            m_rpc.RegisterListener(listener);
        }

        public void UnRegisterRPCListener(object listener)
        {
            m_rpc.UnRegisterListener(listener);
        }


        public void HandleRPCMessage(ISession session, RPCMessage rpcmsg)
        {

            RPCMethodHelper helper = m_rpc.GetMethodHelper(rpcmsg.name);
            if (helper != null)
            {
                object[] args = new object[rpcmsg.raw_args.Count + 1];
                var raw_args = rpcmsg.raw_args;
                var paramInfo = helper.method.GetParameters();

                args[0] = session;

                if (args.Length == paramInfo.Length)
                {
                    for (int i = 0; i < raw_args.Count; i++)
                    {
                        if (raw_args[i].type == RPCArgType.PBObject)
                        {
                            args[i + 1] = PBSerializer.NDeserialize(raw_args[i].raw_value, paramInfo[i + 1].ParameterType);
                        }
                        else
                        {
                            args[i + 1] = raw_args[i].value;
                        }
                    }

                    m_currInvokingName = rpcmsg.name;
                    m_currInvokingSession = session;

                    try
                    {    
                        helper.method.Invoke(helper.listener, BindingFlags.NonPublic, null, args, null);
                    }
                    catch (Exception e)
                    {
                        Debuger.LogError("RPC调用出错：{0}\n{1}", e.Message, e.StackTrace);
                    }
                    m_currInvokingName = null;
                    m_currInvokingSession = null;
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


        public void Return(params object[] args)
        {
            var name = "On" + m_currInvokingName;
            

            RPCMessage rpcmsg = new RPCMessage();
            rpcmsg.name = name;
            rpcmsg.args = args;
            byte[] buffer = PBSerializer.NSerialize(rpcmsg);

            NetMessage msg = new NetMessage();
            msg.head = new ProtocolHead();
            msg.head.dataSize = (ushort)buffer.Length;
            msg.content = buffer;

            byte[] tmp = null;
            int len = msg.Serialize(out tmp);

            m_currInvokingSession.Send(tmp, len);
        }


        public void ReturnError(params object[] args)
        {
            var name = "On" + m_currInvokingName + "Error";


            RPCMessage rpcmsg = new RPCMessage();
            rpcmsg.name = name;
            rpcmsg.args = args;
            byte[] buffer = PBSerializer.NSerialize(rpcmsg);

            NetMessage msg = new NetMessage();
            msg.head = new ProtocolHead();
            msg.head.dataSize = (ushort)buffer.Length;
            msg.content = buffer;

            byte[] tmp = null;
            int len = msg.Serialize(out tmp);

            m_currInvokingSession.Send(tmp, len);
        }


        public void Invoke(ISession session, string name, params object[] args)
        {
            Debuger.Log("->Session[{0}] {1}({2})", session.id, name, args.ToListString());

            RPCMessage rpcmsg = new RPCMessage();
            rpcmsg.name = name;
            rpcmsg.args = args;
            byte[] buffer = PBSerializer.NSerialize(rpcmsg);

            NetMessage msg = new NetMessage();
            msg.head = new ProtocolHead();
            msg.head.dataSize = (ushort)buffer.Length;
            msg.content = buffer;

            byte[] tmp = null;
            int len = msg.Serialize(out tmp);


            session.Send(tmp, len);
        }



        public void Invoke(ISession[] listSession, string name, params object[] args)
        {
            Debuger.Log("->Session<Cnt={0}> {1}({2})", listSession.Length, name, args.ToListString());

            RPCMessage rpcmsg = new RPCMessage();
            rpcmsg.name = name;
            rpcmsg.args = args;
            byte[] buffer = PBSerializer.NSerialize(rpcmsg);

            NetMessage msg = new NetMessage();
            msg.head = new ProtocolHead();
            msg.head.dataSize = (ushort)buffer.Length;
            msg.content = buffer;

            byte[] tmp = null;
            int len = msg.Serialize(out tmp);

            for (int i = 0; i < listSession.Length; i++)
            {
                listSession[i].Send(tmp, len);
            }
        }



        //========================================================================
        //传统的协议处理方式
        //========================================================================

        class ListenerHelper
        {
            public Type TMsg;
            public Delegate onMsg;
        }

        private DictionarySafe<uint, ListenerHelper> m_listMsgListener = new DictionarySafe<uint, ListenerHelper>();




        private void HandlePBMessage(ISession session, NetMessage msg)
        {
            var helper = m_listMsgListener[msg.head.cmd];
            if (helper != null)
            {
                object obj = PBSerializer.NDeserialize(msg.content, helper.TMsg);
                if (obj != null)
                {
                    helper.onMsg.DynamicInvoke(session, msg.head.index, obj);
                }
            }
            else
            {
                Debuger.LogWarning("未找到对应的监听者! cmd:{0}", msg.head.cmd);
            }
        }


        public void Send<TMsg>(ISession session, uint index, uint cmd, TMsg msg)
        {
            Debuger.Log("index:{0}, cmd:{1}", index, cmd);

            NetMessage msgobj = new NetMessage();
            msgobj.head.index = index;
            msgobj.head.cmd = cmd;
            msgobj.head.uid = session.uid;
            msgobj.content = PBSerializer.NSerialize(msg);
            msgobj.head.dataSize = (ushort)msgobj.content.Length;

            byte[] tmp;
            int len = msgobj.Serialize(out tmp);

            session.Send(tmp, len);
        }

        public void AddListener<TMsg>(uint cmd, Action<ISession, uint, TMsg> onMsg)
        {
            Debuger.Log("cmd:{0}, listener:{1}.{2}", cmd, onMsg.Method.DeclaringType.Name, onMsg.Method.Name);

            ListenerHelper helper = new ListenerHelper()
            {
                TMsg = typeof(TMsg),
                onMsg = onMsg
            };

            m_listMsgListener.Add(cmd, helper);
        }


    }
}
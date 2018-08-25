using SGF.Codec;
using SGF.Network.Core.RPCLite;
using SGF.Network.General.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SGF.Network.General.Client
{
    public class RPCManager:RPCManagerBase
    {
        private IConnection m_conn;
        private uint m_token;

        public  RPCManager(IConnection conn)
        {
            m_conn = conn;
        }

        public void OnReceive(RPCMessage msg)
        {
            HandleRPCMessage(msg);
        }

        public void SetToken(uint token)
        {
            Debuger.Log(token);
            m_token = token;
        }

        //========================================================================
        //RPC的协议处理方式
        //========================================================================

        private string m_currInvokingName;

        private void HandleRPCMessage(RPCMessage rpcmsg)
        {
            Debuger.LogVerbose("Connection[{0}]-> {1}({2})", m_conn.Id, rpcmsg.name, rpcmsg.args);
            
            var helper = GetMethodHelper(rpcmsg.name);
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
            Debuger.LogVerbose("->Connection[{0}] {1}({2})", m_conn.Id, name, args);
            
            RPCMessage rpcmsg = new RPCMessage();
            rpcmsg.name = name;
            rpcmsg.args = args;
            byte[] buffer = PBSerializer.NSerialize(rpcmsg);

            NetMessage msg = new NetMessage();
            msg.head = new ProtocolHead();
            msg.head.token = m_token;
            msg.head.dataSize = (uint)buffer.Length;
            msg.content = buffer;

            m_conn.Send(msg);

        }

        public void Return(params object[] args)
        {
            if (m_conn != null)
            {
                var name = "On" + m_currInvokingName;
                Debuger.Log("->Connection[{0}] {1}({2})", m_conn.Id, name, args);

                RPCMessage rpcmsg = new RPCMessage();
                rpcmsg.name = name;
                rpcmsg.args = args;
                byte[] buffer = PBSerializer.NSerialize(rpcmsg);

                NetMessage msg = new NetMessage();
                msg.head = new ProtocolHead();
                msg.head.token = m_token;
                msg.head.dataSize = (uint)buffer.Length;
                msg.content = buffer;
                
                m_conn.Send(msg);
            }
        }

    }
}

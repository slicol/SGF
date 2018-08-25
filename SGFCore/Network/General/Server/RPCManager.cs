using SGF.Codec;
using SGF.Extension;
using SGF.Network.Core.RPCLite;
using SGF.Network.General.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SGF.Network.General.Server
{
    public class RPCManager:RPCManagerBase
    {


        public void OnReceive(ISession session, RPCMessage msg)
        {
            HandleRPCMessage(session, msg);
        }


        //========================================================================
        //RPC的协议处理方式
        //========================================================================
        private ISession m_currInvokingSession;
        private string m_currInvokingName;

        public void HandleRPCMessage(ISession session, RPCMessage rpcmsg)
        {
            RPCMethodHelper helper = GetMethodHelper(rpcmsg.name);
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
            msg.head.dataSize = (uint)buffer.Length;
            msg.content = buffer;
            
            m_currInvokingSession.Send(msg);
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
            msg.head.dataSize = (uint)buffer.Length;
            msg.content = buffer;

            m_currInvokingSession.Send(msg);
        }


        public void Invoke(ISession session, string name, params object[] args)
        {
            Debuger.LogVerbose("->Session[{0}] {1}({2})", session.Id, name, args.ToListString());

            RPCMessage rpcmsg = new RPCMessage();
            rpcmsg.name = name;
            rpcmsg.args = args;
            byte[] buffer = PBSerializer.NSerialize(rpcmsg);

            NetMessage msg = new NetMessage();
            msg.head = new ProtocolHead();
            msg.head.dataSize = (uint)buffer.Length;
            msg.content = buffer;
            
            session.Send(msg);
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
            msg.head.dataSize = (uint)buffer.Length;
            msg.content = buffer;
            
            for (int i = 0; i < listSession.Length; i++)
            {
                listSession[i].Send(msg);
            }
        }


    }
}

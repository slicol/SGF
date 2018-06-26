/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
 * 进程通讯模块。
 * 它基于UdpSocket实现，以RPC的方式进行调用。
 * 由于是基于Udp的简单实现，所以对单次通讯的数据大小有要求，不能超过当前系统的MTU。
 * 如果需要在进程间进行大数据通讯，可以使用Network模块，建立可靠UDP连接，或者TCP连接。
 * 当然也可以采用共享内存和管道方案，期待后续有人来完善。
 * Inter-process communication module.
 * It is based on UdpSocket implementation and is called in RPC mode.
 * Since it is based on a simple implementation of Udp, there is a requirement for the data size of a single communication and cannot exceed the MTU of the current system.
 * If you need to perform big data communication between processes, you can use the Network module to establish a reliable UDP connection or a TCP connection.
 * Of course you can also use shared memory and pipeline solutions and look forward to follow-up improvements.
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
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using SGF.Codec;
using SGF.Network;
using SGF.Network.Core.RPCLite;
using SGF.Extension;

namespace SGF.IPCWork
{
    public class IPCManager
    {
        private int m_id;
        private int m_port;

        private Socket m_SystemSocket;
        private Thread m_ThreadRecv;
        private byte[] m_RecvBufferTemp = new byte[4096];
        private Queue<byte[]> m_RecvBufferQueue = new Queue<byte[]>();

        private bool m_IsRunning = false;
        
        //RPC
        private RPCManagerBase m_rpc;


        public void Init(int id)
        {
            m_id = id;
            m_port = IPCConfig.GetIPCInfo(id).port;

            m_rpc = new RPCManagerBase();
            m_rpc.Init();

        }


        public void Clean()
        {
            Stop();

            if (m_rpc != null)
            {
                m_rpc.Clean();
                m_rpc = null;
            }
        }

        public void Dump()
        {
            m_rpc.Dump();
        }


        public bool IsRunning { get { return m_IsRunning; } }

        public void Start()
        {
            Debuger.Log();
            try
            {
                m_SystemSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                m_SystemSocket.Bind(IPUtils.GetIPEndPointAny(AddressFamily.InterNetwork, m_port));
                m_port = (m_SystemSocket.LocalEndPoint as IPEndPoint).Port;

                m_IsRunning = true;


                m_ThreadRecv = new Thread(Thread_Recv) { IsBackground = true };
                m_ThreadRecv.Start();
            }
            catch (Exception e)
            {
                Debuger.LogError(e.Message + e.StackTrace);
                Stop();
            }


        }

        public void Stop()
        {
            Debuger.Log();

            m_IsRunning = false;

            if (m_ThreadRecv != null)
            {
                m_ThreadRecv.Interrupt();
                m_ThreadRecv = null;
            }

            if (m_SystemSocket != null)
            {
                try
                {
                    m_SystemSocket.Shutdown(SocketShutdown.Both);

                }
                catch (Exception e)
                {
                    Debuger.LogWarning(e.Message + e.StackTrace);
                }

                m_SystemSocket.Close();
                m_SystemSocket = null;
            }
        }



        //=================================================================================
        //发送逻辑
        //=================================================================================
        private void SendMessage(int dst, byte[] bytes, int len)
        {
			m_hasSocketException = false;
            int dstPort = IPCConfig.GetIPCInfo(dst).port;
            IPEndPoint ep = IPUtils.GetHostEndPoint("127.0.0.1", dstPort);
            m_SystemSocket.SendTo(bytes, 0, len, SocketFlags.None, ep);
        }


        //=================================================================================
        //接收逻辑
        //=================================================================================
        private bool m_hasSocketException = false;
        private void Thread_Recv()
        {
            while (m_IsRunning)
            {
                try
                {
                    DoReceiveInThread();

                    if (m_hasSocketException)
                    {
                        m_hasSocketException = false;
                        Debuger.LogWarning("连接异常已经恢复");
                    }
                }
                catch (SocketException se)
                {
                    if (!m_hasSocketException)
                    {
                        m_hasSocketException = true;
                    }

                    Debuger.LogWarning("SocketErrorCode:{0}, {1}", se.SocketErrorCode, se.Message);
                    Thread.Sleep(1);
                }
                catch (Exception e)
                {
                    Debuger.LogWarning(e.Message + "\n" + e.StackTrace);
                    Thread.Sleep(1);
                }
            }

            Debuger.LogWarning("End!");
        }


        private void DoReceiveInThread()
        {
            EndPoint remotePoint = IPUtils.GetIPEndPointAny(AddressFamily.InterNetwork, 0);
            int cnt = m_SystemSocket.ReceiveFrom(m_RecvBufferTemp, m_RecvBufferTemp.Length, SocketFlags.None, ref remotePoint);

            if (cnt > 0)
            {
                byte[] dst = new byte[cnt];
                Buffer.BlockCopy(m_RecvBufferTemp, 0, dst, 0, cnt);

                lock (m_RecvBufferQueue)
                {
                    m_RecvBufferQueue.Enqueue(dst);
                }                
            }
        }


        private void DoReceiveInMain()
        {
            lock (m_RecvBufferQueue)
            {
                while (m_RecvBufferQueue.Count > 0)
                {
                    byte[] buffer = m_RecvBufferQueue.Dequeue();

                    IPCMessage msg = PBSerializer.NDeserialize<IPCMessage>(buffer);

                    HandleMessage(msg);
                }
            }
        }


        public void Tick()
        {
            DoReceiveInMain();
        }



        //========================================================================
        //RPC的协议处理方式
        //========================================================================
        private string m_currInvokingName;
        private int m_currInvokingSrc;


        public void AddRPCListener(object listener)
        {
            m_rpc.RegisterListener(listener);
        }

        public void RemoveRPCListener(object listener)
        {
            m_rpc.UnRegisterListener(listener);
        }



        private void HandleMessage(IPCMessage msg)
        {
            RPCMessage rpcmsg = msg.rpc;

            Debuger.Log("[{0}]-> {1}({2})", msg.src, rpcmsg.name, rpcmsg.args.ToListString());

            var helper = m_rpc.GetMethodHelper(rpcmsg.name);
            if (helper != null)
            {
                object[] args  = new object[rpcmsg.args.Length +1];
                List<RPCRawArg> raw_args = rpcmsg.raw_args;
                ParameterInfo[] paramInfo = helper.method.GetParameters();

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

                    args[0] = msg.src;

                    m_currInvokingName = rpcmsg.name;
                    m_currInvokingSrc = msg.src;

                    try
                    {
                        helper.method.Invoke(helper.listener, BindingFlags.NonPublic, null, args, null);
                    }
                    catch (Exception e)
                    {
                        Debuger.LogError("RPC调用出错：{0}\n{1}", e.Message, e.StackTrace);
                    }

                    m_currInvokingName = "";
                    m_currInvokingSrc = 0;

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
            Debuger.Log("->[{0}] {1}({2})", m_currInvokingSrc, name, args.ToListString());

            RPCMessage rpcmsg = new RPCMessage();
            rpcmsg.name = name;
            rpcmsg.args = args;

            IPCMessage msg = new IPCMessage();
            msg.src = m_id;
            msg.rpc = rpcmsg;


            byte[] temp = PBSerializer.NSerialize(msg);
            SendMessage(m_currInvokingSrc, temp, temp.Length);
        }


        public void ReturnError(string errinfo, int errcode = 1)
        {
            var name = "On" + m_currInvokingName + "Error";
            Debuger.LogWarning("->[{0}] {1}({2},{3})", m_currInvokingSrc, name, errinfo, errcode);

            RPCMessage rpcmsg = new RPCMessage();
            rpcmsg.name = name;
            rpcmsg.args = new object[] { errinfo, errcode };

            IPCMessage msg = new IPCMessage();
            msg.src = m_id;
            msg.rpc = rpcmsg;

            byte[] temp = PBSerializer.NSerialize(msg);
            SendMessage(m_currInvokingSrc, temp, temp.Length);
        }




        public void Invoke(int dst, string name, params object[] args)
        {
            Debuger.Log("->[{0}] {1}({2})", dst, name, args.ToListString());

            RPCMessage rpcmsg = new RPCMessage();
            rpcmsg.name = name;
            rpcmsg.args = args;

            IPCMessage msg = new IPCMessage();
            msg.src = m_id;
            msg.rpc = rpcmsg;

            byte[] temp = PBSerializer.NSerialize(msg);
            SendMessage(dst, temp, temp.Length);
        }




    }
}
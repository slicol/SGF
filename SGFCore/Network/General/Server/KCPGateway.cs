/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
 * 网关类（服务器）
 * Gateway class (Server)
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
using System.Net.Sockets;
using System.Text;
using System.Threading;

using SGF.Network.Core;
using SGF.Utils;

namespace SGF.Network.General.Server
{
    public class KcpGateway : IGateway, ILogTag
    {
        public string LOG_TAG { get; protected set; }
        public IPEndPoint LocalEndPoint { get; private set; }
        public static int SessionActiveTimeout = 5;
        private MapList<uint, KcpSession> m_mapSession;
        private bool m_IsRunning = false;
        private ISessionListener m_listener;
        private int m_port;

        private Socket m_socket = null;
        private NetBufferWriter m_bufferReceive = null;
        private Thread m_ThreadRecv;
        private byte[] m_RecvBufferTemp = new byte[4096];
        private bool m_waitStart = false;


        public KcpGateway(int port, ISessionListener listener)
        {
            LOG_TAG = "KcpGateway<" + port + ">";
            this.Log("port:{0}", port);

            m_bufferReceive = new NetBufferWriter();

            m_port = port;
            m_listener = listener;

            m_mapSession = new MapList<uint, KcpSession>();

            Start();
        }

        public void Clean()
        {
            this.Log();
            m_mapSession.Clear();
            Close();
        }

        public void Dump()
        {
            StringBuilder sb = new StringBuilder();

            var dic = m_mapSession.AsDictionary();
            foreach (var pair in dic)
            {
                ISession session = pair.Value;
                sb.AppendLine("\t" + session.ToString());
            }

            this.LogWarning("\nGateway Sessions ({0}):\n{1}", m_mapSession.Count, sb);

        }

        private static long m_lastSessionId = 0;
        private uint NewSessionID()
        {
            return (uint)Interlocked.Increment(ref m_lastSessionId);
        }


        private void Start()
        {
            this.LogWarning("");

            m_IsRunning = true;

            if (m_socket == null)
            {
                m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                m_socket.Bind(new IPEndPoint(IPAddress.Any, m_port));
                LocalEndPoint = m_socket.LocalEndPoint as IPEndPoint;
                m_port = LocalEndPoint.Port; ;

                m_ThreadRecv = new Thread(Thread_Recv) { IsBackground = true };
                m_ThreadRecv.Start();
            }
        }

        private void DoReceiveInThread()
        {
            EndPoint remotePoint = IPUtils.GetIPEndPointAny(AddressFamily.InterNetwork, 0);
            int cnt = m_socket.ReceiveFrom(m_RecvBufferTemp, m_RecvBufferTemp.Length, SocketFlags.None, ref remotePoint);

            if (cnt > 0)
            {
                m_bufferReceive.Attach(m_RecvBufferTemp, cnt);
                byte[] m_32b = new byte[4];
                m_bufferReceive.ReadBytes(m_32b, 0, 4);
                uint sid = BitConverter.ToUInt32(m_32b, 0);

                lock (m_mapSession)
                {
                    KcpSession session = null;

                    if (sid == 0)
                    {
                        //来自Client的第1个包，只能是鉴权包
                        session = new KcpSession(NewSessionID(), m_listener);
                        m_mapSession.Add(session.Id, session);
                    }
                    else
                    {
                        session = m_mapSession[sid];
                    }


                    if (session != null)
                    {
                        session.Active(m_socket, remotePoint as IPEndPoint);
                        session.DoReceiveInGateway(m_RecvBufferTemp, cnt);
                    }
                    else
                    {
                        this.LogWarning("无效的包! sid:{0}", sid);
                        //需要返回给客户端，Session无效了,直接返回一个Sid为0的包
                        //当客户端收到包好，会抛出Session过期事件
                        //因为正常情况下，客户端收到的Session肯定不为0
                        m_socket.SendTo(new byte[4], remotePoint);
                    }
                }
            }
        }




        private void Close()
        {
            this.LogWarning("");

            m_IsRunning = false;

            if (m_ThreadRecv != null)
            {
                m_ThreadRecv.Interrupt();
                m_ThreadRecv = null;
            }

            if (m_socket != null)
            {
                try
                {
                    m_socket.Shutdown(SocketShutdown.Both);

                }
                catch (Exception e)
                {
                    this.LogWarning(e.Message + e.StackTrace);
                }

                m_socket.Close();
                m_socket = null;
            }
        }



        //=================================================================================
        //接收线程 
        //=================================================================================
        private bool m_hasSocketException = false;

        private void Thread_Recv()
        {
            Debuger.LogWarning("Begin ......");

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
                    
                    Debuger.LogWarning("SocketErrorCode:{0}, {1}", se.SocketErrorCode,se.Message);
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



        private int m_lastClearSessionTime = 0;
        public void Tick()
        {
            if (m_IsRunning)
            {
                lock (m_mapSession)
                {
                    int current = (int)TimeUtils.GetTotalMillisecondsSince1970();

                    if (current - m_lastClearSessionTime > SessionActiveTimeout * 1000 / 2)
                    {
                        m_lastClearSessionTime = current;
                        ClearNoActiveSession();
                    }

                    var list = m_mapSession.AsList();
                    int cnt = list.Count;
                    for (int i = 0; i < cnt; i++)
                    {
                        list[i].Tick(current);
                    }
                }
            }
        }

        private void ClearNoActiveSession()
        {
            var list = m_mapSession.AsList();
            var dir = m_mapSession.AsDictionary();
            int cnt = list.Count;

            for (int i = cnt - 1; i >= 0; i--)
            {
                var session = list[i];
                if (!session.IsActived)
                {
                    list.RemoveAt(i);
                    dir.Remove(session.Id);
                    session.Clean();
                }
            }
        }
    }
}
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
using SGF.Network.General.Proto;
using SGF.Utils;

namespace SGF.Network.General.Server
{
    public class UdpGateway:IGateway,ILogTag
    {
        public string LOG_TAG { get; protected set; }
        public IPEndPoint LocalEndPoint { get; private set; }
        public static int SessionActiveTimeout = 5;
        private MapList<uint, UdpSession> m_mapSession;
        private bool m_IsRunning = false;
        private ISessionListener m_listener;
        private int m_port;

        private Socket m_socket = null;
        private SocketAsyncEventArgs m_saeReceive = null;
        private NetBufferWriter m_bufferReceive = null;
        private SocketAsyncEventArgsPool m_pool;

        public UdpGateway(int port, ISessionListener listener)
        {
            LOG_TAG = "UdpGateway<" + port + ">";
            this.Log("port:{0}", port);

            m_bufferReceive = new NetBufferWriter();
            m_pool = new SocketAsyncEventArgsPool(NetDefine.PacketBufferSize, 10000);

            m_port = port;
            m_listener = listener;

            m_mapSession = new MapList<uint, UdpSession>();

            Start();
        }

        public void Clean()
        {
            this.Log();
            m_mapSession.Clear();
            Close();
        }

        private void CleanReceive()
        {
            this.Log();

            if (m_saeReceive != null)
            {
                m_saeReceive.Completed -= OnReceiveCompleted;
                m_pool.Release(m_saeReceive);
                m_saeReceive = null;
            }
        }

        public void Dump()
        {
            StringBuilder sb = new StringBuilder();

            var dic = m_mapSession.AsDictionary();
            var i = 0;
            foreach (var pair in dic)
            {
                i++;
                ISession session = pair.Value;
                sb.AppendLine("\t" + i + "." + session.ToString());
            }

            Debuger.LogWarning("\nGateway Sessions ({0}):\n{1}", m_mapSession.Count, sb);

        }


        private static long m_lastSessionId = 0;
        private uint NewSessionID()
        {
            return (uint)Interlocked.Increment(ref m_lastSessionId);
        }

        public void Start()
        {
            this.LogWarning("");

            m_IsRunning = true;

            if (m_socket == null)
            {
                m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                m_socket.Bind(new IPEndPoint(IPAddress.Any, m_port));
                LocalEndPoint = m_socket.LocalEndPoint as IPEndPoint;
                m_port = LocalEndPoint.Port; ;

                m_saeReceive = m_pool.Acquire();
                m_saeReceive.Completed += OnReceiveCompleted;
                m_saeReceive.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

                ReceiveAsync();
            }
        }

        private void ReceiveAsync()
        {
            if (m_socket == null)
            {
                return;
            }

            bool result = true;
            try { result = m_socket.ReceiveFromAsync(m_saeReceive); }
            catch (Exception ex) { Debuger.LogError(ex.Message); }

            if (!result)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(_ => OnReceiveCompleted(this, m_saeReceive));
            }
        }

        private void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            this.Log(e.SocketError.ToString());

            if (e.SocketError != SocketError.Success)
            {
                this.LogWarning(e.SocketError.ToString());
            }
            else
            {
                if (e.BytesTransferred == 0)
                {
                    this.LogWarning("收到的数据长度为0");
                }
                else
                {
                    m_bufferReceive.Attach(e.Buffer, e.BytesTransferred);
                    NetMessage msg = new NetMessage();
                    if (msg.Deserialize(m_bufferReceive))
                    {
                        lock (m_mapSession)
                        {
                            UdpSession session = null;

                            if (msg.head.sid == 0)
                            {
                                session = new UdpSession(NewSessionID(), m_pool, m_listener);
                                m_mapSession.Add(session.Id, session);
                            }
                            else
                            {
                                session = m_mapSession[msg.head.sid];
                                if (session == null)
                                {
                                    //对于UDP，可以重新分配SessionId
                                    session = new UdpSession(NewSessionID(), m_pool, m_listener);
                                    m_mapSession.Add(session.Id, session);
                                }
                            }


                            if (session != null)
                            {
                                session.Active(m_socket, (IPEndPoint)e.RemoteEndPoint);
                                session.DoReceiveInGateway(msg);
                            }
                            else
                            {
                                this.LogWarning("无效的包! sid:{0}", msg.head.sid);
                                //需要返回给客户端，Session无效了
                                UdpSession.ReturnErrorMessage(m_socket, (IPEndPoint) e.RemoteEndPoint,
                                    NetErrorCode.SessionExpire, "Session Expired!");
                            }
                        }

                    }
                    else
                    {
                        this.LogError("反序列化失败！");
                    }
                    
                }
            }
            
            this.ReceiveAsync();
        }



        private void Close()
        {
            this.LogWarning("");

            m_IsRunning = false;

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

            CleanReceive();
        }


        //=================================================================================
        //检查超时 
        //=================================================================================

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
/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
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
    public class TcpGateway:IGateway
    {
        public static int SessionActiveTimeout = 5;
        private MapList<long, TcpSession> m_mapSession;
        private bool m_IsRunning = false;
        private ISessionListener m_listener;
        private int m_port;
        private bool m_waitReStart = false;

        
        private const int BACKLOG = 500;
        private Socket m_socket = null;
        private readonly SocketAsyncEventArgs m_saeAccept = null;

        private SocketAsyncEventArgsPool m_saePool;

        public TcpGateway(int port, ISessionListener listener)
        {
            Debuger.Log("port:{0}", port);

            m_saePool = new SocketAsyncEventArgsPool(NetDefine.PacketBufferSize, 10000);
            m_saeAccept = new SocketAsyncEventArgs();
            m_saeAccept.Completed += OnAcceptCompleted;
            
            m_port = port;
            m_listener = listener;

            m_mapSession = new MapList<long, TcpSession>();

            Start();
        }

        public void Clean()
        {
            Debuger.Log();
            m_mapSession.Clear();
            Close();
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
                sb.AppendLine("\t" + i  + "."+ session.ToString());
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
            Debuger.LogWarning("");

            m_waitReStart = false;
            m_IsRunning = true;

            if (m_socket == null)
            {
                m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                m_socket.Bind(new IPEndPoint(IPAddress.Any, m_port));
                m_socket.Listen(BACKLOG);

                AcceptAsync();
            }
        }

        private void AcceptAsync()
        {
            if (m_socket == null)
            {
                return;
            }

            bool result = true;
            try { result = m_socket.AcceptAsync(m_saeAccept); }
            catch (Exception ex) { Debuger.LogError(ex.Message); }

            if (!result) System.Threading.ThreadPool.QueueUserWorkItem(_ => OnAcceptCompleted(this, m_saeAccept));
        }

        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            Socket acceptSocket = null;
            if (e.SocketError == SocketError.Success)
            {
                acceptSocket = e.AcceptSocket;
            }
            e.AcceptSocket = null;

            if (acceptSocket != null)
            {
                var sid = NewSessionID();
                var session = new TcpSession(sid, m_saePool, m_listener);
                session.Active(acceptSocket);

                lock (m_mapSession)
                {
                    m_mapSession.Add(session.Id, session);
                }
                
            }

            this.AcceptAsync();
        }

        

        private void Close()
        {
            Debuger.LogWarning("");

            m_IsRunning = false;

            if (m_socket != null)
            {
                try
                {
                    m_socket.Shutdown(SocketShutdown.Both);

                }
                catch (Exception e)
                {
                    Debuger.LogWarning(e.Message + e.StackTrace);
                }

                m_socket.Close();
                m_socket = null;
            }
        }

        private void ReStart()
        {
            Debuger.LogWarning("");
            Close();
            m_waitReStart = true;
        }

        public ISession GetSession(long sid)
        {
            ISession session = null;
            lock (m_mapSession)
            {
                session = m_mapSession[sid];
            }
            return session;
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
            else
            {
                if (m_waitReStart)
                {
                    Start();
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
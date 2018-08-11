/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
 * 帧同步模块
 * Frame synchronization module
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

namespace SGF.Network.FSPLite.Server
{
    
    public class FSPGateway
    {
        private static uint ms_lastSid = 0;
        public static uint NewSessionID()
        {
            return ++ms_lastSid;
        }


        //=====================================================================

        //基础数据
        private bool m_IsRunning = false;
        public bool IsRunning { get { return m_IsRunning; } }


        //线程模块
        private Thread m_ThreadRecv;


        //通讯部分
        private Socket m_SystemSocket;
        private byte[] m_RecvBufferTemp = new byte[4096];
        private NetBufferReader m_RecvBufferTempReader = new NetBufferReader();
        private int m_port;
        

        //Session管理
        private MapList<uint, FSPSession> m_mapSession;



        public void Init(int port)
        {
            Debuger.Log("port:{0}", port);
            m_port = port;
            m_mapSession = new MapList<uint, FSPSession>();
            Start();
        }

        public void Clean()
        {
            Debuger.Log();
            m_mapSession.Clear();
            Close();
        }

        public int Port
        {
            get { return (m_SystemSocket.LocalEndPoint as IPEndPoint).Port; }
        }

        public string Host
        {
            get { return IPUtils.SelfIP; }
        }

        public void Dump()
        {
            StringBuilder sb = new StringBuilder();

            var dic = m_mapSession.AsDictionary();
            foreach (var pair in dic)
            {
                var session = pair.Value;
                sb.AppendLine("\t" + session.ToString());
            }

            Debuger.LogWarning("\nFSPGateway Sessions ({0}):\n{1}", m_mapSession.Count, sb);

        }


        public void Start()
        {
            Debuger.Log();

            m_IsRunning = true;

            m_SystemSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            m_SystemSocket.Bind(IPUtils.GetIPEndPointAny(AddressFamily.InterNetwork, m_port));

            m_ThreadRecv = new Thread(Thread_Recv) { IsBackground = true };
            m_ThreadRecv.Start();
        }



        public void Close()
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


        public FSPSession CreateSession()
        {
            Debuger.Log();
            uint sid = NewSessionID();
            FSPSession session = new FSPSession(sid, HandleSessionSend);
            m_mapSession.Add(sid, session);
            return session;
        }


        public FSPSession GetSession(uint sid)
        {
            FSPSession session = null;
            lock (m_mapSession)
            {
                session = m_mapSession[sid];
            }
            return session;
        }


        private void HandleSessionSend(IPEndPoint remoteEndPoint, byte[] buffer, int size)
        {
            if (m_SystemSocket != null)
            {
                int cnt = m_SystemSocket.SendTo(buffer, 0, size, SocketFlags.None, remoteEndPoint);
            }
            else
            {
                Debuger.LogError("Socket已经关闭！");
            }
        }

        //=================================================================================
        //接收线程 
        //=================================================================================

        private void Thread_Recv()
        {
            while (m_IsRunning)
            {
                try
                {
                    DoReceiveInThread();
                }
                catch (Exception e)
                {
                    Debuger.LogWarning(e.Message + "\n" + e.StackTrace);
                    Thread.Sleep(1);
                }

            }
        }


        private void DoReceiveInThread()
        {
            EndPoint remotePoint = IPUtils.GetIPEndPointAny(AddressFamily.InterNetwork, 0);
            int cnt = m_SystemSocket.ReceiveFrom(m_RecvBufferTemp, m_RecvBufferTemp.Length, SocketFlags.None, ref remotePoint);

            if (cnt > 0)
            {
                
                m_RecvBufferTempReader.Attach(m_RecvBufferTemp, cnt);
                byte[] m_32b = new byte[4];
                m_RecvBufferTempReader.ReadBytes(m_32b, 0, 4);
                uint sid = BitConverter.ToUInt32(m_32b, 0);

                lock (m_mapSession)
                {
                    FSPSession session = null;
                    if (sid == 0)
                    {
                        Debuger.LogError("基于KCP的Sid为0，该包需要被丢掉");
                    }
                    else
                    {
                        session = m_mapSession[sid];
                    }

                    if (session != null)
                    {
                        session.Active(remotePoint as IPEndPoint);
                        session.DoReceiveInGateway(m_RecvBufferTemp, cnt);
                    }
                    else
                    {
                        Debuger.LogWarning("无效的包! sid:{0}", sid);
                    }
                }

            }
        }




        //=================================================================================
        //时钟信号 
        //=================================================================================

        private uint m_lastClearSessionTime = 0;
        public void Tick()
        {
            if (m_IsRunning)
            {
                lock (m_mapSession)
                {
                    uint current = (uint)TimeUtils.GetTotalMillisecondsSince1970();

                    if (current - m_lastClearSessionTime > FSPSession.ActiveTimeout * 1000 / 2)
                    {
                        m_lastClearSessionTime = current;
                        ClearNoActiveSession();
                    }


                    var list = m_mapSession.AsList();
                    int cnt = list.Count;
                    for (int i = 0; i < cnt; i++)
                    {
                        var session = list[i];
                        session.Tick(current);
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
                if (!session.IsActived())
                {
                    list.RemoveAt(i);
                    dir.Remove(session.id);
                }
            }
        }

    }
}
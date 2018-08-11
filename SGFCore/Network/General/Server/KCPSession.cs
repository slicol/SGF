/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
 * 基于KCP的可靠UDP会话类（服务器）
 * KCP-based reliable UDP session class (server)
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
using System.Text;
using System.Threading;
using SGF.Network.Core;
using SGF.Network.General.Proto;
using SGF.SEvent;
using SGF.Time;


namespace SGF.Network.General.Server
{
    public class KcpSession:ISession, ILogTag
    {
        public string LOG_TAG { get; protected set; }

        //==========================================================
        //事件
        //==========================================================
        public Signal<ISession, int> onSendError { get; private set; }
        public Signal<ISession, int> onReceiveError { get; private set; }

        //==========================================================
        //公共成员变量
        //==========================================================
        public uint AuthToken { get; set; }
        public uint Id { get; protected set; }
        public IPEndPoint LocalEndPoint { get; private set; }
        public IPEndPoint RemoteEndPoint { get; private set; }
        public ushort Ping { get; set; }
        public bool IsActived { get { return Thread.VolatileRead(ref this.m_actived) == 1; } }
        private int m_actived = 0;

        //==========================================================
        //私有成员变量
        //==========================================================
        private Socket m_socket;
        private ISessionListener m_listener;

        private NetBufferWriter m_bufferSend;
        private Queue<NetMessage> m_queueReceive;

        //KCP
        private KCP m_Kcp;
        private SwitchQueue<byte[]> m_RecvBufQueue = new SwitchQueue<byte[]>();
        private uint m_NextKcpUpdateTime = 0;
        private bool m_NeedKcpUpdateFlag = false;


        public KcpSession(uint sid, ISessionListener listener)
        {
            LOG_TAG = "KcpSession[" + sid + "]";
            this.Log();

            onSendError = new Signal<ISession, int>();
            onReceiveError = new Signal<ISession, int>();

            m_bufferSend = new NetBufferWriter(new byte[0]);
            m_queueReceive = new Queue<NetMessage>();

            this.Id = sid;
            m_listener = listener;


        }

        public string ToString(string prefix = "")
        {
            return "KcpSession[" + Id + "," + RemoteEndPoint.Port + "]";
        }

        public virtual void Clean()
        {
            this.Log();

            m_socket = null;
            m_actived = 0;
        }

        //======================================================================
        //连接与断开连接相关
        //======================================================================
        public void Active(Socket socket, IPEndPoint remoteEndPoint)
        {
            RemoteEndPoint = (IPEndPoint)remoteEndPoint;

            if (Interlocked.CompareExchange(ref this.m_actived, 1, 0) == 0)
            {
                m_socket = socket;

                LocalEndPoint = (IPEndPoint)socket.LocalEndPoint;

                m_Kcp = new KCP(Id, HandleKcpSend);
                m_Kcp.NoDelay(1, 10, 2, 1);
                m_Kcp.WndSize(128, 128);
            }
        }


        //======================================================================
        //发送数据
        //======================================================================
        public bool Send(NetMessage msg)
        {
            this.Log();

            if (!IsActived)
            {
                Debuger.LogWarning("Session已经不活跃了！");
                return false;
            }


            msg.head.sid = Id;

            m_bufferSend.Attach(new byte[msg.Length], 0);
            msg.Serialize(m_bufferSend);

            var bytes = m_bufferSend.GetBytes();
            var len = m_bufferSend.Length;

            return m_Kcp.Send(bytes, len) == 0;
            
        }

        private void HandleKcpSend(byte[] bytes, int len)
        {
            m_socket.SendTo(bytes, 0, len, SocketFlags.None, RemoteEndPoint);
        }


        //======================================================================
        //接收数据
        //======================================================================

        public void DoReceiveInGateway(byte[] buffer, int size)
        {
            byte[] dst = new byte[size];
            Buffer.BlockCopy(buffer, 0, dst, 0, size);
            m_RecvBufQueue.Push(dst);
        }

        private void DoReceiveInMain()
        {
            m_RecvBufQueue.Switch();

            while (!m_RecvBufQueue.Empty())
            {
                var recvBufferRaw = m_RecvBufQueue.Pop();
                int ret = m_Kcp.Input(recvBufferRaw, recvBufferRaw.Length);

                //收到的不是一个正确的KCP包
                if (ret < 0)
                {
                    Debuger.LogError("收到不正确的KCP包!Ret:{0}", ret);
                    return;
                }

                m_NeedKcpUpdateFlag = true;

                for (int size = m_Kcp.PeekSize(); size > 0; size = m_Kcp.PeekSize())
                {
                    var recvBuffer = new byte[size];
                    if (m_Kcp.Recv(recvBuffer) > 0)
                    {
                        NetMessage msg = new NetMessage();
                        msg.Deserialize(recvBuffer, size);

                        m_listener.OnReceive(this, msg);
                    }
                }

            }
        }


        public void Tick(int currentTimeMS)
        {
            DoReceiveInMain();

            uint current = (uint)currentTimeMS;

            if (m_NeedKcpUpdateFlag || current >= m_NextKcpUpdateTime)
            {
                m_Kcp.Update(current);
                m_NextKcpUpdateTime = m_Kcp.Check(current);
                m_NeedKcpUpdateFlag = false;
            }
        }



    }
}
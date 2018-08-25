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
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SGF.Codec;
using SGF.Network.Core;
using SGF.Network.General.Proto;
using SGF.SEvent;

namespace SGF.Network.General.Server
{
    public class UdpSession:ISession, ILogTag
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
        public bool IsActived{get { return Thread.VolatileRead(ref this.m_actived) == 1; }}
        private int m_actived = 0;


        //==========================================================
        //私有成员变量
        //==========================================================
        private Socket m_socket;
        private SocketAsyncEventArgsPool m_pool;
        private ISessionListener m_listener;

        //发送相关
        private SocketAsyncEventArgs m_saeSend = null;
        private NetPacket m_currSendingPacket = null;
        private Queue<NetPacket> m_queueSend;
        private NetBufferWriter m_bufferSend;
        private int m_sending = 0;

        //接收相关
        private Queue<NetMessage> m_queueReceive;


        public UdpSession(uint sid, SocketAsyncEventArgsPool pool, ISessionListener listener)
        {
            LOG_TAG = "UdpSession[" + sid + "]";
            this.Log();

            onSendError = new Signal<ISession, int>();
            onReceiveError = new Signal<ISession, int>();

            m_pool = pool;

            m_queueSend = new Queue<NetPacket>();
            m_bufferSend = new NetBufferWriter(new byte[0]);
            
            m_queueReceive = new Queue<NetMessage>();

            this.Id = sid;
            m_listener = listener;
        }

        public override string ToString()
        {
            return "UdpSession[" + Id + "," + RemoteEndPoint.Port + "]";
        }

        public virtual void Clean()
        {
            this.Log();

            m_socket = null;
            m_actived = 0;

            CleanSend();
        }

        private void CleanSend()
        {
            this.Log();

            m_currSendingPacket = null;

            if (m_saeSend != null)
            {
                m_saeSend.Completed -= OnSendCompleted;
                m_pool.Release(m_saeSend);
                m_saeSend = null;
            }
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

                m_saeSend = m_pool.Acquire();
                m_saeSend.Completed += OnSendCompleted;
                m_saeSend.RemoteEndPoint = remoteEndPoint;

                BeginSend();
            }
        }

        //======================================================================
        //发送数据
        //======================================================================

        public bool Send(NetMessage msg)
        {
            this.Log();

            msg.head.sid = Id;

            m_bufferSend.Attach(new byte[msg.Length], 0);
            msg.Serialize(m_bufferSend);

            NetPacket packet = new NetPacket(m_bufferSend.GetBytes());

            lock (m_queueSend)
            {
                m_queueSend.Enqueue(packet);
            }

            if (m_sending == 0 && m_actived == 1)
            {
                if (m_currSendingPacket == null)
                {
                    lock (m_queueSend)
                    {
                        if (m_queueSend.Count > 0)
                        {
                            m_currSendingPacket = m_queueSend.Dequeue();
                            SendInternal(m_saeSend);
                        }
                    }
                }
            }

            return true;
        }

        protected void BeginSend()
        {
            TrySendNext();
        }

        private void TrySendNext()
        {
            this.Log();

            if (m_sending == 0)
            {
                if (m_currSendingPacket == null)
                {
                    lock (m_queueSend)
                    {
                        if (m_queueSend.Count > 0)
                        {
                            m_currSendingPacket = m_queueSend.Dequeue();
                            SendInternal(m_saeSend);
                        }
                    }
                }
            }
        }

        private void SendInternal(SocketAsyncEventArgs e)
        {
            this.Log(e.SocketError.ToString());

            m_sending = 1;

            var packet = m_currSendingPacket;

            var length = Math.Min(packet.Bytes.Length - packet.SentSize, e.Buffer.Length);
            var result = false;
            try
            {
                Buffer.BlockCopy(packet.Bytes, packet.SentSize, e.Buffer, 0, length);
                e.SetBuffer(0, length);
                result = m_socket.SendToAsync(e);
            }
            catch (Exception ex)
            {
                this.LogWarning("发送数据出错：{0}", ex.Message);
            }

            if (!result)
            {
                ThreadPool.QueueUserWorkItem(_ => OnSendCompleted(this, e));
            }
        }

        private void OnSendCompleted(object sender, SocketAsyncEventArgs e)
        {
            this.Log(e.SocketError.ToString());

            if (e.SocketError != SocketError.Success)
            {
                this.LogWarning("发送数据出错：{0}", e.SocketError);
                onSendError.Invoke(this, (int)NetErrorCode.UnkownError);
                return;
            }

            var packed = m_currSendingPacket;
            packed.SentSize += e.BytesTransferred;

            //将PackedBufferSize的Buffer发送完
            if (e.Offset + e.BytesTransferred < e.Count)
            {
                var result = true;
                try
                {
                    e.SetBuffer(e.Offset + e.BytesTransferred, e.Count - e.BytesTransferred - e.Offset);
                    result = m_socket.SendToAsync(e);
                }
                catch (Exception ex)
                {
                    this.LogWarning("发送数据出错：{0}", ex.Message);
                }

                if (!result)
                {
                    ThreadPool.QueueUserWorkItem(_ => OnSendCompleted(this, e));
                }
            }
            else
            {

                if (!packed.IsSent)
                {
                    //继续发当前Packet的下一个分组
                    SendInternal(e);
                }
                else
                {
                    //一个Packet发送完成
                    m_currSendingPacket = null;
                    m_sending = 0;
                    
                    //继续发下一个Packet
                    TrySendNext();
                }
            }
        }





        //======================================================================
        //接收数据
        //======================================================================

        internal void DoReceiveInGateway(NetMessage msg)
        {
            lock (m_queueReceive)
            {
                m_queueReceive.Enqueue(msg);
            }
        }

        public NetMessage Receive()
        {
            NetMessage msg = null;
            lock (m_queueReceive)
            {
                if (m_queueReceive.Count > 0)
                {
                    msg = m_queueReceive.Dequeue();
                    return msg;
                }
            }

            return null;

        }

        public void Tick(int currentMS)
        {
            var msg = Receive();
            while (msg != null)
            {
                m_listener.OnReceive(this, msg);
                msg = Receive();
            }
        }



        internal static void ReturnErrorMessage(Socket socket, IPEndPoint remoteEndPoint, NetErrorCode errcode, string errinfo = "")
        {
            NetErrorMessage msg = new NetErrorMessage();
            msg.code = (int)errcode;
            msg.info = errinfo;

            NetMessage msgobj = new NetMessage();
            msgobj.head.index = 0;
            msgobj.head.cmd = 0;
            msgobj.head.token = 0;
            msgobj.head.sid = 0;
            msgobj.content = PBSerializer.NSerialize(msg);
            msgobj.head.dataSize = (uint)msgobj.content.Length;

            NetBuffer bufferSend = new NetBuffer(new byte[msgobj.Length]);
            msgobj.Serialize(bufferSend);
            socket.SendTo(bufferSend.GetBytes(), remoteEndPoint);
        }
    }
}
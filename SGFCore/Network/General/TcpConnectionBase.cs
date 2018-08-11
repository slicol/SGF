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
using SGF.Network.Core;
using SGF.Network.General.Proto;


namespace SGF.Network.General
{
    public class TcpConnectionBase:ILogTag
    {
        public string LOG_TAG { get; protected set; }


        //==========================================================
        //公共成员变量
        //==========================================================
        public uint Id { get; protected set; }
        public IPEndPoint LocalEndPoint { get; private set; }
        public IPEndPoint RemoteEndPoint { get; private set; }
        public bool IsActived { get { return Thread.VolatileRead(ref this.m_actived) == 1; } }
        private int m_actived = 0;
        public ushort Ping { get; set; }

        //==========================================================
        //私有成员变量
        //==========================================================
        private Socket m_socket;
        private SocketAsyncEventArgsPool m_pool;

        //发送相关
        private SocketAsyncEventArgs m_saeSend = null;
        private NetPacket m_currSendingPacket = null;
        private Queue<NetPacket> m_queueSend;
        private NetBufferWriter m_bufferSend;
        private int m_sending = 0;

        //接收相关
        private SocketAsyncEventArgs m_saeReceive = null;
        private Queue<NetMessage> m_queueReceive;
        private NetBufferWriter m_bufferReceive;


        //==========================================================
        //构造与清理
        //==========================================================
        public TcpConnectionBase(SocketAsyncEventArgsPool pool = null)
        {
            LOG_TAG = "TcpConnectionBase";

            m_pool = pool;
            if (m_pool == null)
            {
                m_pool = new SocketAsyncEventArgsPool(NetDefine.PacketBufferSize, 0);
            }

            m_queueSend = new Queue<NetPacket>();
            m_bufferSend = new NetBufferWriter(new byte[0]);

            m_queueReceive = new Queue<NetMessage>();
            m_bufferReceive = new NetBufferWriter(new byte[NetDefine.ReceiveBufferMinSize]);
        }

        public virtual void Clean()
        {
            this.Log();

            m_actived = 0;

            Close();
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


        //======================================================================
        //连接（对于Client和Server，以及UDP和TCP，连接的逻辑不同）与断开连接
        //======================================================================

        public void Active(Socket socket)
        {
            if (m_actived == 0)
            {
                m_socket = socket;

                socket.NoDelay = true;
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
                socket.ReceiveBufferSize = NetDefine.SocketBufferSize;
                socket.SendBufferSize = NetDefine.SocketBufferSize;

                RemoteEndPoint = (IPEndPoint)socket.RemoteEndPoint;
                LocalEndPoint = (IPEndPoint)socket.LocalEndPoint;

                m_saeSend = m_pool.Acquire();
                m_saeSend.Completed += OnSendCompleted;

                m_saeReceive = m_pool.Acquire();
                m_saeReceive.Completed += OnReceiveCompleted;

                m_actived = 1;

                BeginSend();
                BeginReceive();

                
            }
        }
        

        public void Close()
        {
            this.Log();

            if (m_socket != null)
            {
                try { m_socket.Shutdown(SocketShutdown.Both); }
                catch (Exception ex) { this.LogWarning(ex.Message); }

                m_socket.Close();
                m_socket = null;
            }

            CleanSend();
            CleanReceive();

            if (Interlocked.CompareExchange(ref this.m_actived, 0, 1) == 1)
            {
                Disconnect();
            }
        }

        private void Disconnect()
        {
            this.Log();

            m_actived = 0;
            m_sending = 0;

            CleanSend();
            CleanReceive();

            var e = new SocketAsyncEventArgs();
            e.Completed += OnDisconnectCompleted;

            var result = false;
            try
            {
                m_socket.Shutdown(SocketShutdown.Both);
                result = m_socket.DisconnectAsync(e);
            }
            catch (Exception ex)
            {
                //this.LogError(ex.Message);
            }

            if (!result)
            {
                ThreadPool.QueueUserWorkItem(_ => OnDisconnectCompleted(this, e));
            }

        }


        private void OnDisconnectCompleted(object sender, SocketAsyncEventArgs e)
        {
            this.Log(e.SocketError.ToString());

            e.Completed -= OnDisconnectCompleted;
            e.Dispose();

            try { m_socket.Close(); }
            catch (Exception ex) {  }

            OnDisconnected();
        }

        protected virtual void OnDisconnected()
        {
            
        }

        //======================================================================
        //发送数据
        //======================================================================
        public bool Send(NetMessage msg)
        {
            this.LogVerbose();

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
                lock (m_queueSend)
                {
                    if (m_currSendingPacket == null)
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
            this.LogVerbose();

            if (m_sending == 0)
            {
                lock (m_queueSend)
                {
                    if (m_currSendingPacket == null)
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
            this.LogVerbose("{0}",e.SocketError);

            m_sending = 1;

            var packet = m_currSendingPacket;

            var length = Math.Min(packet.Bytes.Length - packet.SentSize, e.Buffer.Length);
            var result = false;
            try
            {
                Buffer.BlockCopy(packet.Bytes, packet.SentSize, e.Buffer, 0, length);
                e.SetBuffer(0, length);
                result = m_socket.SendAsync(e);
            }
            catch (Exception ex)
            {
                this.LogError("发送数据出错：{0}", ex.Message);
            }

            if (!result)
            {
                ThreadPool.QueueUserWorkItem(_ => OnSendCompleted(this, e));
            }
        }

        private void OnSendCompleted(object sender, SocketAsyncEventArgs e)
        {
            this.LogVerbose(e.SocketError.ToString());

            if (e.SocketError != SocketError.Success)
            {
                Disconnect();
                OnSendError((int)NetErrorCode.UnkownError, e.SocketError.ToString());
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
                    result = m_socket.SendAsync(e);
                }
                catch (Exception ex)
                {
                    this.LogError("发送数据出错：{0}", ex.Message);
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
                    OnSendSuccess();

                    //继续发下一个Packet
                    TrySendNext();
                }
            }
        }

        protected virtual void OnSendError(int errcode, string info)
        {
            this.LogError("{0}:{1}",errcode, info);
        }

        protected virtual void OnSendSuccess()
        {
            
        }


        //======================================================================
        //接收数据
        //======================================================================
        protected void BeginReceive()
        {
            ReceiveInternal();
        }

        private void ReceiveInternal()
        {
            this.LogVerbose();

            bool result = false;
            try{result = m_socket.ReceiveAsync(m_saeReceive);}
            catch (Exception ex){this.LogError("接收数据出错：{0}", ex.Message);}

            if (!result)
            {
                ThreadPool.QueueUserWorkItem(_ => OnReceiveCompleted(this, m_saeReceive));
            }

        }

        private void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            this.LogVerbose(e.SocketError.ToString());

            if (e.SocketError != SocketError.Success)
            {
                Disconnect();
                OnReceiveError((int)NetErrorCode.UnkownError, e.SocketError.ToString());
                return;
            }

            if (e.BytesTransferred < 1)
            {
                Disconnect();
                OnReceiveError((int)NetErrorCode.UnkownError, "收到的字节为0");
                return;
            }

            if (m_bufferReceive.Capacity < m_bufferReceive.Length + e.BytesTransferred)
            {
                //扩容
                m_bufferReceive.AdjustCapacity((m_bufferReceive.Length + e.BytesTransferred) * 2);
            }

            m_bufferReceive.SetPositionToLength();
            m_bufferReceive.WriteBytes(e.Buffer, 0, e.BytesTransferred);

            this.LogVerbose("Received Length:{0}", m_bufferReceive.Length);

            while (m_bufferReceive.Length > 0)
            {
                m_bufferReceive.Position = 0;

                NetMessage msg = new NetMessage();
                if (msg.Deserialize(m_bufferReceive))
                {
                    m_bufferReceive.Arrangement();
                    lock (m_queueReceive)
                    {
                        m_queueReceive.Enqueue(msg);
                    }
                }
                else
                {
                    this.LogVerbose("Cannot Deserialize One Message!");
                    break;
                }
            }


            ReceiveInternal();
        }

        protected virtual void OnReceiveError(int errcode, string info)
        {
            this.LogError("{0}:{1}", errcode, info);
        }


        /// <summary>
        /// 主线程读取数据
        /// </summary>
        /// <returns></returns>
        public NetMessage Receive()
        {
            NetMessage msg = null;
            lock (m_queueReceive)
            {
                if(m_queueReceive.Count >0)
                {
                    msg = m_queueReceive.Dequeue();
                    return msg;
                }
            }

            return null;
            
        }



    }
}
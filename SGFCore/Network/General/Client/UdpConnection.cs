/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
 * 基于UDP的不可靠连接类（客户端）
 * UDP-based unreliable connection class (client)
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

namespace SGF.Network.General.Client
{
    public class UdpConnection: IConnection, ILogTag
    {
        public string LOG_TAG { get; protected set; }

        //==========================================================
        //事件
        //==========================================================
        public Signal<IConnection, int, string> onSendError { get; private set; }
        public Signal<IConnection, int, string> onReceiveError { get; private set; }
        public Signal<IConnection, int, string> onServerError { get; private set; }

        /// <summary>
        /// 当收到数据时发出信号
        /// 参数：字节数组，长度
        /// </summary>
        public Signal<NetMessage> onReceive { get; private set; }

        //==========================================================
        //公共成员变量
        //==========================================================
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
        private SocketAsyncEventArgsPool m_pool;

        private List<IPEndPoint> m_listRemoteEndPoint;
        private int m_currRemoteEndPointIndex = -1;
        private int m_localPort;

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
        //构造函数
        //==========================================================
        public UdpConnection(int localPort, SocketAsyncEventArgsPool pool)
        {
            LOG_TAG = "UdpConnection[" + 0 + "," + localPort + "]";
            this.Log("connId:{0}, localPort:{1}", 0, localPort);

            onReceive = new Signal<NetMessage>();
            onSendError = new Signal<IConnection, int, string>();
            onReceiveError = new Signal<IConnection, int, string>();
            onServerError = new Signal<IConnection, int, string>();

            m_listRemoteEndPoint = new List<IPEndPoint>();
            m_currRemoteEndPointIndex = -1;

            m_pool = new SocketAsyncEventArgsPool(NetDefine.PacketBufferSize, 0);

            m_queueSend = new Queue<NetPacket>();
            m_bufferSend = new NetBufferWriter(new byte[0]);

            m_queueReceive = new Queue<NetMessage>();
            m_bufferReceive = new NetBufferWriter(new byte[NetDefine.ReceiveBufferMinSize]);

            m_localPort = localPort;
        }


        public void Clean()
        {
            this.Log();
            
            onReceive.RemoveAllListeners();
            onReceiveError.RemoveAllListeners();
            onSendError.RemoveAllListeners();
            onServerError.RemoveAllListeners();

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
        //连接与断开连接
        //======================================================================
        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="remoteIP"></param>
        /// <param name="remotePort"></param>
        public void Connect(string remoteIP, int remotePort)
        {
            this.Log("{0}:{1}", remoteIP, remotePort);

            IPAddress ipaddr = null;
            if (!IPAddress.TryParse(remoteIP, out ipaddr))
            {
                this.LogError("无法解析为有效的IPAddress:{0}", remoteIP);
                return;
            }

            m_listRemoteEndPoint.Clear();
            m_listRemoteEndPoint.Add(new IPEndPoint(ipaddr, remotePort));
            m_currRemoteEndPointIndex = 0;

            ConnectInternal(m_listRemoteEndPoint[m_currRemoteEndPointIndex]);
        }

        public void Connect(IPEndPoint[] listRemoteEndPoint)
        {

            if (listRemoteEndPoint.Length == 0)
            {
                this.LogError("参数错误：listRemoteEndPoint.Length = 0");
                return;
            }

            m_listRemoteEndPoint.Clear();
            m_listRemoteEndPoint.AddRange(listRemoteEndPoint);
            m_currRemoteEndPointIndex = 0;

            ConnectInternal(m_listRemoteEndPoint[m_currRemoteEndPointIndex]);
        }

        public void Close()
        {
            this.Log();
            this.m_actived = 0;

            if (m_socket != null)
            {
                try { m_socket.Shutdown(SocketShutdown.Both); }
                catch (Exception ex) { this.LogWarning(ex.Message); }

                m_socket.Close();
                m_socket = null;
            }


            CleanSend();
            CleanReceive();
        }


        private void TryReconnect()
        {
            this.Log();

            if (!IsActived)
            {
                m_currRemoteEndPointIndex++;
                m_currRemoteEndPointIndex = m_currRemoteEndPointIndex % m_listRemoteEndPoint.Count;
                ConnectInternal(m_listRemoteEndPoint[m_currRemoteEndPointIndex]);
            }
            else
            {
                this.LogWarning("当前连接还在，不能重连！");
            }
        }

        private void ConnectInternal(IPEndPoint remoteEndPoint)
        {
            this.Log();

            var socket = new Socket(remoteEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

            var localEndPoint = new IPEndPoint(IPAddress.Any, m_localPort);
            try { socket.Bind(localEndPoint); }
            catch (Exception exception) { this.LogWarning("指定的Port无法绑定:{0}", localEndPoint); }


            var e = new SocketAsyncEventArgs();
            e.UserToken = socket;
            e.RemoteEndPoint = remoteEndPoint;
            e.Completed += OnConnectCompleted;

            ThreadPool.QueueUserWorkItem(_ => OnConnectCompleted(null, e));
        }

        private void OnConnectCompleted(object sender, SocketAsyncEventArgs e)
        {
            this.LogVerbose(e.SocketError.ToString());

            var socket = e.UserToken as Socket;
            var error = e.SocketError;
            e.UserToken = null;
            e.Completed -= OnConnectCompleted;
            e.Dispose();

            Active(socket, (IPEndPoint)e.RemoteEndPoint);

            LOG_TAG = "UdpConnection[" + Id + "," + LocalEndPoint.Port + "]";
            this.LogVerbose("连接成功！");          
        }


        public void Active(Socket socket, IPEndPoint remoteEndPoint)
        {
            if (Interlocked.CompareExchange(ref this.m_actived, 1, 0) == 0)
            {
                m_socket = socket;
                
                socket.ReceiveBufferSize = NetDefine.SocketBufferSize;
                socket.SendBufferSize = NetDefine.SocketBufferSize;

                RemoteEndPoint = (IPEndPoint)remoteEndPoint;
                LocalEndPoint = (IPEndPoint)socket.LocalEndPoint;

                m_saeSend = m_pool.Acquire();
                m_saeSend.Completed += OnSendCompleted;
                m_saeSend.RemoteEndPoint = remoteEndPoint;

                m_saeReceive = m_pool.Acquire();
                m_saeReceive.Completed += OnReceiveCompleted;
                m_saeReceive.RemoteEndPoint = remoteEndPoint;

                BeginSend();
                BeginReceive();
            }
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
            this.LogVerbose();

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
            this.LogVerbose(e.SocketError.ToString());

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
                onSendError.InvokeSafe(this, (int)NetErrorCode.UnkownError,ex.Message);
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
                this.LogWarning("发送失败！{0}", e.SocketError);
                onSendError.InvokeSafe(this,(int)NetErrorCode.UnkownError, e.SocketError.ToString());

                //继续发下一个Packet
                m_currSendingPacket = null;
                m_sending = 0;
                TrySendNext();

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
                    this.LogWarning("发送数据出错：{0}", ex.Message);
                    onSendError.InvokeSafe(this, (int)NetErrorCode.UnkownError, ex.Message);
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
        protected void BeginReceive()
        {
            ReceiveInternal();
        }

        private void ReceiveInternal()
        {
            this.LogVerbose();

            bool result = false;
            try
            {
                result = m_socket.ReceiveFromAsync(m_saeReceive);
            }
            catch (Exception ex)
            {
                this.LogWarning("接收数据出错：{0}", ex.Message);
                onReceiveError.InvokeSafe(this, (int) NetErrorCode.UnkownError, ex.Message);
            }

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
                this.LogWarning("接收出错！{0}", e.SocketError);
                onReceiveError.InvokeSafe(this, (int)NetErrorCode.SocketError, e.SocketError.ToString());
                ReceiveInternal();
                return;
            }

            if (e.BytesTransferred < 1)
            {
                this.LogWarning("接收出错！{0}", e.SocketError);
                onReceiveError.InvokeSafe(this, (int)NetErrorCode.SocketError, e.SocketError.ToString());
                ReceiveInternal();
                return;
            }


            m_bufferReceive.Attach(e.Buffer, e.BytesTransferred);
            NetMessage msg = new NetMessage();
            if (msg.Deserialize(m_bufferReceive))
            {
                lock (m_queueReceive)
                {
                    m_queueReceive.Enqueue(msg);
                }
            }
            else
            {
                this.LogError("反序列化失败！");
                onReceiveError.InvokeSafe(this, (int)NetErrorCode.DeserializeError, "反序列化失败");
            }
            
            ReceiveInternal();
        }



        //===================================================================
        /// <summary>
        /// 主线程读取数据
        /// </summary>
        /// <returns></returns>
        private NetMessage Receive()
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

        public void Tick()
        {
            var msg = Receive();
            while (msg != null)
            {
                if (msg.head.sid == 0)
                {
                    var errmsg = PBSerializer.NDeserialize<NetErrorMessage>(msg.content);
                    this.LogWarning("服务器返回错误:{0},{1}", errmsg.code, errmsg.info);

                    HandleServerError(errmsg.code);

                    onServerError.InvokeSafe(this, errmsg.code, errmsg.info);
                }
                else
                {
                    //更新SessionId
                    if (Id != msg.head.sid)
                    {
                        Id = msg.head.sid;
                        LOG_TAG = "UdpConnection[" + Id + "," + m_localPort + "]";
                        this.LogWarning("SessionId发生变化：{0}", Id);
                    }

                    onReceive.InvokeSafe(msg);
                }

                msg = Receive();
            }
        }

        private void HandleServerError(int errcode)
        {
            if (errcode == (int) NetErrorCode.SessionExpire)
            {
                Id = 0;
                LOG_TAG = "UdpConnection[" + Id + "," + m_localPort + "]";
                this.LogWarning("Session过期!");
            }

            onServerError.InvokeSafe(this, (int)NetErrorCode.UnkownError, "");
        }
    }
}
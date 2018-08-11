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
using SGF.SEvent;


namespace SGF.Network.General.Client
{
    public class TcpConnection:TcpConnectionBase, IConnection
    {
        //==========================================================
        //事件
        //==========================================================
        public Signal<IConnection> onConnecting { get; private set; }
        public Signal<IConnection> onConnectSuccess { get; private set; }
        public Signal<IConnection, int> onConnectError { get; private set; }
        public Signal<IConnection> onDisconnected { get; private set; }
        public Signal<IConnection, int, string> onSendError { get; private set; }
        public Signal<IConnection> onSendSuccess { get; private set; }
        public Signal<IConnection, int, string> onReceiveError{ get; private set; }
        public Signal<IConnection, int, string> onServerError { get; private set; }
        /// <summary>
        /// 当收到数据时发出信号
        /// 参数：字节数组，长度
        /// </summary>
        public Signal<NetMessage> onReceive { get; private set; }

        
        //==========================================================
        //私有成员变量
        //==========================================================
        private List<IPEndPoint> m_listRemoteEndPoint;
        private int m_currRemoteEndPointIndex = -1;
        private int m_localPort;
        private int m_maxReconnCnt = 0;
        private int m_curReconnIndex = 0;

        //==========================================================
        //构造函数
        //==========================================================
        public TcpConnection(int localPort, SocketAsyncEventArgsPool pool, int maxReconnCnt = 0):base(pool)
        {
            LOG_TAG = "TcpConnection[" + 0 + "," + localPort + "]";
            this.Log("connId:{0}, localPort:{1}, maxReconnCnt", 0, localPort, maxReconnCnt);
            m_maxReconnCnt = maxReconnCnt;
            onReceive = new Signal<NetMessage>();
            onConnectSuccess = new Signal<IConnection>();
            onConnectError = new Signal<IConnection, int>();
            onDisconnected = new Signal<IConnection>();
            onSendSuccess = new Signal<IConnection>();
            onSendError = new Signal<IConnection, int, string>();
            onConnecting = new Signal<IConnection>();
            onReceiveError = new Signal<IConnection, int, string>();
            onServerError = new Signal<IConnection, int, string>();
            m_listRemoteEndPoint = new List<IPEndPoint>();
            m_currRemoteEndPointIndex = -1;

            m_localPort = localPort;
        }


        public override void Clean()
        {
            base.Clean();
            onReceive.RemoveAllListeners();
            onConnectSuccess.RemoveAllListeners();
            onConnectError.RemoveAllListeners();
            onDisconnected.RemoveAllListeners();
            onSendSuccess.RemoveAllListeners();
            onSendError.RemoveAllListeners();
            onConnecting.RemoveAllListeners();
            onReceiveError.RemoveAllListeners();
            onServerError.RemoveAllListeners();
            m_listRemoteEndPoint.Clear();
            GC.SuppressFinalize(this);
            
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



        private void TryReconnect()
        {
            this.Log();

            if (!IsActived)
            {

                if (m_maxReconnCnt == 0 || m_curReconnIndex < m_maxReconnCnt)
                {
                    m_curReconnIndex++;

                    if (m_listRemoteEndPoint.Count > 0)
                    {
                        m_currRemoteEndPointIndex++;
                        m_currRemoteEndPointIndex = m_currRemoteEndPointIndex % m_listRemoteEndPoint.Count;
                        ConnectInternal(m_listRemoteEndPoint[m_currRemoteEndPointIndex]);
                    }
                }
                else
                {
                    onConnectError.Invoke(this, (int)NetErrorCode.ReconnectFailed);
                }
            }
            else
            {
                this.LogWarning("当前连接还在，不能重连！");
            }
        }

        private void ConnectInternal(IPEndPoint remoteEndPoint)
        {
            this.Log();

            onConnecting.Invoke(this);

            var socket = new Socket(remoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            var localEndPoint = new IPEndPoint(IPAddress.Any, m_localPort);
            try { socket.Bind(localEndPoint); }
            catch (Exception exception) { this.LogWarning("指定的Port无法绑定:{0}", localEndPoint); }

            
            var e = new SocketAsyncEventArgs();
            e.UserToken = socket;
            e.RemoteEndPoint = remoteEndPoint;
            e.Completed += OnConnectCompleted;

            bool result = false;
            try
            {
                result = socket.ConnectAsync(e);
            }
            catch (Exception ex)
            {
                this.LogWarning(ex.Message);
            }

            if (!result)
            {
                ThreadPool.QueueUserWorkItem(_ => OnConnectCompleted(null, e));
            }

        }

        private void OnConnectCompleted(object sender, SocketAsyncEventArgs e)
        {
            this.Log(e.SocketError.ToString());

            var socket = e.UserToken as Socket;
            var error = e.SocketError;
            e.UserToken = null;
            e.Completed -= OnConnectCompleted;
            e.Dispose();

            if (error != SocketError.Success)
            {
                socket.Close();
                this.LogWarning("连接失败：{0}", error);

                OnConnectError((int)error);
            }
            else
            {
                base.Active(socket);

                LOG_TAG = "TcpConnection[" + Id + "," + LocalEndPoint.Port + "]";
                this.Log("连接成功！");

                onConnectSuccess.Invoke(this);
            }
        }

        private void OnConnectError(int errcode)
        {
            this.LogWarning(errcode.ToString());
            onConnectError.Invoke(this, errcode);

            ThreadPool.QueueUserWorkItem(_ => TryReconnect());
        }

        protected override void OnDisconnected()
        {
            base.OnDisconnected();
            onDisconnected.Invoke(this);
        }


        //======================================================================
        //发送数据
        //======================================================================
        
        protected override void OnSendError(int errcode, string info)
        {
            this.LogWarning("{0}:{1}", errcode, info);
            onSendError.Invoke(this, errcode,info);
        }

        protected override void OnSendSuccess()
        {
            onSendSuccess.Invoke(this);
        }

        //======================================================================
        //接收数据
        //======================================================================
        protected override void OnReceiveError(int errcode, string info)
        {
            this.LogWarning("{0}:{1}", errcode, info);
            onReceiveError.Invoke(this, errcode,info);
        }


        public void Tick()
        {
            var msg = base.Receive();
            while (msg != null)
            {
                //更新SessionId
                if (Id != msg.head.sid)
                {
                    Id = msg.head.sid;
                    LOG_TAG = "TcpConnection[" + Id + "," + m_localPort + "]";
                    this.LogWarning("SessionId发生变化：{0}", Id);
                }

                onReceive.Invoke(msg);
                msg = base.Receive();
            }
        }
    }
}
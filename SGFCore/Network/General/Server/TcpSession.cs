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
using SGF.Network.Core;
using SGF.SEvent;

namespace SGF.Network.General.Server
{
    public class TcpSession:TcpConnectionBase, ISession
    {
        //==========================================================
        //事件
        //==========================================================

        public Signal<ISession> onDisconnected { get; private set; }
        public Signal<ISession, int> onSendError { get; private set; }
        public Signal<ISession> onSendSuccess { get; private set; }
        public Signal<ISession, int> onReceiveError { get; private set; }

        //==========================================================
        //公共成员变量
        //==========================================================
        public uint AuthToken { get; set; }

        //==========================================================
        //私有成员变量
        //==========================================================
        private ISessionListener m_listener;

        public TcpSession(uint sid, SocketAsyncEventArgsPool pool, ISessionListener listener):base(pool)
        {
            LOG_TAG = "TcpSession[" + sid + "]";

            this.Log();

            onDisconnected = new Signal<ISession>();
            onSendSuccess = new Signal<ISession>();
            onSendError = new Signal<ISession, int>();
            onReceiveError = new Signal<ISession, int>();
            
            this.Id = sid;
            m_listener = listener;
        }

        public override string ToString()
        {
            return "TcpSession[" + Id + "," + RemoteEndPoint.Port + "]";
        }


        //======================================================================
        //断开连接相关
        //======================================================================
        protected override void OnDisconnected()
        {
            base.OnDisconnected();
            m_listener.OnDisconnected(this);
            onDisconnected.Invoke(this);
        }


        //======================================================================
        //发送数据
        //======================================================================
        protected override void OnSendError(int errcode, string info)
        {

            this.LogWarning("{0}:{1}", errcode, info);
            onSendError.Invoke(this, errcode);
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
            onReceiveError.Invoke(this, errcode);
        }




        public void Tick(int currentMS)
        {
            var msg = base.Receive();
            while (msg != null)
            {
                m_listener.OnReceive(this, msg);
                msg = base.Receive();
            }
        }


    }
}
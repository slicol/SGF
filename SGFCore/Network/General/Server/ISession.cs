/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
 * 会话接口定义（客户端）
 * Session interface definition (client)
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


using System.Net;

namespace SGF.Network.General.Server
{

    public interface ISessionListener
    {
        void OnReceive(ISession session, byte[] bytes, int len);
    }

    public static class SessionID
    {
        private static uint ms_lastSid = 0;
        public static uint NewID()
        {
            return ++ms_lastSid;
        }
    }


    public interface ISession
    {
        uint id { get; }
        uint uid { get; }
        ushort ping { get; set; }
        void Active(IPEndPoint remotePoint);
        bool IsActived();
        bool IsAuth();
        void SetAuth(uint userId);
        bool Send(byte[] bytes, int len);
        IPEndPoint remoteEndPoint { get; }

        void Tick(uint currentTimeMS);
        void DoReceiveInGateway(byte[] buffer, int len);
    }
}
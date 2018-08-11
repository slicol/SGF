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
using SGF.Network.General.Proto;


namespace SGF.Network.General.Server
{
    public interface ISessionListener
    {
        void OnReceive(ISession session, NetMessage msg);
        void OnDisconnected(ISession session);
    }

    public interface ISession
    {
        uint AuthToken { get; set; }
        /// <summary>
        /// SessionID
        /// </summary>
        uint Id { get; }
        /// <summary>
        /// 连接是否被激活
        /// </summary>
        bool IsActived { get; }

        /// <summary>
        /// 连接的Ping值
        /// </summary>
        ushort Ping { get; set; }
        /// <summary>
        /// 初始化成功后，可以获取本地EndPoint
        /// </summary>
        IPEndPoint LocalEndPoint { get; }

        /// <summary>
        /// 连接建立后，可以获取远端EndPoint
        /// </summary>
        IPEndPoint RemoteEndPoint { get; }

        /// <summary>
        /// 数据发送
        /// 有可能是同步模式
        /// 也可能是异步模式
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        bool Send(NetMessage msg);

        void Tick(int currentMS);
    }
}
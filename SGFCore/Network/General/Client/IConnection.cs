/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
 * 连接接口定义（客户端）
 * Connection interface definition (client)
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
using SGF.Network.General.Proto;
using SGF.SEvent;

namespace SGF.Network.General.Client
{
    public interface IConnection
    {
        /// <summary>
        /// 当收到数据时发出信号
        /// 参数：字节数组，长度
        /// </summary>
        Signal<NetMessage> onReceive { get; }
        Signal<IConnection, int, string> onServerError{ get; }
        
        /// <summary>
        /// 清理整个连接，即反初始化
        /// 它是统一管理的
        /// </summary>
        void Clean();

        /// <summary>
        /// 主动关闭连接
        /// 对于Client来讲，有可能再次重连
        /// 对于Server来讲，用于强制将 Client断线，然后由Gateway统一Clean
        /// </summary>
        void Close();

        /// <summary>
        /// 连接操作
        /// </summary>
        /// <param name="remoteIP"></param>
        /// <param name="remotePort"></param>
        void Connect(string remoteIP, int remotePort);
        void Connect(IPEndPoint[] listRemoteEndPoints);

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
        /// <param name="msg"></param>
        /// <returns></returns>
        bool Send(NetMessage msg);


        /// <summary>
        /// 无论是用IOCP，还是自己实现的IO模型
        /// 都可能需要在网络线程与主线程之间【统一】同步数据
        /// 当然只是可能，也不是必要的
        /// 但是参照Apollo的设计，还是统一同步数据的
        /// </summary>
        void Tick();
    }
}
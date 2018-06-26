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
using SGF.Event;

namespace SGF.Network.General.Client
{
    public interface IConnection
    {
        /// <summary>
        /// 字节数组，长度
        /// </summary>
        SGFEvent<byte[], int> onReceive { get; }

        void Init(int connId, int bindPort);
        void Clean();

        bool Connected { get; }

        int id { get; }

        int bindPort { get; }



        void Connect(string ip, int port);

        void Close();

        bool Send(byte[] bytes, int len);

        void Tick();
    }
}
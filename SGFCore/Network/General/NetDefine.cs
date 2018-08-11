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


namespace SGF.Network.General
{
    public class NetDefine
    {
        /// <summary>
        /// 收包数据的最小Buffer大小
        /// 实际收包时，如果Buffer里的数据没有人读取，会自动扩容
        /// </summary>
        public static int ReceiveBufferMinSize = 1024;

        /// <summary>
        /// Socket的Buffer大小
        /// </summary>
        public static int SocketBufferSize = 8192;

        /// <summary>
        /// 底层分包的Buffer大小
        /// </summary>
        public static int PacketBufferSize = 8192;
    }

    
}
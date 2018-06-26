/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
 * 帧同步模块
 * Frame synchronization module
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

namespace SGF.Network.FSPLite
{
    public class FSPBasicCmd
    {
        /// <summary>
        /// PVP战斗结束
        /// </summary>
        public const int GAME_BEGIN = 1000;

        /// <summary>
        /// 对局开始
        /// </summary>
        public const int ROUND_BEGIN = 1001;

        /// <summary>
        /// 开始加载
        /// </summary>
        public const int LOAD_START = 1002;
        /// <summary>
        /// 加载进度条
        /// </summary>
        public const int LOAD_PROGRESS = 1003;

        /// <summary>
        /// 可以开始控制...
        /// </summary>
        public const int CONTROL_START = 1004;

        /// <summary>
        /// 发送中途退出
        /// </summary>
        public const int GAME_EXIT = 1005;

        /// <summary>
        /// 对局结束
        /// </summary>
        public const int ROUND_END = 1006;

        /// <summary>
        /// PVP战斗结束
        /// </summary>
        public const int GAME_END = 1007;

        /// <summary>
        /// 鉴权身份字段
        /// </summary>
        public const int AUTH = 1008;

        /// <summary>
        /// PING 响应回包...
        /// </summary>
        public const int PING = 1009;

    }
}
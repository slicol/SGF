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
    public enum FSPGameState
    {
        /// <summary>
        /// 0 初始状态
        /// </summary>
        None = 0,

        /// <summary>
        /// 游戏创建状态
        /// 只有在该状态下，才允许加入玩家
        /// 当所有玩家都发VKey.GameBegin后，进入下一个状态
        /// </summary>
        Create,

        /// <summary>
        /// 游戏开始状态
        /// 在该状态下，等待所有玩家发VKey.RoundBegin，或者 判断玩家是否掉线
        /// 当所有人都发送VKey.RoundBegin，进入下一个状态
        /// 当有玩家掉线，则从FSPGame中删除该玩家：
        ///     判断如果只剩下1个玩家了，则直接进入GameEnd状态，否则不影响游戏状态
        /// </summary>
        GameBegin,

        /// <summary>
        /// 回合开始状态
        /// （这个时候客户端可能在加载资源）
        /// 在该状态下，等待所有玩家发VKey.ControlStart， 或者 判断玩家是否掉线
        /// 当所有人都发送VKey.ControlStart，进入下一个状态
        /// 当有玩家掉线，则从FSPGame中删除该玩家：
        ///     判断如果只剩下1个玩家了，则直接进入GameEnd状态，否则不影响游戏状态
        /// </summary>
        RoundBegin,

        /// <summary>
        /// 可以开始操作状态
        /// （因为每个回合可能都会有加载过程，不同的玩家加载速度可能不同，需要用一个状态统一一下）
        /// 在该状态下，接收玩家的业务VKey， 或者 VKey.RoundEnd，或者VKey.GameExit
        /// 当所有人都发送VKey.RoundEnd，进入下一个状态
        /// 当有玩家掉线，或者发送VKey.GameExit，则从FSPGame中删除该玩家：
        ///     判断如果只剩下1个玩家了，则直接进入GameEnd状态，否则不影响游戏状态
        /// </summary>
        ControlStart,

        /// <summary>
        /// 回合结束状态
        /// （大部分游戏只有1个回合，也有些游戏有多个回合，由客户端逻辑决定）
        /// 在该状态下，等待玩家发送VKey.GameEnd，或者 VKey.RoundBegin（如果游戏不只1个回合的话）
        /// 当所有人都发送VKey.GameEnd，或者 VKey.RoundBegin时，进入下一个状态
        /// 当有玩家掉线，则从FSPGame中删除该玩家：
        ///     判断如果只剩下1个玩家了，则直接进入GameEnd状态，否则不影响游戏状态
        /// </summary>
        RoundEnd,

        /// <summary>
        /// 游戏结束状态
        /// 在该状态下，不再接收任何Vkey，然后给所有玩家发VKey.GameEnd，并且等待FSPServer关闭
        /// </summary>
        GameEnd


    }
}
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

using System;
using System.Collections.Generic;
using System.Text;
using SGF.Extension;

namespace SGF.Network.FSPLite.Server
{
    public class FSPGame
    {
        public static int ActiveTimeout = 10;

        //有一个玩家退出游戏
        public Action<uint> onGameExit;

        //游戏真正结束
        public Action<int> onGameEnd;



        /// <summary>
        /// 最大支持的玩家数：31
        /// 因为用来保存玩家Flag的Int只有31位有效位可用，不过31已经足够了
        /// </summary>
        private const int MaxPlayerNum = 31;

        //基础参数
        private int m_authId;

        private uint m_gameId;
        public uint id { get { return m_gameId; } }

        //游戏状态
        private FSPGameState m_State;
        private int m_StateParam1;
        private int m_StateParam2;
        public FSPGameState State { get { return m_State; } }
        

        //Player的Cmd标识
        private int m_GameBeginFlag = 0;
        private int m_RoundBeginFlag = 0;
        private int m_ControlStartFlag = 0;
        private int m_RoundEndFlag = 0;
        private int m_GameEndFlag = 0;

        //Round标志
        private int m_CurRoundId = 0;
        public int CurrentRoundId { get { return m_CurRoundId; } }


        //服务器的当前帧
        private FSPFrame m_LockedFrame = new FSPFrame();

        private int m_CurFrameId = 0;



        //玩家列表
        private List<FSPPlayer> m_ListPlayer = new List<FSPPlayer>();
        private List<FSPPlayer> m_ListPlayersExitOnNextFrame = new List<FSPPlayer>();



        //---------------------------------------------------------
        public void Create(uint gameId, int authId)
        {
            Debuger.Log();
            m_authId = authId;
            m_gameId = gameId;
            m_CurRoundId = 0;
            SetGameState(FSPGameState.Create);
        }

        public void Release()
        {
            SetGameState(FSPGameState.None);

            for (int i = 0; i < m_ListPlayer.Count; i++)
            {
                FSPPlayer player = m_ListPlayer[i];
                player.Release();
            }
            m_ListPlayer.Clear();
            m_ListPlayersExitOnNextFrame.Clear();

            onGameExit = null;
            onGameEnd = null;

            Debuger.Log();
        }

        public string ToString(string prefix = "")
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("[{0}] AuthId:{1}, State:{2}, CurrentRound:{3}, CurrentFrameId:{4}",m_gameId, m_authId, m_State,
                m_CurRoundId, m_CurFrameId);

            sb.AppendFormat("\n{0}PlayerList:", prefix);
            for (int i = 0; i < m_ListPlayer.Count; i++)
            {
                sb.AppendFormat("\n{0}Player{1}", prefix, m_ListPlayer[i].ToString(prefix + "\t"));
            }

            sb.AppendFormat("\n{0}ListPlayersExitOnNextFrame:", prefix);
            for (int i = 0; i < m_ListPlayersExitOnNextFrame.Count; i++)
            {
                sb.AppendFormat("\n{0}Player{1}", prefix, m_ListPlayersExitOnNextFrame[i].ToString(prefix + "\t"));
            }

            return sb.ToString();
        }
        

        //===================================================================

        public FSPPlayer AddPlayer(uint playerId, FSPSession session)
        {
            Debuger.Log("playerId:{0}", playerId);

            if (m_State != FSPGameState.Create)
            {
                Debuger.LogError("当前状态下无法AddPlayer! State = {0}", m_State);
                return null;
            }

            FSPPlayer player = null;
            for (int i = 0; i < m_ListPlayer.Count; i++)
            {
                player = m_ListPlayer[i];
                if (player.id == playerId)
                {
                    Debuger.LogWarning("PlayerId已经存在！用新的替代旧的! PlayerId = " + playerId);
                    m_ListPlayer.RemoveAt(i);
                    player.Release();
                    break;
                }
            }

            if (m_ListPlayer.Count >= MaxPlayerNum)
            {
                Debuger.LogError("已经达到最大玩家数了! MaxPlayerNum = {0}", MaxPlayerNum);
                return null;
            }


            player = new FSPPlayer();
            player.Create(playerId, m_authId, session, OnRecvFromPlayer);
            m_ListPlayer.Add(player);

            return player;
        }

        private FSPPlayer GetPlayer(uint playerId)
        {
            FSPPlayer player = null;
            for (int i = 0; i < m_ListPlayer.Count; i++)
            {
                player = m_ListPlayer[i];
                if (player.id == playerId)
                {
                    return player;
                }
            }
            return null;
        }


        internal int GetPlayerCount()
        {
            return m_ListPlayer.Count;
        }

        public List<FSPPlayer> GetPlayerList()
        {
            return m_ListPlayer;
        }


        //---------------------------------------------------------
        //收到客户端Player的Cmd
        private void OnRecvFromPlayer(FSPPlayer player, FSPMessage msg)
        {
            HandleClientCmd(player, msg);
        }

        protected virtual void HandleClientCmd(FSPPlayer player, FSPMessage msg)
        {

            uint playerId = player.id;

            //处理鉴权
            if (!player.HasAuthed)
            {
                if (msg.cmd == FSPBasicCmd.AUTH)
                {
                    player.SetAuth(msg.args[0]);
                }
                else
                {
                    Debuger.LogWarning("当前Player未鉴权，无法处理该Cmd：{0}", msg.cmd);
                }
                return;
            }

            switch (msg.cmd)
            {
                case FSPBasicCmd.GAME_BEGIN:
                {
                    Debuger.Log("GAME_BEGIN, playerId = {0}, cmd = {1}", playerId, msg);
                    SetFlag(playerId, ref m_GameBeginFlag, "m_GameBeginFlag");
                    break;
                }
                case FSPBasicCmd.ROUND_BEGIN:
                {
                    Debuger.Log("ROUND_BEGIN, playerId = {0}, cmd = {1}", playerId, msg);
                    SetFlag(playerId, ref m_RoundBeginFlag, "m_RoundBeginFlag");
                    break;
                }
                case FSPBasicCmd.CONTROL_START:
                {
                    Debuger.Log("CONTROL_START, playerId = {0}, cmd = {1}", playerId, msg);
                    SetFlag(playerId, ref m_ControlStartFlag, "m_ControlStartFlag");
                    break;
                }
                case FSPBasicCmd.ROUND_END:
                {
                    Debuger.Log("ROUND_END, playerId = {0}, cmd = {1}", playerId, msg);
                    SetFlag(playerId, ref m_RoundEndFlag, "m_RoundEndFlag");
                    break;
                }
                case FSPBasicCmd.GAME_END:
                {
                    Debuger.Log("GAME_END, playerId = {0}, cmd = {1}", playerId, msg);
                    SetFlag(playerId, ref m_GameEndFlag, "m_GameEndFlag");
                    break;
                }
                case FSPBasicCmd.GAME_EXIT:
                {
                    Debuger.Log("GAME_EXIT, playerId = {0}, cmd = {1}", playerId, msg);
                    HandleGameExit(playerId, msg);
                    break;
                }
                default:
                {
                    Debuger.Log("playerId = {0}, cmd = {1}", playerId, msg);
                    AddCmdToCurrentFrame(playerId, msg);
                    break;
                }
            }


        }


        private void AddCmdToCurrentFrame(uint playerId, FSPMessage msg)
        {
            msg.playerId = playerId;
            m_LockedFrame.msgs.Add(msg);
        }


        private void AddBasicCmdToCurrentFrame(int cmd, int arg = 0)
        { 
            FSPMessage msg = new FSPMessage();
            msg.cmd = cmd;
            msg.args = new[] {arg};
            AddCmdToCurrentFrame(0, msg);
        }



        private void HandleGameExit(uint playerId, FSPMessage msg)
        {
            AddCmdToCurrentFrame(playerId, msg);

            FSPPlayer player = GetPlayer(playerId);

            if (player != null)
            {
                player.WaitForExit = true;

                if (onGameExit != null)
                {
                    onGameExit(player.id);
                }
            }
        }


        //=======================================================================================

        /// <summary>
        /// 驱动游戏状态
        /// </summary>
        public void EnterFrame()
        {
            for (int i = 0; i < m_ListPlayersExitOnNextFrame.Count; i++)
            {
                var player = m_ListPlayersExitOnNextFrame[i];
                player.Release();
            }
            m_ListPlayersExitOnNextFrame.Clear();


            
            //处理游戏状态切换
            HandleGameState();


            //经过上面状态处理之后，有可能状态还会发生变化
            if (m_State == FSPGameState.None)
            {
                return;
            }



            if (m_LockedFrame.frameId != 0 || !m_LockedFrame.IsEmpty())
            {
                //将当前帧扔级Player
                for (int i = 0; i < m_ListPlayer.Count; i++)
                {
                    FSPPlayer player = m_ListPlayer[i];
                    player.SendToClient(m_LockedFrame);

                    if (player.WaitForExit)
                    {
                        m_ListPlayersExitOnNextFrame.Add(player);
                        m_ListPlayer.RemoveAt(i);
                        --i;
                    }

                }
            }


            //0帧每个循环需要额外清除掉再重新统计
            if (m_LockedFrame.frameId == 0)
            {
                m_LockedFrame = new FSPFrame();
            }


            //在这个阶段，帧号才会不停往上加
            if (m_State == FSPGameState.RoundBegin || m_State == FSPGameState.ControlStart)
            {
                m_CurFrameId++;
                m_LockedFrame = new FSPFrame();
                m_LockedFrame.frameId = m_CurFrameId;
            }
            
        }






        //设置状态机
        protected void SetGameState(FSPGameState state, int param1 = 0, int param2 = 0)
        {
            Debuger.Log(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
            Debuger.Log("{0} -> {1}, param1 = {2}, param2 = {3}", m_State, state, param1, param2);
            Debuger.Log("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");

            m_State = state;
            m_StateParam1 = param1;
            m_StateParam2 = param2;
        }

        private void HandleGameState()
        {
            switch (m_State)
            {
                case FSPGameState.None:
                {
                    //进入这个状态的游戏，马上将会被回收
                    //这里是否要考虑session中的所有消息都发完了？
                    break;
                }
                case FSPGameState.Create: //游戏刚创建，未有任何玩家加入, 这个阶段等待玩家加入
                {
                    OnState_Create();
                    break;
                }
                case FSPGameState.GameBegin: //游戏开始，等待RoundBegin
                {
                    OnState_GameBegin();
                    break;
                }
                case FSPGameState.RoundBegin: //回合已经开始，开始加载资源等，等待ControlStart
                {
                    OnState_RoundBegin();
                    break;
                }
                case FSPGameState.ControlStart: //在这个阶段可操作，这时候接受游戏中的各种行为包，并等待RoundEnd
                {
                    OnState_ControlStart();
                    break;
                }
                case FSPGameState.RoundEnd: //回合已经结束，判断是否进行下一轮，即等待RoundBegin，或者GameEnd
                {
                    OnState_RoundEnd();
                    break;
                }
                case FSPGameState.GameEnd://游戏结束
                {
                    OnState_GameEnd();
                    break;
                }
                default:
                    break;
            }
        }


        protected virtual int OnState_Create()
        {
            //如果有任何一方已经鉴权完毕，则游戏进入GameBegin状态准备加载
            if (IsFlagFull(m_GameBeginFlag))
            {
                SetGameState(FSPGameState.GameBegin);
                AddBasicCmdToCurrentFrame(FSPBasicCmd.GAME_BEGIN);
                return 0;
            }
            return 0;
        }


        /// <summary>
        /// 游戏开始状态
        /// 在该状态下，等待所有玩家发VKey.RoundBegin，或者 判断玩家是否掉线
        /// 当所有人都发送VKey.RoundBegin，进入下一个状态
        /// 当有玩家掉线，则从FSPGame中删除该玩家：
        ///     判断如果只剩下1个玩家了，则直接进入GameEnd状态，否则不影响游戏状态
        /// </summary>
        protected virtual int OnState_GameBegin()
        {
            if (CheckGameAbnormalEnd())
            {
                return 0;
            }

            if (IsFlagFull(m_RoundBeginFlag))
            {
                SetGameState(FSPGameState.RoundBegin);
                IncRoundId();
                ClearRound();
                AddBasicCmdToCurrentFrame(FSPBasicCmd.ROUND_BEGIN, m_CurRoundId);

                return 0;
            }

            return 0;
        }


        /// <summary>
        /// 回合开始状态
        /// （这个时候客户端可能在加载资源）
        /// 在该状态下，等待所有玩家发VKey.ControlStart， 或者 判断玩家是否掉线
        /// 当所有人都发送VKey.ControlStart，进入下一个状态
        /// 当有玩家掉线，则从FSPGame中删除该玩家：
        ///     判断如果只剩下1个玩家了，则直接进入GameEnd状态，否则不影响游戏状态
        /// </summary>
        protected virtual int OnState_RoundBegin()
        {
            if (CheckGameAbnormalEnd())
            {
                return 0;
            }

            if (IsFlagFull(m_ControlStartFlag))
            {
                ResetRoundFlag();
                SetGameState(FSPGameState.ControlStart);
                AddBasicCmdToCurrentFrame(FSPBasicCmd.CONTROL_START);
                return 0;
            }

            return 0;
        }


        /// <summary>
        /// 可以开始操作状态
        /// （因为每个回合可能都会有加载过程，不同的玩家加载速度可能不同，需要用一个状态统一一下）
        /// 在该状态下，接收玩家的业务VKey， 或者 VKey.RoundEnd，或者VKey.GameExit
        /// 当所有人都发送VKey.RoundEnd，进入下一个状态
        /// 当有玩家掉线，或者发送VKey.GameExit，则从FSPGame中删除该玩家：
        ///     判断如果只剩下1个玩家了，则直接进入GameEnd状态，否则不影响游戏状态
        /// </summary>
        protected virtual int OnState_ControlStart()
        {
            if (CheckGameAbnormalEnd())
            {
                return 0;
            }

            if (IsFlagFull(m_RoundEndFlag))
            {
                SetGameState(FSPGameState.RoundEnd);
                ClearRound();
                AddBasicCmdToCurrentFrame(FSPBasicCmd.ROUND_END, m_CurRoundId);
                return 0;
            }

            return 0;
        }


        /// <summary>
        /// 回合结束状态
        /// （大部分游戏只有1个回合，也有些游戏有多个回合，由客户端逻辑决定）
        /// 在该状态下，等待玩家发送VKey.GameEnd，或者 VKey.RoundBegin（如果游戏不只1个回合的话）
        /// 当所有人都发送VKey.GameEnd，或者 VKey.RoundBegin时，进入下一个状态
        /// 当有玩家掉线，则从FSPGame中删除该玩家：
        ///     判断如果只剩下1个玩家了，则直接进入GameEnd状态，否则不影响游戏状态
        /// </summary>
        protected virtual int OnState_RoundEnd()
        {
            if (CheckGameAbnormalEnd())
            {
                return 0;
            }


            //这是正常GameEnd
            if (IsFlagFull(m_GameEndFlag))
            {
                SetGameState(FSPGameState.GameEnd, (int)FSPGameEndReason.Normal);
                AddBasicCmdToCurrentFrame(FSPBasicCmd.GAME_END, (int)FSPGameEndReason.Normal);
                return 0;
            }


            if (IsFlagFull(m_RoundBeginFlag))
            {
                SetGameState(FSPGameState.RoundBegin);
                ClearRound();
                IncRoundId();
                AddBasicCmdToCurrentFrame(FSPBasicCmd.ROUND_BEGIN, m_CurRoundId);
                return 0;
            }


            return 0;
        }


        protected virtual int OnState_GameEnd()
        {
            //到这里就等业务层去读取数据了 
            if (onGameEnd != null)
            {
                onGameEnd(m_StateParam1);
                onGameEnd = null;
            }
            return 0;
        }


        public bool IsGameEnd()
        {
            return m_State == FSPGameState.GameEnd;
        }


        //============================================================

        /// <summary>
        /// 检测游戏是否异常结束
        /// </summary>
        private bool CheckGameAbnormalEnd()
        {
            //判断还剩下多少玩家，如果玩家少于2，则表示至少有玩家主动退出
            if (m_ListPlayer.Count < 1)
            {
                //直接进入GameEnd状态
                SetGameState(FSPGameState.GameEnd, (int)FSPGameEndReason.AllOtherExit);
                AddBasicCmdToCurrentFrame(FSPBasicCmd.GAME_END, (int)FSPGameEndReason.AllOtherExit);
                return true;
            }

            // 检测玩家在线状态
            for (int i = 0; i < m_ListPlayer.Count; i++)
            {
                FSPPlayer player = m_ListPlayer[i];
                if (player.IsLose())
                {
                    m_ListPlayer.RemoveAt(i);
                    player.Release();
                    --i;
                }
            }

            //判断还剩下多少玩家，如果玩家少于1，则表示至少有玩家主动退出
            if (m_ListPlayer.Count < 1)
            {
                //直接进入GameEnd状态
                SetGameState(FSPGameState.GameEnd, (int)FSPGameEndReason.AllOtherLost);
                AddBasicCmdToCurrentFrame(FSPBasicCmd.GAME_END, (int)FSPGameEndReason.AllOtherLost);
                return true;
            }


            return false;
        }



        private void IncRoundId()
        {
            ++m_CurRoundId;
        }

        private int ClearRound()
        {
            m_LockedFrame = new FSPFrame();
            m_CurFrameId = 0;

            ResetRoundFlag();

            for (int i = 0; i < m_ListPlayer.Count; i++)
            {
                if (m_ListPlayer[i] != null)
                {
                    m_ListPlayer[i].ClearRound();
                }
            }

            return 0;
        }


        private void ResetRoundFlag()
        {
            m_RoundBeginFlag = 0;
            m_ControlStartFlag = 0;
            m_RoundEndFlag = 0;
            m_GameEndFlag = 0;
        }



        //============================================================

        //--------------------------------------------------------------------
        #region Player 状态标志工具函数

        private void SetFlag(uint playerId, ref int flag, string flagname)
        {
            flag |= (0x01 << ((int)playerId - 1));
            Debuger.Log("player = {0}, flag = {1}", playerId, flagname);
        }

        private void ClsFlag(int playerId, ref int flag, string flagname)
        {
            flag &= (~(0x01 << (playerId - 1)));
        }

        public bool IsAnyFlagSet(int flag)
        {
            return flag != 0;
        }

        public bool IsFlagFull(int flag)
        {
            if (m_ListPlayer.Count > 0)
            {
                for (int i = 0; i < m_ListPlayer.Count; i++)
                {
                    FSPPlayer player = m_ListPlayer[i];
                    int playerId = (int)player.id;
                    if ((flag & (0x01 << (playerId - 1))) == 0)
                    {
                        return false;
                    }
                }
                return true;
            }

            return false;

        }


        #endregion


    }
}
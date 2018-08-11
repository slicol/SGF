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

using SGF.Extension;
using SGF.Utils;

namespace SGF.Network.FSPLite.Client
{
    public class FSPManager:ILogTag
    {

        public string LOG_TAG { get; private set; }

        //来自帧同步服务器的事件
        public event Action<int> onGameBegin;
        public event Action<int> onRoundBegin;
        public event Action<int> onControlStart;
        public event Action<int> onRoundEnd;
        public event Action<int> onGameEnd;
        public event Action<uint> onGameExit;


        //基础参数
        private bool m_IsRunning = false;
        private FSPClient m_Client;
        private FSPParam m_Param;
        private uint m_MinePlayerId;
        private Action<int, FSPFrame> m_FrameListener;
        private int m_CurrentFrameIndex;
        private int m_LockedFrameIndex;

        public uint MainPlayerId { get { return m_MinePlayerId; } }


        //游戏状态
        private FSPGameState m_GameState = FSPGameState.None;
        public FSPGameState GameState { get { return m_GameState; } }


        //帧列表
        private DictionarySafe<int, FSPFrame> m_FrameBuffer;//缓存的帧

        private FSPFrameController m_FrameCtrl;
        public FSPFrameController FrameController { get { return m_FrameCtrl; } }

        //本地模拟
        private FSPFrame m_NextLocalFrame;



        public void Start(FSPParam param, uint playerId)
        {
            m_Param = param;
            m_MinePlayerId = playerId;
            LOG_TAG = "FSPManager[" + playerId + "]";

            Debuger.Log();

            if (m_Param.useLocal)
            {
                m_LockedFrameIndex = param.maxFrameId;
            }
            else
            {
                m_Client = new FSPClient();
                m_Client.Init(m_Param.sid);
                m_Client.SetFSPAuthInfo(param.authId);
                m_Client.SetFSPListener(OnFSPListener);

                m_Client.Connect(param.host, param.port);
                m_Client.VerifyAuth();

                m_LockedFrameIndex = m_Param.clientFrameRateMultiple - 1;
            }

            m_IsRunning = true;
            m_GameState = FSPGameState.Create;

            m_FrameBuffer = new DictionarySafe<int, FSPFrame>();
            m_CurrentFrameIndex = 0;

            m_FrameCtrl = new FSPFrameController();
            m_FrameCtrl.Start(param);
        }


        public void Stop()
        {
            Debuger.Log();

            m_GameState = FSPGameState.None;

            if (m_Client != null)
            {
                m_Client.Clean();
                m_Client = null;
            }

            m_FrameListener = null;
            m_FrameCtrl.Stop();
            m_FrameBuffer.Clear();
            m_IsRunning = false;

            onGameBegin = null;
            onRoundBegin = null;
            onControlStart = null;
            onGameEnd = null;
            onRoundEnd = null;
        }

        

        /// <summary>
        /// 设置帧数据的监听
        /// </summary>
        /// <param name="listener"></param>
        public void SetFrameListener(Action<int, FSPFrame> listener)
        {
            Debuger.Log();
            m_FrameListener = listener;
        }



        /// <summary>
        /// 监听来自FSPClient的帧数据
        /// </summary>
        /// <param name="frame"></param>
        private void OnFSPListener(FSPFrame frame)
        {
            AddServerFrame(frame);
        }

        

        private void AddServerFrame(FSPFrame frame)
        {
            
            if (frame.frameId <= 0)
            {
                ExecuteFrame(frame.frameId, frame);
                return;
            }


            frame.frameId = frame.frameId * m_Param.clientFrameRateMultiple;
            m_LockedFrameIndex = frame.frameId + m_Param.clientFrameRateMultiple - 1;
            
            m_FrameBuffer.Add(frame.frameId, frame);
            m_FrameCtrl.AddFrameId(frame.frameId);
            
        }



        #region 对基础流程Cmd的处理

        public void SendGameBegin()
        {
            Debuger.Log();
            SendFSP(FSPBasicCmd.GAME_BEGIN, 0);
        }

        private void Handle_GameBegin(int arg)
        {
            Debuger.Log(arg);
            m_GameState = FSPGameState.GameBegin;
            if (onGameBegin != null)
            {
                onGameBegin(arg);
            }
        }

        public void SendRoundBegin()
        {
            Debuger.Log();
            SendFSP(FSPBasicCmd.ROUND_BEGIN, 0);
        }

        private void Handle_RoundBegin(int arg)
        {
            Debuger.Log(arg);
            m_GameState = FSPGameState.RoundBegin;
            m_CurrentFrameIndex = 0;

            if (!m_Param.useLocal)
            {
                m_LockedFrameIndex = m_Param.clientFrameRateMultiple - 1;
            }
            else
            {
                m_LockedFrameIndex = m_Param.maxFrameId;
            }

            m_FrameBuffer.Clear();

            if (onRoundBegin != null)
            {
                onRoundBegin(arg);
            }
        }

        public void SendControlStart()
        {
            Debuger.Log();
            SendFSP(FSPBasicCmd.CONTROL_START, 0);
        }
        private void Handle_ControlStart(int arg)
        {
            Debuger.Log(arg);
            m_GameState = FSPGameState.ControlStart;
            if (onControlStart != null)
            {
                onControlStart(arg);
            }
        }

        public void SendRoundEnd()
        {
            Debuger.Log();
            SendFSP(FSPBasicCmd.ROUND_END, 0);
        }
        private void Handle_RoundEnd(int arg)
        {
            Debuger.Log(arg);
            m_GameState = FSPGameState.RoundEnd;
            if (onRoundEnd != null)
            {
                onRoundEnd(arg);
            }
        }

        public void SendGameEnd()
        {
            Debuger.Log();
            SendFSP(FSPBasicCmd.GAME_END, 0);
        }
        private void Handle_GameEnd(int arg)
        {
            Debuger.Log(arg);
            m_GameState = FSPGameState.GameEnd;
            if (onGameEnd != null)
            {
                onGameEnd(arg);
            }
        }


        public void SendGameExit()
        {
            Debuger.Log();
            SendFSP(FSPBasicCmd.GAME_EXIT, 0);
        }

        private void Handle_GameExit(uint playerId)
        {
            Debuger.Log(playerId);
            if (onGameExit != null)
            {
                onGameExit(playerId);
            }
        }


        #endregion




        /// <summary>
        /// 给外界用来发送FSPCmd的
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="arg"></param>

        public void SendFSP(int cmd, params int[] args)
        {
            if (!m_IsRunning)
            {
                return;
            }

            if (m_Param.useLocal)
            {
                SendFSPLocal(cmd, args);
            }
            else
            {
                m_Client.SendFSP(m_CurrentFrameIndex, cmd, args);
            }
        }



        /// <summary>
        /// 用于本地兼容，比如打PVE的时候，也可以用帧同步兼容
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="arg"></param>
        private void SendFSPLocal(int cmd, int[] args)
        {
            Debuger.Log("vkey={0}, arg={1}", cmd, args.ToListString());

            if (m_NextLocalFrame == null || m_NextLocalFrame.frameId != m_CurrentFrameIndex + 1)
            {
                m_NextLocalFrame = new FSPFrame();
                m_NextLocalFrame.frameId = m_CurrentFrameIndex + 1;
                
                m_FrameBuffer.Add(m_NextLocalFrame.frameId, m_NextLocalFrame);
            }

            FSPMessage msg = new FSPMessage();
            msg.cmd = cmd;
            msg.args = args;
            msg.playerId = m_MinePlayerId;
            m_NextLocalFrame.msgs.Add(msg);
            
        }



        /// <summary>
        /// 由外界驱动
        /// </summary>
        public void Tick()
        {
            if (!m_IsRunning)
            {
                return;
            }


            if (m_Param.useLocal)
            {
                if (m_LockedFrameIndex == 0 || m_LockedFrameIndex > m_CurrentFrameIndex)
                {
                    m_CurrentFrameIndex++;
                    var frame = m_FrameBuffer[m_CurrentFrameIndex];
                    ExecuteFrame(m_CurrentFrameIndex, frame);
                }
            }
            else
            {
                m_Client.Tick();

                int speed = m_FrameCtrl.GetFrameSpeed(m_CurrentFrameIndex);
                while (speed > 0)
                {
                    if (m_CurrentFrameIndex < m_LockedFrameIndex)
                    {
                        m_CurrentFrameIndex++;
                        var frame = m_FrameBuffer[m_CurrentFrameIndex];
                        ExecuteFrame(m_CurrentFrameIndex, frame);
                    }
                    speed--;
                }
                
            }
        }


        /// <summary>
        /// 执行每一帧
        /// </summary>
        /// <param name="frameId"></param>
        /// <param name="frame"></param>
        private void ExecuteFrame(int frameId, FSPFrame frame)
        {

            if (frame !=null && !frame.IsEmpty())
            {
                for (int i = 0; i < frame.msgs.Count; i++)
                {
                    var msg = frame.msgs[i];
                    switch (msg.cmd)
                    {
                        case FSPBasicCmd.GAME_BEGIN: Handle_GameBegin(msg.args[0]);break;
                        case FSPBasicCmd.ROUND_BEGIN: Handle_RoundBegin(msg.args[0]); break;
                        case FSPBasicCmd.CONTROL_START: Handle_ControlStart(msg.args[0]); break;
                        case FSPBasicCmd.ROUND_END: Handle_RoundEnd(msg.args[0]); break;
                        case FSPBasicCmd.GAME_END: Handle_GameEnd(msg.args[0]); break;
                        case FSPBasicCmd.GAME_EXIT: Handle_GameExit(msg.playerId); break;
                    }

                }

 
            }


            if (m_FrameListener != null)
            {
                m_FrameListener(frameId, frame);
            }

        }


        //======================================================================

        public string ToDebugString()
        {
            string str = "";
            if (m_FrameCtrl != null)
            {
                str += ("NewestFrameId:" + m_FrameCtrl.NewestFrameId) + "; ";
                str += ("PlayedFrameId:" + m_CurrentFrameIndex) + "; ";
                str += ("IsInBuffing:" + m_FrameCtrl.IsInBuffing) + "; ";
                str += ("IsInSpeedUp:" + m_FrameCtrl.IsInSpeedUp) + "; ";
                str += ("FrameBufferSize:" + m_FrameCtrl.JitterBufferSize) + "; ";
            }

            if (m_Client != null)
            {
                str += m_Client.ToDebugString();
            }

            return str;
        }
    }
}
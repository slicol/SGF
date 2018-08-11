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
using SGF.Utils;

namespace SGF.Network.FSPLite.Server
{
    public class FSPManager
    {

        private long m_lastTicks = 0;

        private bool m_UseCustomEnterFrame;
        //===========================================================
        private FSPParam m_param = new FSPParam();
        private FSPGateway m_gateway;

        private MapList<uint, FSPGame> m_mapGame;
        private uint m_lastClearGameTime = 0;

        public void Init(int port)
        {
            Debuger.Log("port:{0}", port);
            m_gateway = new FSPGateway();
            m_gateway.Init(port);
            m_param.port = m_gateway.Port;
            m_param.host = m_gateway.Host;
            m_mapGame = new MapList<uint, FSPGame>();
        }

        public void Clean()
        {
            Debuger.Log();
            m_mapGame.Clear();
        }


        public void Dump()
        {
            m_gateway.Dump();

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("\nFSPParam:{0}", m_param.ToString("\t"));
            sb.AppendLine("\nGameList:");

            var listGame = m_mapGame.AsDictionary();
            foreach (var game in listGame)
            {
                sb.AppendFormat("\n\tGame {0}", game.Value.ToString("\t\t"));
            }
            
            Debuger.LogWarning(sb.ToString());
        }


        //=======================================================================
        //设置FSP参数
        public void SetFrameInterval(int serverFrameInterval, int clientFrameRateMultiple) //MS
        {
            Debuger.Log("serverFrameInterval:{0}, clientFrameRateMultiple:{1}", serverFrameInterval, clientFrameRateMultiple);
            m_param.serverFrameInterval = serverFrameInterval;
            m_param.clientFrameRateMultiple = clientFrameRateMultiple;

        }


        public void SetServerTimeout(int serverTimeout)
        {
            m_param.serverTimeout = serverTimeout;
        }

        public int GetFrameInterval()
        {
            return m_param.serverFrameInterval;
        }

        public FSPParam GetParam()
        {
            m_param.port = m_gateway.Port;
            m_param.host = m_gateway.Host;
            return m_param.Clone();
        }


        //=======================================================================
        //管理Game单局
        public FSPGame CreateGame(uint gameId, int authId)
        {
            Debuger.Log("gameId:{0}, auth:{1}", gameId, authId);

            FSPGame game = new FSPGame();
            game.Create(gameId, authId);
            
            m_mapGame.Add(gameId, game);
            return game;
        }

        public void ReleaseGame(uint gameId)
        {
            Debuger.Log("gameId:{0}", gameId);

            FSPGame game = m_mapGame[gameId];
            if (game != null)
            {
                game.Release();
                m_mapGame.Remove(gameId);
            }
        }


        /// <summary>
        /// 创建一个玩家
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="playerId"></param>
        /// <returns>SessionId</returns>
        public uint AddPlayer(uint gameId, uint playerId)
        {
            var game = m_mapGame[gameId];
            var session = m_gateway.CreateSession();
            game.AddPlayer(playerId, session);
            return session.id;
        }

        /// <summary>
        /// 创建一组玩家
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="listPlayerId"></param>
        /// <returns>SessionId列表</returns>
        public List<uint> AddPlayer(uint gameId, List<uint> listPlayerId)
        {
            Debuger.Log("gameId:{0}, players:{1}", gameId, listPlayerId.ToListString());

            var game = m_mapGame[gameId];
            List<uint> listSid = new List<uint>();
            for (int i = 0; i < listPlayerId.Count; i++)
            {
                var session = m_gateway.CreateSession();
                game.AddPlayer(listPlayerId[i], session);
                listSid.Add(session.id);
            }

            return listSid;

        }


        //=======================================================================

        //时钟信号
        public void Tick()
        {
            m_gateway.Tick();


            //清除非激活的Game
            uint current = (uint)TimeUtils.GetTotalMillisecondsSince1970();

            if (current - m_lastClearGameTime > FSPGame.ActiveTimeout * 1000 / 2)
            {
                m_lastClearGameTime = current;
                ClearNoActiveGame();
            }

            long nowticks = DateTime.Now.Ticks;
            long interval = nowticks - m_lastTicks;

            //这里做了Fixbug
            long frameIntervalTicks = m_param.serverFrameInterval * 10000;
            if (interval > frameIntervalTicks)
            {
                m_lastTicks = nowticks - (nowticks % (frameIntervalTicks));

                if (!m_UseCustomEnterFrame)
                {
                    EnterFrame();
                }
            }
        }


        public void EnterFrame()
        {
            if (m_gateway.IsRunning)
            {
                var listGame = m_mapGame.AsList();
                for (int i = 0; i < listGame.Count; i++)
                {
                    listGame[i].EnterFrame();
                }
            }
            
        }


        private void ClearNoActiveGame()
        {
            var list = m_mapGame.AsList();
            var dir = m_mapGame.AsDictionary();
            int cnt = list.Count;

            for (int i = cnt - 1; i >= 0; i--)
            {
                var game = list[i];
                if (game.IsGameEnd())
                {
                    list.RemoveAt(i);
                    dir.Remove(game.id);
                }
            }
        }


    }
}
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

namespace SGF.Network.FSPLite.Client
{
    public class FSPFrameController
    {
        //缓冲控制
        private int m_ClientFrameRateMultiple = 2;
        private int m_JitterBuffSize = 0;
        private bool m_IsInBuffing = false;
        private int m_NewestFrameId;


        //加速控制
        private bool m_EnableSpeedUp = true;
        private int m_DefaultSpeed = 1;
        private bool m_IsInSpeedUp = false;

        //自动缓冲
        private bool m_EnableAutoBuff = true;
        private int m_AutoBuffCnt = 0;
        private int m_AutoBuffInterval = 15;


        public bool IsInBuffing { get { return m_IsInBuffing; } }
        public bool IsInSpeedUp { get { return m_IsInSpeedUp; } }
        public int JitterBufferSize { get { return m_JitterBuffSize; } set { m_JitterBuffSize = value; } }
        public int NewestFrameId { get { return m_NewestFrameId; } }


        public void Start(FSPParam param)
        {
            SetParam(param);
        }

        public void Stop()
        {
            
        }

        public void SetParam(FSPParam param)
        {
            m_ClientFrameRateMultiple = param.clientFrameRateMultiple;
            m_JitterBuffSize = param.jitterBufferSize;
            m_EnableSpeedUp = param.enableSpeedUp;
            m_DefaultSpeed = param.defaultSpeed;
            m_EnableAutoBuff = param.enableAutoBuffer;

        }


        public void AddFrameId(int frameId)
        {
            m_NewestFrameId = frameId;
        }


        public int GetFrameSpeed(int curFrameId)
        {
            int speed = 0;
            var newFrameNum = m_NewestFrameId - curFrameId;

            //是否正在缓冲中
            if (!m_IsInBuffing)
            {
                //没有在缓冲中

                if (newFrameNum == 0)
                {
                    //需要缓冲一下
                    m_IsInBuffing = true;
                    m_AutoBuffCnt = m_AutoBuffInterval;
                }
                else
                {
                    //因为即将播去这么多帧
                    newFrameNum -= m_DefaultSpeed;

                    int speedUpFrameNum = newFrameNum - m_JitterBuffSize;
                    if (speedUpFrameNum >= m_ClientFrameRateMultiple)
                    {
                        //可以加速
                        if (m_EnableSpeedUp)
                        {
                            speed = 2;
                            if (speedUpFrameNum > 100)
                            {
                                speed = 8;
                            }
                            else if (speedUpFrameNum > 50)
                            {
                                speed = 4;
                            }
                        }
                        else
                        {
                            speed = m_DefaultSpeed;
                        }
                    }
                    else
                    {
                        //还达不到可加速的帧数
                        speed = m_DefaultSpeed;

                        if (m_EnableAutoBuff)
                        {
                            m_AutoBuffCnt--;
                            if (m_AutoBuffCnt <= 0)
                            {
                                m_AutoBuffCnt = m_AutoBuffInterval;
                                if (speedUpFrameNum < m_ClientFrameRateMultiple - 1)
                                {
                                    //这个时候大概率接下来会缺帧
                                    speed = 0;
                                }
                            }
                        }
                        
                    }

                }
            }
            else
            {
                //正在缓冲中
                int speedUpFrameNum = newFrameNum - m_JitterBuffSize;
                if (speedUpFrameNum > 0)
                {
                    m_IsInBuffing = false;
                }
            }

            m_IsInSpeedUp = speed > m_DefaultSpeed;
            return speed;
        }



    }
}
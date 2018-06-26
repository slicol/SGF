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

namespace SGF.Network.FSPLite.Server
{
    public class FSPPlayer
    {
        private uint m_id;
        public uint id { get { return m_id; } }

        private bool m_hasAuthed = false;
        private FSPSession m_session;
        private Action<FSPPlayer, FSPMessage> m_RecvListener;

        //发送数据
        private Queue<FSPFrame> m_FrameCache = null;

        private int m_LastAddFrameId = 0;

        private int m_authId = 0;

        public bool WaitForExit = false;

        //=================================================================
        public void Create(uint id, int authId, FSPSession session, Action<FSPPlayer, FSPMessage> listener)
        {
            Debuger.Log("id:{0}, authId:{1}, sid:{2}", id, authId, session.id);

            m_id = id;
            m_authId = authId;
            m_RecvListener = listener;
            m_session = session;
            m_session.SetReceiveListener(OnRecvFromSession);

            m_FrameCache = new Queue<FSPFrame>();

        }

        public void Release()
        {
            Debuger.Log();
            if (m_session != null)
            {
                m_session.SetReceiveListener(null);
                m_session.Active(false);
                m_session = null;
            }
        }

        public string ToString(string prefix = "")
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("[{0}] Auth:{1}, IsLose:{2}, EndPoint:{3}", m_id, HasAuthed, IsLose(), m_session.RemoteEndPoint);
            return sb.ToString();
        }


        //====================================================================
        //接收数据逻辑

        private void OnRecvFromSession(FSPDataC2S data)
        {
            if (m_session.IsEndPointChanged)
            {
                m_session.IsEndPointChanged = false;
                m_hasAuthed = false;
            }

            if (m_RecvListener != null)
            {
                for (int i = 0; i < data.msgs.Count; i++)
                {
                    m_RecvListener(this, data.msgs[i]);
                }
            }
        }


        //====================================================================
        //发送相关逻辑
        public void SendToClient(FSPFrame frame)
        {
            if (frame != null)
            {
                if (!m_FrameCache.Contains(frame))
                {
                    m_FrameCache.Enqueue(frame);
                }
            }


            while (m_FrameCache.Count > 0)
            {
                if (SendInternal(m_FrameCache.Peek()))
                {
                    m_FrameCache.Dequeue();
                }
            }
        }

        private bool SendInternal(FSPFrame frame)
        {
            if (frame.frameId != 0 && frame.frameId <= m_LastAddFrameId)
            {
                //已经Add过了
                return true;
            }


            if (m_session != null)
            {
                if (m_session.Send(frame))
                {
                    m_LastAddFrameId = frame.frameId;
                    return true;
                }
            }

            return false;
        }

        //====================================================================
        //鉴权相关逻辑

        public void SetAuth(int authId)
        {
            Debuger.Log(authId);
            //这里暂时不做真正的鉴权，只是让流程完整
            m_hasAuthed = m_authId == authId;

        }

        public bool HasAuthed { get { return m_hasAuthed; } }

        //====================================================================


        public void ClearRound()
        {
            Debuger.Log();
            m_FrameCache.Clear();
            m_LastAddFrameId = 0;
        }

        public bool IsLose()
        {
            return !m_session.IsActived();
        }


    }


}
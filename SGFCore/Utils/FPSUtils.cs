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

using System;
using System.Collections.Generic;
using System.Text;

namespace SGF.Utils
{
    public static class FPSUtils
    {
        public class FPSStatInfo
        {
            public string tag;
            public long beginTime;
            internal uint beginFrame;
            public long timeLength;
            public uint frameCount;
            
        }

        private static uint m_frameIndex = 0;
        private static Stack<FPSStatInfo> m_stack = new Stack<FPSStatInfo>();
        private static List<FPSStatInfo> m_listResult = new List<FPSStatInfo>();

        /// <summary>
        /// 不支持嵌套
        /// </summary>
        /// <param name="tag"></param>
        public static void Begin(string tag)
        {
            FPSStatInfo info = new FPSStatInfo();
            info.tag = tag;
            info.beginFrame = m_frameIndex;
            info.beginTime = DateTime.Now.Ticks;
            m_stack.Push(info);
        }

        public static void End()
        {
            if (m_stack.Count > 0)
            {
                var info = m_stack.Pop();
                info.timeLength = DateTime.Now.Ticks - info.beginTime;
                info.frameCount = m_frameIndex - info.beginFrame;
                m_listResult.Add(info);
            }
            else
            {
                Debuger.LogError("Begin与End没有成对调用！");
            }
        }

        public static void Update()
        {
            ++m_frameIndex;
        }

        public static float GetAverageFPS(string tag)
        {
            long totalTime = 0;
            uint totalFrame = 0;
            for (int i = 0; i < m_listResult.Count; i++)
            {
                var info = m_listResult[i];
                if (info.tag == tag)
                {
                    totalTime += info.timeLength;
                    totalTime += info.frameCount;
                }
            }
            
            return (float)(totalFrame / TimeUtils.GetSecondsFromTicks(totalTime));
        }

        public static FPSStatInfo[] GetFPSStatInfos()
        {
            return m_listResult.ToArray();
        }

        public static string GetDumpString(bool cluster = false)
        {
            StringBuilder sb = new StringBuilder();
            
            if (cluster)
            {
                Dictionary<string, long[]> mapFPS = new Dictionary<string, long[]>();
                for (int i = 0; i < m_listResult.Count; i++)
                {
                    var info = m_listResult[i];
                    long[] tmp = null;
                    if (!mapFPS.TryGetValue(info.tag, out tmp))
                    {
                        tmp = new long[2];
                        mapFPS.Add(info.tag, tmp);
                    }

                    tmp[0] += info.timeLength;
                    tmp[1] += info.frameCount;
                }

                sb.AppendFormat("{0,2}. {1,5} {2}\n", "#", "FPS", "Tag");
                int j = 0;
                foreach (var pair in mapFPS)
                {
                    var tag = pair.Key;
                    float fps = (float)(pair.Value[1] / TimeUtils.GetSecondsFromTicks(pair.Value[0]));
                    sb.AppendFormat("{0,2}. {1,5} {2}\n", j, fps, tag);
                    j++;
                }
                sb.AppendFormat("-----------------------------\n");
            }
            else
            {
                sb.AppendFormat("{0,2}. {1,5} {2}\n", "#", "FPS", "Tag");
                for (int i = 0; i < m_listResult.Count; i++)
                {
                    var info = m_listResult[i];
                    float fps = (float)(info.frameCount / TimeUtils.GetSecondsFromTicks(info.timeLength));
                    sb.AppendFormat("{0,2}. {1,5} {2}\n", i, fps, info.tag);
                }
                sb.AppendFormat("-----------------------------\n");
            }

            return sb.ToString();
        }
    }
}
/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
 * DebugerGUI
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
using System.IO;
using SGF.Network;
using SGF.Network.protocol;
using SGF.Unity.UI.SGUI;
using SGF.Utils;
using UnityEngine;

namespace SGF.Unity.DebugerGUI
{
    public class NetDebugerGUI
    {
        public const string TAG = "NetDebugerGUI";


        private static bool m_UseRealtimeProfile = false;

        private static string m_dbgFileName = "";

        public static void Init()
        {
            NetDebuger.EnableLog = NetDebuger.EnableLog;
            SGFDebugerGUI.AddDbgGUI("NetDebuger", OnDbgGUI);
        }



        private static void OnDbgGUI()
        {
            NetDebuger.EnableLog = GUILayout.Toggle(NetDebuger.EnableLog, "NetDebuger.EnableLog");

            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical();
                {        
                    NetDebuger.EnableProfiler = GUILayout.Toggle(NetDebuger.EnableProfiler, "NetDebuger.EnableProfiler");
                    if (NetDebuger.EnableProfiler)
                    {
                        OnDbgGUI_ProfilerControl();
                    }
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                {
                    NetDebuger.EnableWeakNet = GUILayout.Toggle(NetDebuger.EnableWeakNet, "NetDebuger.EnableWeakNet");
                    if (NetDebuger.EnableWeakNet)
                    {
                        OnDbgGUI_WeakNetControl();
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            GUILayout.Box("", GUILayout.Height(1));

            GUILayout.BeginHorizontal();
            {
                if (m_UseRealtimeProfile)
                {
                    Color old = GUI.color;
                    GUI.color = Color.red;
                    GUILayout.Button("Realtime Profiler");
                    GUI.color = old;
                    if (GUILayout.Button("Offline Profiler"))
                    {
                        m_UseRealtimeProfile = false;
                    }
                }
                else
                {
                    if (GUILayout.Button("Realtime Profiler"))
                    {
                        m_UseRealtimeProfile = true;
                    }
                    Color old = GUI.color;
                    GUI.color = Color.red;
                    GUILayout.Button("Offline Profiler");
                    GUI.color = old;
                }

            }
            GUILayout.EndHorizontal();

            if (m_UseRealtimeProfile)
            {
                OnDbgGUI_RealtimeProfiler();
            }
            else
            {
                OnDbgGUI_OfflineProfiler();    
            }

        }

        private static void OnDbgGUI_ProfilerControl()
        {
            if (GUILayout.Button("Clear Profiler Cache"))
            {
                NetDebuger.ClearSample();
                m_dbgFileName = null;
            }

            if (string.IsNullOrEmpty(m_dbgFileName))
            {
                m_dbgFileName = GetDefaultFileName();
            }

            GUILayout.Label("Input Profiler FileName:");
            m_dbgFileName = GUILayout.TextField(m_dbgFileName);

            if (GUILayout.Button("Save Profiler File"))
            {
                NetDebugFile file = new NetDebugFile();
                file.Content.profiler_samples.AddRange(NetDebuger.SampleList);
                file.Save(NetDebuger.DbgFileDir, m_dbgFileName);

                NetDebuger.ClearSample();
                m_dbgFileName = null;

                CheckFileList();
            }
        }

        private static void OnDbgGUI_WeakNetControl()
        {
            GUILayout.Label("Packet Loss Rate: " + (int)(NetDebuger.PacketLossRate * 100) + "%");
            NetDebuger.PacketLossRate = GUILayout.HorizontalSlider(NetDebuger.PacketLossRate, 0, 1f);

            GUILayout.Label("Jitter Rate: " + (int)(NetDebuger.JitterRate * 100) + "%");
            NetDebuger.JitterRate = GUILayout.HorizontalSlider(NetDebuger.JitterRate, 0, 1f);

            GUILayout.Label(string.Format("Jitter Delay: [{0} ~ {1}] ms", NetDebuger.JitterDelayMin, NetDebuger.JitterDelayMax));

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Min:", GUILayout.Width(50));
                NetDebuger.JitterDelayMin = (int)GUILayout.HorizontalSlider(NetDebuger.JitterDelayMin, 0, 200);
                if (NetDebuger.JitterDelayMin > NetDebuger.JitterDelayMax)
                {
                    NetDebuger.JitterDelayMin = NetDebuger.JitterDelayMax;
                }         
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Max:", GUILayout.Width(50));
                NetDebuger.JitterDelayMax = (int)GUILayout.HorizontalSlider(NetDebuger.JitterDelayMax, 0, 2000);
                if (NetDebuger.JitterDelayMax < NetDebuger.JitterDelayMin)
                {
                    NetDebuger.JitterDelayMax = NetDebuger.JitterDelayMin;
                }
            }

            GUILayout.EndHorizontal();
        }

        //========================================================================
        //Realtime Profiler
        private static DictionarySafe<string, GUICurve> m_mapProfileCurve;
        private static DictionarySafe<string, List<NetSampleItem>> m_mapProfileSampleList;

        private bool m_RealTimeUpdate = true;
        private static int m_NumPerPage = 50;
        private int m_MaxTimePerLog = 500;//MS
        private int m_TotalCnt = 0;
        private static int m_profileNextIndex = 0;

        private static void OnDbgGUI_RealtimeProfiler()
        {
            if (m_mapProfileCurve == null)
            {
                m_mapProfileCurve = new DictionarySafe<string, GUICurve>();
            }

            if (m_mapProfileSampleList == null)
            {
                m_mapProfileSampleList = new DictionarySafe<string, List<NetSampleItem>>();
            }

            try
            {
                UpdateProfileSampleList();
            }
            catch (Exception e)
            {
                Debuger.LogError(TAG, "OnDbgGUI_RealtimeProfiler() UpdateProfileSampleList: " + e.Message);
            }
            


            GUILayout.Label("每屏数据采样个数：" + m_NumPerPage);
            m_NumPerPage = (int)GUILayout.HorizontalSlider(m_NumPerPage, 10, 1800);

            foreach (KeyValuePair<string, List<NetSampleItem>> pair in m_mapProfileSampleList)
            {
                string name = pair.Key;
                GUICurve curve = m_mapProfileCurve[name];
                bool toggle = (bool) curve.state;
                toggle = GUILayout.Toggle(toggle, name);
                curve.state = toggle;
                
                if (toggle)
                {
                    OnGUI_ReportProfiler(curve,pair.Value);
                }
            }
        }



        private static void UpdateProfileSampleList()
        {
            List<NetSampleItem> list = NetDebuger.SampleList;
            int totalCnt = list.Count;

            if (m_profileNextIndex > totalCnt)
            {
                m_mapProfileSampleList = new DictionarySafe<string, List<NetSampleItem>>();
                m_mapProfileCurve = new DictionarySafe<string, GUICurve>();
                m_profileNextIndex = 0;
            }

            
            while (m_profileNextIndex < totalCnt)
            {
                NetSampleItem item = list[m_profileNextIndex];
                List<NetSampleItem> listTemp = m_mapProfileSampleList[item.name];

                if (listTemp == null)
                {
                    listTemp = new List<NetSampleItem>();
                    m_mapProfileSampleList.Add(item.name, listTemp);

                    GUICurve curve = new GUICurve();
                    curve.state = false;
                    m_mapProfileCurve.Add(item.name, curve);
                }
                listTemp.Add(item);

                m_profileNextIndex++;
            }
        }

        private static float m_profilerRangeMinY = 0;
        private static float m_profilerRangeMaxY = 200;
        private static void OnGUI_ReportProfiler(GUICurve curve, List<NetSampleItem> list)
        {
            curve.Clear();

            if (list != null)
            {
                int beginIndex = list.Count - (int)m_NumPerPage;
                if (beginIndex < 0)
                {
                    beginIndex = 0;
                }

                List<Vector2> listPoints = NetDebugerGUI.DoAnalyseBasic(list, beginIndex);

                for (int i = 0; i < listPoints.Count; i++)
                {
                    curve.AddPoint(listPoints[i]);
                }
            }


            float minY = m_profilerRangeMinY;
            float maxY = m_profilerRangeMaxY;
            GUILayout.Label("RangeY:");
            GUILayout.BeginHorizontal();
            minY = GUILayout.HorizontalSlider(minY, 0, maxY);
            maxY = GUILayout.HorizontalSlider(maxY, minY, 200);
            GUILayout.EndHorizontal();
            curve.SetRangeY(minY, maxY);
            m_profilerRangeMinY = minY;
            m_profilerRangeMaxY = maxY;


            curve.DrawLayout(GUILayout.Height(200));
            Rect rect = GUILayoutUtility.GetLastRect();
            GUI.Label(rect, string.Format("X:[ {0} , {1} ]\nY:[ {2} , {3} ]", curve.MinX, curve.MaxX, curve.MinY, curve.MaxY));
        }

        //========================================================================
        //Offline Profiler 
        #region Offline Profiler

        private static List<string> m_listDbgFileName;
        private static DictionarySafe<string, bool> m_mapDbgFileToggle;
        private static DictionarySafe<string, NetDebugFile> m_mapDbgFile;
        private static DictionarySafe<string, GUICurve> m_mapCurve;

        private static void OnDbgGUI_OfflineProfiler()
        {
            if (GUILayout.Button("Refresh Profiler File List"))
            {
                CheckFileList();
            }

            GUILayout.Label("File List:");
            if (m_listDbgFileName != null)
            {
                for (int i = 0; i < m_listDbgFileName.Count; i++)
                {
                    string filename = m_listDbgFileName[i];
                    bool toggle = m_mapDbgFileToggle[filename];

                    toggle = GUILayout.Toggle(toggle, filename);
                    m_mapDbgFileToggle[filename] = toggle;

                    if (toggle)
                    {
                        OnDbgGUI_AnalyseFile(filename);
                    }

                    GUILayout.Box("", GUILayout.Height(2));
                }
            }
        }

        private static void OnDbgGUI_AnalyseFile(string filename)
        {
            NetDebugFile file = m_mapDbgFile[filename];
            if (file == null)
            {
                file = new NetDebugFile();
                file.Open(NetDebuger.DbgFileDir, filename);
                m_mapDbgFile.Add(filename,file);
            }

            for (int i = 0; i < file.ListAllSampleName.Count; i++)
            {
                string name = file.ListAllSampleName[i];
                GUICurve curve = m_mapCurve[filename + ">" + name];

                if (curve == null)
                {
                    curve = new GUICurve();
                    curve.state = false;
                    m_mapCurve[filename + ">" + name] = curve;
                }

                bool toggle = (bool)curve.state;
                toggle = GUILayout.Toggle(toggle, name);
                curve.state = toggle;

                if (toggle)
                {
                    OnDbgGUI_ReportFileCurve(curve, file, name);
                }
            }

        }


        private static void OnDbgGUI_ReportFileCurve(GUICurve curve, NetDebugFile file, string sampleName)
        {
            if (curve.Length == 0)
            {
                List<NetSampleItem> list = file.GetSampleList(sampleName);
                List<Vector2> listPoints = DoAnalyseBasic(list);
                curve.AddRangePoint(listPoints);
            }

            float minY = curve.MinY;
            float maxY = curve.MaxY;
            GUILayout.Label("RangeY:");
            GUILayout.BeginHorizontal();
            minY = GUILayout.HorizontalSlider(minY, 0, maxY);
            maxY = GUILayout.HorizontalSlider(maxY, minY, 200);
            GUILayout.EndHorizontal();
            curve.SetRangeY(minY, maxY);

            curve.DrawLayout(GUILayout.Height(200));
            Rect rect = GUILayoutUtility.GetLastRect();
            GUI.Label(rect, string.Format("X:[ {0} , {1} ]\nY:[ {2} , {3} ]", curve.MinX, curve.MaxX, curve.MinY, curve.MaxY));
        }

        #endregion


        //========================================================================
        //数据处理

        public static List<Vector2> DoAnalyseBasic(List<NetSampleItem> list, int beginIndex = 0, int endIndex = 0)
        {
            if (list == null || list.Count < 2)
            {
                return null;
            }

            if (beginIndex < 0 || beginIndex >= list.Count)
            {
                beginIndex = 0;
            }

            if (endIndex <= 0 || endIndex > list.Count)
            {
                endIndex = list.Count;
            }

            if (endIndex - beginIndex < 2)
            {
                return null;
            }


            List<Vector2> listPoint = new List<Vector2>();

            for (int i = beginIndex; i < endIndex; i++)
            {
                if (list[i] == null)
                {
                    break;
                }

                long dtime = 0;

                if (i > 0)
                {
                    dtime = list[i].time - list[i - 1].time;
                }
                else if (list.Count > 1)
                {
                    dtime = list[i + 1].time - list[i].time;
                }

                Vector2 v = new Vector2();
                v.x = i;
                v.y = dtime / 10000.0f;
                if (v.y > 200)
                {
                    v.y = 200;
                }
                listPoint.Add(v);
            }

            return listPoint;
        }

        



        //=============================================================
        //
        #region 工具函数

        public static string GetDefaultFileName()
        {
            string filename;
            DateTime now = DateTime.Now;
            filename = "FSPDebuger_" + now.GetDateTimeFormats('s')[0].ToString(); //2005-11-05T14:06:25
            filename = filename.Replace("-", "_");
            filename = filename.Replace(":", "_");
            filename = filename.Replace(" ", "");
            filename += ".fsp";
            return filename;
        }



        private static void CheckFileList()
        {
            m_listDbgFileName = new List<string>();
            m_mapDbgFileToggle = new DictionarySafe<string, bool>();
            m_mapDbgFile = new DictionarySafe<string, NetDebugFile>();
            m_mapCurve = new DictionarySafe<string, GUICurve>();

            try
            {
                DirectoryInfo dirinfo = new DirectoryInfo(NetDebuger.DbgFileDir);

                if (dirinfo.Exists == false)
                {
                    return;
                }

                FileInfo[] files = dirinfo.GetFiles("*", SearchOption.TopDirectoryOnly);

                for (int i = 0; i < files.Length; i++)
                {
                    m_listDbgFileName.Add(files[i].Name);
                }

            }
            catch (IOException e)
            {
                Debuger.LogWarning(TAG, "CheckFileList() Error：" + e.Message);
            }
        }

        #endregion

    }
}

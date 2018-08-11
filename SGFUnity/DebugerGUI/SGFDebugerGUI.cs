/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
 * SGFDebugerGUI
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SGF.Unity.Common;
using SGF.Utils;
using UnityEngine;
using UnityEngine.UI;


namespace SGF.Unity.DebugerGUI
{

    public class SGFDebugerGUI : SGFDebugerGUIBase
    {
        public const string BasePath = "Assets/SGF/Debuger/";
        public const string UIBasePath = "Assets/Resources/";
     
        public const string Permission_Log = "Log";

        #region 一个完整的SGFDebugerGUI类
        private static SGFDebugerGUI ms_Instance;
        
        public new static void Show(string permission)
        {

            if (string.IsNullOrEmpty(permission))
            {
                return;
            }

            if (ms_Instance == null)
            {
                GameObject prefab = (GameObject)Resources.Load("SGFDebugerGUI");
                if (prefab != null)
                {
                    GameObject obj = (GameObject) Instantiate(prefab);
                    ms_Instance = obj.GetComponent<SGFDebugerGUI>();
                    DontDestroyOnLoad(obj);
                }
            }

            if(ms_Instance!=null)
            {
                ms_Instance.EnsureLogCallback();
                (ms_Instance as SGFDebugerGUIBase).Show(permission);
            }
        }

        public static void Show(string openId, string permission)
        {
            //采用OpenId作为权限检查对象
            if (string.IsNullOrEmpty(openId))
            {
                return;
            }

            permission = ValidPermissionInWhiteList(permission, openId);
            if (string.IsNullOrEmpty(permission))
            {
                return;
            }

            Show(permission);
        }

        public new static void Hide()
        {
            if (ms_Instance != null)
            {
                (ms_Instance as SGFDebugerGUIBase).Hide();
                //Destroy(ms_Instance.gameObject);
                //ms_Instance = null;
            }
        }


        #endregion

        #region 白名单控制

        private static readonly string[] WhiteList_Admin =
        {
            //Slicol
            "5FEF17CFD26A88F5C9349B71D3AD1DBE",
            "C4FD8FAE90332C1EEF96443415127AC4",
            "2383818E509286DDEF99183DB3E251D7",
            "2A02EC0596D89486937A411680F9071E",
            "ozwwNjxOTUPKIqEZk-hQ9pEkeTsA",
            "ozwwNj3hYrjqN9mx7Yskg3UoGj1U",
        };

        private static readonly string[] WhiteList_Log =
        {

        };


        private static DictionarySafe<string, string[]> MapWhiteList; 

        private static string ValidPermissionInWhiteList(string permission, string openId)
        {
            if (MapWhiteList == null)
            {
                MapWhiteList = new DictionarySafe<string, string[]>();
                MapWhiteList.Add(Permission_Admin, WhiteList_Admin);
                MapWhiteList.Add(Permission_Log, WhiteList_Log);
                
                //TODO 新的白名单在这里增加
            }

            if (string.IsNullOrEmpty(permission))
            {
                bool valided = (Array.IndexOf(WhiteList_Admin, openId) >= 0);
                if (valided)
                {
                    return Permission_Admin;
                }

                permission = "";

                foreach (KeyValuePair<string, string[]> pair in MapWhiteList)
                {
                    valided = (Array.IndexOf(pair.Value, openId) >= 0);
                    if (valided)
                    {
                        if (permission == "")
                        {
                            permission = pair.Key;
                        }
                        else
                        {
                            permission = permission + "|" + pair.Key;
                        }
                    }
                }
            }
            else
            {
                bool valided = false;
                if (MapWhiteList.ContainsKey(permission))
                {
                    valided = (Array.IndexOf(MapWhiteList[permission], openId) >= 0);
                }

                if (!valided)
                {
                    permission = "";
                }
            }

            return permission;
        }


        #endregion

        //=========================================================================================
        private GUIStyle m_lowerLeftFontStyle;
        //=========================================================================================

        public Image UIMask;
        //=========================================================================================
        
        override protected void Awake()
        {
            base.Awake();

            ms_Instance = this;
            
            m_lowerLeftFontStyle = new GUIStyle();
            m_lowerLeftFontStyle.clipping = TextClipping.Clip;
            m_lowerLeftFontStyle.border = new RectOffset(0, 0, 0, 0);
            m_lowerLeftFontStyle.normal.background = null;
            m_lowerLeftFontStyle.fontSize = 12;
            m_lowerLeftFontStyle.normal.textColor = Color.white;
            m_lowerLeftFontStyle.fontStyle = FontStyle.Bold;
            m_lowerLeftFontStyle.alignment = TextAnchor.LowerLeft;

            if (UIMask != null)
            {
                UIMask.gameObject.SetActive(true);
            }
        }

        protected void Start()
        {
            AddDbgGUI("日志", OnGUI_LogToggle, Permission_Log);
        }

        public void EnsureLogCallback()
        {

        }

        override protected void OnTitleGUI(Rect rect)
        {

        }

        protected override void OnCustomGUI(Rect rect)
        {

        }


        protected override void OnMaskGUI(Rect rect)
        {
            if (UIMask != null)
            {
                UIMask.rectTransform.sizeDelta = new Vector2(rect.width, rect.height);
                UIMask.rectTransform.localPosition = new Vector3(rect.x - Screen.width/2 + rect.width/2,
                    Screen.height/2 - rect.y - rect.height/2);
            }
        }


        private Vector2 GUI2ScreenPoint(Vector2 v)
        {
            return new Vector2(v.x, Screen.height - v.y);
        }
        private Vector2 ScreenToGUIPoint(Vector2 v)
        {
            return new Vector2(v.x, Screen.height - v.y);
        }



        //==========================================================================================
        //

        private static string m_LogUploadTips = "";
        private static string m_LogUploadUrl = "http://101.227.153.40:8080/replay";
        private static bool m_IsLogUploading = false;
        private static string m_LogReportText = "";

        private void OnGUI_LogToggle()
        {
            GUILayout.BeginVertical();
            
            Debuger.EnableLog = GUILayout.Toggle(Debuger.EnableLog, "Debuger.EnableLog");
            Debuger.EnableTime = GUILayout.Toggle(Debuger.EnableTime, "Debuger.EnableTime");
            Debuger.EnableStack = GUILayout.Toggle(Debuger.EnableStack, "Debuger.EnableStack");
            Debuger.EnableSave = GUILayout.Toggle(Debuger.EnableSave, "Debuger.EnableSave");

            if (Debuger.EnableSave)
            {
                GUILayout.TextArea(Debuger.LogFileDir);
                GUILayout.TextField(Debuger.LogFileName);

                if (!string.IsNullOrEmpty(m_LogUploadTips))
                {
                    GUILayout.Label("Upload Tips:" + m_LogUploadTips);
                }

                if (GUILayout.Button("上传日志"))
                {
                    UploadLog();
                }
            }

            GUILayout.Space(5);

            if (!string.IsNullOrEmpty(m_LogReportText))
            {
                GUILayout.Label("当前日志");
                GUILayout.Label(m_LogReportText);
            }

            if (GUILayout.Button("显示或者刷新日志"))
            {
                byte[] logBytes = LoadCurrentLogFile();
                if (logBytes != null && logBytes.Length > 0)
                {
                    string logStr = Encoding.UTF8.GetString(logBytes);
                    if (logStr.Length >= 15000)
                    {
                        m_LogReportText = logStr.Substring(logStr.Length - 15000);
                    }
                    else if (m_LogReportText.Length + logStr.Length >= 15000)
                    {
                        m_LogReportText = m_LogReportText.Substring(m_LogReportText.Length - (15000 - logStr.Length));
                        m_LogReportText += logStr;
                    }
                    else
                    {
                        m_LogReportText += logStr;    
                    }
                }
            }



            GUILayout.EndVertical();
        }

        //==========================================================================================
        #region 读取日志文本

        private static byte[] LoadCurrentLogFile()
        {
            string fullpath = Debuger.LogFileDir + Debuger.LogFileName;

            if (!File.Exists(fullpath))
            {
                Debuger.LogError("SGFDebugerGUI", "LoadCurrentLogFile() File Is Not Exist:" + fullpath);
                return null;
            }

            try
            {
                if (Debuger.LogFileWriter != null)
                {
                    Debuger.LogFileWriter.Close();
                    Debuger.LogFileWriter = null;
                    Debuger.LogWarning("SGFDebugerGUI", "LoadCurrentLogFile() 取出当前日志文件内容，之后的日志将存入新的日志文件！");
                }

                byte[] bytes = FileUtils.ReadFile(fullpath);
                return bytes;
            }
            catch (Exception e)
            {
                Debuger.LogError("SGFDebugerGUI", "LoadCurrentLogFile() Failed: " + e.Message + e.StackTrace);
                return null;
            }

        }
        #endregion

        //==========================================================================================
        #region 上传日志
        public static void UploadLog()
        {
            if (m_IsLogUploading)
            {
                m_LogUploadTips = "正在上传中，请稍候再操作..";
                return;
            }

            try
            {
                byte[] bytes = LoadCurrentLogFile();

                if (bytes != null && bytes.Length > 0)
                {
                    WWWForm form = new WWWForm();
                    form.AddField("User", 0);
                    form.AddField("Time", (int)TimeUtils.GetTotalSecondsSince1970());
                    form.AddBinaryData("Content", bytes);

                    WWW www = new WWW(m_LogUploadUrl, form);
                    MonoHelper.StartCoroutine(UploadLogPost(www));

                    m_IsLogUploading = true;
                }
            }
            catch (Exception e)
            {
                Debuger.LogError("SGFDebugerGUI", "Upload() Failed: " + e.Message + e.StackTrace);
                return;
            }
        }

        private static IEnumerator UploadLogPost(WWW www)
        {
            m_LogUploadTips = "开始上传...";

            yield return www;
            m_IsLogUploading = false;

            if (www.error != null)
            {
                m_LogUploadTips = "上传失败!";
                Debuger.LogError("SGFDebugerGUI", "Upload() Failed: " + www.error);
            }
            else
            {
                m_LogUploadTips = "上传完成!";
                Debuger.Log("SGFDebugerGUI", "Upload() Complete!!!");
            }
        }

        #endregion
    }


}


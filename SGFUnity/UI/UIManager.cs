/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
 * UI管理器
 * UI Manager
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
using SGF.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SGF.Unity.UI
{
    public class UIManager : Singleton<UIManager>
    {
        public static string MainScene = "Main";
        public static string MainPage = "UIMainPage";
        public static string SceneLoading = "SceneLoading";

        class UIPageTrack
        {
            public string name;
            public object arg;
            public Type type;
        }


        private Stack<UIPageTrack> m_pageTrackStack;
        private UIPageTrack m_currentPage;

        private List<UIPanel> m_listLoadedPanel;

        private Action<string> onSceneLoadedOnly;


        public UIManager()
        {
            m_pageTrackStack = new Stack<UIPageTrack>();
            m_listLoadedPanel = new List<UIPanel>();
        }


        public void Init(string uiResRoot)
        {
            UIRes.UIResRoot = uiResRoot;

            m_pageTrackStack.Clear();
            m_listLoadedPanel.Clear();

            SceneManager.sceneLoaded += (scene, mode) =>
            {
                if (onSceneLoadedOnly != null) onSceneLoadedOnly(scene.name);
            };
        }

        public void Clean()
        {
            CloseAllLoadedPanel();

            m_pageTrackStack.Clear();
            m_listLoadedPanel.Clear();
        }

        private void CloseAllLoadedPanel()
        {
            for (int i = m_listLoadedPanel.Count - 1; i >= 0; i--)
            {
                var panel = m_listLoadedPanel[i];
                if (panel == null)
                {
                    m_listLoadedPanel.RemoveAt(i);
                }
                else if (panel.IsOpen)
                {
                    panel.Close();
                }

            }
        }


        private string GetShortUIName(string name)
        {
            int i = name.LastIndexOf("/");
            if(i < 0) i = name.LastIndexOf("\\");
            if (i < 0) return name;
            return name.Substring(i+1);
        }

        public T Open<T>(string name, object arg = null, Type implType = null) where T : UIPanel
        {
            T ui = UIRoot.Find<T>(GetShortUIName(name));
            if (ui == null)
            {
                ui = Load<T>(name, implType);
            }
            if (ui != null)
            {
                if (m_listLoadedPanel.IndexOf(ui) < 0)
                {
                    m_listLoadedPanel.Add(ui);
                }

                ui.Open(arg);
                UIRoot.Sort();
            }
            else
            {
                Debuger.LogError("UI不存在：{0}", name);
            }
            return ui;
        }


        private T Load<T>(string name, Type implType) where T : UIPanel
        {
            T ui = default(T);
            GameObject original = UIRes.LoadPrefab(name);
            if (original != null)
            {
                GameObject go = GameObject.Instantiate(original);
                ui = go.GetComponent<T>();
                if (ui == null)
                {
                    try
                    {
                        ui = go.AddComponent(implType) as T;
                    }
                    catch (Exception e)
                    {
                        Debuger.LogError("无法自动添加抽象的UIPanel");
                    }

                }

                if (ui != null)
                {
                    go.name = GetShortUIName(name);
                    UIRoot.AddChild(ui);
                }
                else
                {
                    Debuger.LogError("Prefab没有增加对应组件: " + name);
                }
            }
            else
            {
                Debuger.LogError("Res Not Found: " + name);
            }

            return ui;
        }


        public UIPanel GetUI(string name)
        {
            string shortname = GetShortUIName(name);
            for (int i = 0; i < m_listLoadedPanel.Count; i++)
            {
                if (m_listLoadedPanel[i].name == shortname || m_listLoadedPanel[i].name == name)
                {
                    return m_listLoadedPanel[i];
                }
            }
            return null;
        }


        //==========================================================================================

        public void LoadScene(string scene, Action onLoadComplete)
        {
            onSceneLoadedOnly = (sceneName) =>
            {
                if (sceneName == scene)
                {
                    onSceneLoadedOnly = null;
                    if (onLoadComplete != null) onLoadComplete();
                    CloseLoading(SceneLoading);
                }
            };

            OpenLoading(SceneLoading);
            SceneManager.LoadScene(scene);
        }

        //==========================================================================================
        #region Page管理


        public void OpenPage(string name, object arg = null)
        {
            Debuger.Log("name:{0}, arg:{1} ", name, arg);

            if (m_currentPage != null)
            {
                if (m_currentPage.name != name)
                {
                    m_pageTrackStack.Push(m_currentPage);
                }
                
            }

            OpenPageWorker(name, arg, null);
        }

        public void GoBackPage()
        {
            if (m_pageTrackStack.Count > 0)
            {
                var track = m_pageTrackStack.Pop();
                OpenPageWorker(track.name, track.arg, track.type);
            }
            else
            {
                EnterMainPage();
            }
        }

        private void OpenPageWorker(string page, object arg, Type type)
        {
            m_currentPage = new UIPageTrack();
            m_currentPage.name = page;
            m_currentPage.arg = arg;
            m_currentPage.type = type;

            CloseAllLoadedPanel();

            Open<UIPage>(page, arg, type);
        }


        public void EnterMainPage()
        {
            m_pageTrackStack.Clear();
            OpenPageInScene(MainScene, MainPage, null, null);
        }

        public void OpenPageInScene(string scene, string page)
        {
            OpenPageInScene(scene, page, null, null);
        }

        private void OpenPageInScene(string scene, string page, object arg, Type type)
        {
            Debuger.Log("scene:{0}, page:{1}, arg:{2} ", scene, page, arg);

            string oldScene = SceneManager.GetActiveScene().name;
            if (oldScene == scene)
            {
                OpenPageWorker(page, arg, type);
            }
            else
            {
                LoadScene(scene, () =>
                {
                    OpenPageWorker(page, arg, type);
                });
            }
        }
        #endregion

        //=======================================================================

        #region UIWindow管理

        public UIWindow OpenWindow(string name, object arg = null)
        {
            Debuger.Log(name);
            UIWindow ui = Open<UIWindow>(name, arg);
            return ui;
        }


        public void CloseWindow(string name, object arg = null)
        {
            Debuger.Log(name);
            UIWindow ui = GetUI(name) as UIWindow;
            if (ui != null)
            {
                ui.Close(arg);
            }
        }



        #endregion

        //=======================================================================

        #region UIWidget管理

        public UIWidget OpenWidget(string name, object arg = null)
        {
            Debuger.Log(name);
            UIWidget ui = Open<UIWidget>(name, arg);
            return ui;
        }

        public void CloseWidget(string name, object arg = null)
        {
            Debuger.Log(name);
            UIWidget ui = GetUI(name) as UIWidget;
            if (ui != null)
            {
                ui.Close(arg);
            }
        }

        #endregion


        //==========================================================================================
        #region UILoading管理


        public UILoading OpenLoading(string name, object arg = null)
        {
            Debuger.Log(name);
            UILoading ui = Open<UILoading>(name, arg);
            return ui;
        }

        public void CloseLoading(string name, object arg = null)
        {
            Debuger.Log(name);
            UILoading ui = GetUI(name) as UILoading;
            if (ui != null)
            {
                ui.Close(arg);
            }
        }


        #endregion

        //==========================================================================================



    }
}
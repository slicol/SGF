////////////////////////////////////////////////////////////////////
//                            _ooOoo_                             //
//                           o8888888o                            //
//                           88" . "88                            //
//                           (| ^_^ |)                            //
//                           O\  =  /O                            //
//                        ____/`---'\____                         //
//                      .'  \\|     |//  `.                       //
//                     /  \\|||  :  |||//  \                      //
//                    /  _||||| -:- |||||-  \                     //
//                    |   | \\\  -  /// |   |                     //
//                    | \_|  ''\---/''  |   |                     //
//                    \  .-\__  `-`  ___/-. /                     //
//                  ___`. .'  /--.--\  `. . ___                   //
//                ."" '<  `.___\_<|>_/___.'  >'"".                //
//              | | :  `- \`.;`\ _ /`;.`/ - ` : | |               //
//              \  \ `-.   \_ __\ /__ _/   .-` /  /               //
//        ========`-.____`-.___\_____/___.-`____.-'========       //
//                             `=---='                            //
//        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^      //
//            佛祖保佑       无BUG        不修改                   //
////////////////////////////////////////////////////////////////////
/*
 * 描述：
 * 作者：slicol
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;



namespace SGF.Unity.DebugerGUI
{

    public class SGFDebugerGUIBase : MonoBehaviour
    {
        public const string Permission_Admin = "Admin";
		private const string PrefsKey_SelectedGUI = "SGFDebugerGUI_SelectedGUI";
        class DbgGUIProxy
        {
            public string name;
            public string permission;
            public Action handler;
        }

        public static bool FullScreen = true;
        public static bool UseBigGUI = true;
        public static bool EnableSelectedFirst = true;

        //DbgCmd系统
        #region DbgCmd系统
        private static Dictionary<string, DbgGUIProxy> ms_MapDbgGUI = new Dictionary<string, DbgGUIProxy>();
        private static List<DbgGUIProxy> ms_ListDbgGUI = new List<DbgGUIProxy>();

        private static string ms_SelectedGUI = null;

        /// <summary>
        /// 注册一个DbgGUI
        /// </summary>
        /// <param name="name"></param>
        /// <param name="handler"></param>
        /// <param name="permission">默认是需要管理员权限的</param>
        public static void AddDbgGUI(string name, Action handler, string permission = Permission_Admin)
        {
            if (ms_MapDbgGUI.ContainsKey(name))
            {
                DbgGUIProxy proxy = ms_MapDbgGUI[name];
                proxy.handler = handler;
                proxy.permission = permission;
            }
            else
            {
                DbgGUIProxy proxy = new DbgGUIProxy();
                proxy.name = name;
                proxy.permission = permission;
                proxy.handler = handler;
                ms_MapDbgGUI.Add(name, proxy);

                if (name == ms_SelectedGUI && EnableSelectedFirst)
                {
                    ms_ListDbgGUI.Insert(0,proxy);
                }
                else
                {
                    ms_ListDbgGUI.Add(proxy);
                }

            }


        }

        public static void RemoveDbgGUI(string name)
        {
            if (ms_MapDbgGUI.ContainsKey(name))
            {
                if (ms_SelectedGUI == name)
                {
                    ms_SelectedGUI = null;
                }

                DbgGUIProxy proxy = ms_MapDbgGUI[name];
                ms_ListDbgGUI.Remove(proxy);

                ms_MapDbgGUI.Remove(name);
            }
        }
        #endregion


        //=========================================================================================

        #region SGFDebugerGUI的View实现
        private GUIStyle m_WinStyle;
        
        public int BaseSizeBig = 80;
        public int BaseSizeSmall = 40;
        private int m_BaseSize = 40;
        public int BaseSize { get { return m_BaseSize; } }

        public GUISkin GUISkinBig;
        public GUISkin GUISkinSmall;
        private GUISkin m_GuiSkin;
        
        //=========================================================================================
        //=========================================================================================
        public Rect WinRect;
        public Rect WinRectMin = new Rect(0, 0, 150, 100);
        public Rect WinRectMaxSmall = new Rect(0, 0, 500, 600);
        public Rect WinRectMaxBig = new Rect(0, 0, 1000, 1200);
        
        private bool m_IsExpended = false;
        public bool IsExpended { get { return m_IsExpended; } }

        private bool m_IsVisible = false;
        private string m_Permission = "";
        //=========================================================================================
        public static string StatusBarInfo = "";
        private int m_StatusBarHeight = 40;
        //=========================================================================================
        

        protected virtual  void Awake()
        {
            UpdateGUISkin();

            DontDestroyOnLoad(this.gameObject);
            WinRect = WinRectMin;

            //ms_SelectedGUI = PlayerPrefs.GetString(PrefsKey_SelectedGUI, null);
        }

        private void UpdateGUISkin()
        {
            if (UseBigGUI)
            {
                m_BaseSize = BaseSizeBig;
                m_GuiSkin = GUISkinBig;
            }
            else
            {
                m_BaseSize = BaseSizeSmall;
                m_GuiSkin = GUISkinSmall;
            }

            WinRectMin = new Rect(0, 0, (m_BaseSize + 4) * 2, m_BaseSize+4);
        }

        //=========================================================================================

        internal void Show(string permission)
        {
            if (m_Permission != Permission_Admin)
            {
                string[] tmp = permission.Split('|');
                for (int i = 0; i < tmp.Length; i++)
                {
                    if (m_Permission.IndexOf(tmp[i]) < 0)
                    {
                        m_Permission = m_Permission + "|" + tmp[i];
                    }
                }                
            }

            m_IsVisible = true;

            ms_SelectedGUI = PlayerPrefs.GetString(PrefsKey_SelectedGUI, null);
            
        }

        internal void Hide()
        {
            m_IsVisible = false;
        }

        //=========================================================================================

        private void OnGUI()
        {
            if (!m_IsVisible)
            {
                return;
            }


            if (m_WinStyle == null)
            {
                m_WinStyle = new GUIStyle(GUI.skin.window);
                m_WinStyle.normal.textColor = Color.white;
                m_WinStyle.alignment = TextAnchor.MiddleCenter;
            }

            GUI.color = Color.white;

            WinRect = GUI.Window(0, WinRect, OnWindow, "", m_WinStyle);

            OnMaskGUI(WinRect);
            OnCustomGUI(WinRect);

            OnStatusGUI();
        }


        private void OnWindow(int windowID)
        {
            getDownPos();

            GUI.skin.button.fixedHeight = m_BaseSize;

            if (!m_IsExpended)
            {
                if (GUI.Button(new Rect(2, 2, m_BaseSize, m_BaseSize), "∨"))
                {
                    m_IsExpended = true;
                }

                GUI.DragWindow(new Rect(0, 0, Screen.width, Screen.height));
                return;
            }


            GUI.Box(new Rect(2, 2, WinRect.width - 4, m_BaseSize), "");

            Rect rect = new Rect(2, 2, m_BaseSize, m_BaseSize);
            if (GUI.Button(rect, "∧"))
            {
                WinRect.width = WinRectMin.width;
                WinRect.height = WinRectMin.height;
                m_IsExpended = false;
                return;
            }

            rect.x += m_BaseSize + 2;
            if (GUI.Button(rect, UseBigGUI ? "小" : "大"))
            {
                UseBigGUI = !UseBigGUI;
                UpdateGUISkin();
            }

            rect.x += m_BaseSize + 2;
            rect.width = (int)(m_BaseSize*1.2);
            if (GUI.Button(rect, FullScreen ? "全屏√" : "全屏 "))
            {
                FullScreen = !FullScreen;
            }

            rect.x += m_BaseSize + 2;
            rect.width = WinRect.width - 2 - rect.x;
            OnTitleGUI(rect);

            GUI.skin = m_GuiSkin;
            OnGUI_DbgCmdSystem();

            if (FullScreen)
            {
                WinRect.x = 0;
                WinRect.y = 0;
                WinRect.width = Screen.width;
                WinRect.height = Screen.height - m_StatusBarHeight;      
            }
            else
            {
                if (UseBigGUI)
                {
                    WinRect.width = WinRectMaxBig.width;
                    WinRect.height = System.Math.Min(WinRectMaxBig.height, Screen.height);    
                }
                else
                {
                    WinRect.width = WinRectMaxSmall.width;
                    WinRect.height = System.Math.Min(WinRectMaxSmall.height, Screen.height);        
                }
                

                //Debug.Log(m_winRect.y + "  ... " + downPos.y);
                if ((Screen.height - downPos.y) - WinRect.y < m_BaseSize + 4)
                {
                    GUI.DragWindow(new Rect(0, 0, Screen.width, Screen.height));
                }
            }

        }


        protected virtual void OnCustomGUI(Rect rect)
        {

        }

        protected virtual void OnMaskGUI(Rect rect)
        {

        }

        protected virtual void OnTitleGUI(Rect rect)
        {
            
        }

        private void OnStatusGUI()
        {
            if (!string.IsNullOrEmpty(StatusBarInfo))
            {
                GUI.Label(new Rect(0, Screen.height - m_StatusBarHeight, Screen.width, m_StatusBarHeight), StatusBarInfo);
            }
        }

        //=========================================================================================
        //Win绘制
        private Vector2 m_ScrollPosition_CmdList = new Vector2();
        private Vector2 m_ScrollPosition_CmdGUI = new Vector2();

        private float m_OldDrag_CmdGUI;
        private float m_OldDrag_CmdList;

        private void OnGUI_DbgCmdSystem()
        {
            getDownPos();

            //命令列表
            OnGUI_CmdList();

            //命令内容
            OnGUI_CmdGUI();
        }

        private void OnGUI_CmdList()
        {
            Rect rtCmdList = new Rect();
            rtCmdList.x += 2;
            rtCmdList.y += (m_BaseSize + 4);
            rtCmdList.width = m_BaseSize*3 + 10;
            rtCmdList.height = WinRect.height - (m_BaseSize + 4) - 2;
            GUILayout.BeginArea(rtCmdList);
            {
                Vector2 drag = getDrag();
                if (drag.y != 0 && rtCmdList.Contains(new Vector2(downPos.x - WinRect.x, Screen.height - downPos.y - WinRect.y)))
                {
                    m_ScrollPosition_CmdList.y += (drag.y - m_OldDrag_CmdList);
                }

                m_ScrollPosition_CmdList = GUILayout.BeginScrollView(m_ScrollPosition_CmdList);
                {
                    m_OldDrag_CmdList = drag.y;

                    GUILayout.BeginVertical();
                    {
                        bool isSelectChanged = false;
                        DbgGUIProxy newSelectProxy = null;
                        for (int i = 0; i < ms_ListDbgGUI.Count; i++)
                        {
                            DbgGUIProxy proxy = ms_ListDbgGUI[i];
                            if (m_Permission.IndexOf(Permission_Admin) >= 0 || m_Permission.IndexOf(proxy.permission) >= 0)
                            {
                                if (ms_SelectedGUI != proxy.name)
                                {
                                    if (GUILayout.Button(proxy.name, GUILayout.Width(m_BaseSize * 3 - 5), GUILayout.Height(m_BaseSize)))
                                    {
                                        ms_SelectedGUI = proxy.name;
                                        isSelectChanged = true;
                                        newSelectProxy = proxy;
                                    }
                                }
                                else
                                {
                                    GUI.skin.box.alignment = TextAnchor.MiddleCenter;
                                    Color oldColor = GUI.color;
                                    GUI.color = Color.red;
                                    GUILayout.Box(proxy.name, GUILayout.Width(m_BaseSize * 3 - 5), GUILayout.Height(m_BaseSize));
                                    GUI.color = oldColor;
                                }
                            }

                        }

                        if (isSelectChanged)
                        {
                            PlayerPrefs.SetString(PrefsKey_SelectedGUI, ms_SelectedGUI);
                            if (EnableSelectedFirst)
                            {
                                ms_ListDbgGUI.Remove(newSelectProxy);
                                ms_ListDbgGUI.Insert(0, newSelectProxy);
                            }
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndScrollView();


            }
            GUILayout.EndArea();
        }

        private void OnGUI_CmdGUI()
        {
            Rect rtCmdGUI = new Rect();
            rtCmdGUI.x += m_BaseSize * 3 + 2;
            rtCmdGUI.y += (m_BaseSize + 4);
            rtCmdGUI.width = WinRect.width - (m_BaseSize * 3 + 2) - 2;
            rtCmdGUI.height = WinRect.height - (m_BaseSize + 4) - 2;

            GUILayout.BeginArea(rtCmdGUI);
            {
                Vector2 drag = getDrag();

                if (drag.y != 0 && rtCmdGUI.Contains(new Vector2(downPos.x - WinRect.x, Screen.height - downPos.y - WinRect.y)))
                {
                    m_ScrollPosition_CmdGUI.y += (drag.y - m_OldDrag_CmdGUI);
                }
                m_ScrollPosition_CmdGUI = GUILayout.BeginScrollView(m_ScrollPosition_CmdGUI);
                {
                    m_OldDrag_CmdGUI = drag.y;

                    if (ms_MapDbgGUI != null && ms_SelectedGUI != null)
                    {
                        if (ms_MapDbgGUI.ContainsKey(ms_SelectedGUI))
                        {
                            GUI.skin.button.fixedHeight = m_BaseSize;
                            GUI.skin.toggle.fixedHeight = m_BaseSize/2;

                            DbgGUIProxy proxy = ms_MapDbgGUI[ms_SelectedGUI];
                            Action handler = proxy.handler;
                            handler();
                        }
                    }
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndArea();
        }



        #region Drag相关

        Vector2 mousePosition;
        Vector2 getDrag()
        {

            if (Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.IPhonePlayer)
            {
                if (Input.touches.Length != 1)
                {
                    return Vector2.zero;
                }
                return Input.touches[0].position - downPos;
            }
            else
            {
                if (Input.GetMouseButton(0))
                {
                    mousePosition = Input.mousePosition;
                    return mousePosition - downPos;
                }
                else
                {
                    return Vector2.zero;
                }
            }
        }

        Vector2 startPos;

        Vector2 downPos;
        Vector2 getDownPos()
        {
            if (Application.platform == RuntimePlatform.Android ||
               Application.platform == RuntimePlatform.IPhonePlayer)
            {

                if (Input.touches.Length == 1 && Input.touches[0].phase == TouchPhase.Began)
                {
                    downPos = Input.touches[0].position;
                    return downPos;
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    downPos.x = Input.mousePosition.x;
                    downPos.y = Input.mousePosition.y;
                    return downPos;
                }
            }

            return Vector2.zero;
        }
        #endregion

        //==========================================================================================
        //

        #endregion

    }


}


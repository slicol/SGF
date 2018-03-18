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
using System.Collections;
using UnityEngine;

namespace SGF.Unity.Common
{
    public delegate void MonoUpdaterEvent();

    public class MonoHelper : MonoSingleton<MonoHelper>
    {

        //===========================================================

        private event MonoUpdaterEvent UpdateEvent;
        private event MonoUpdaterEvent FixedUpdateEvent;

        public static void AddUpdateListener(MonoUpdaterEvent listener)
        {
            if (Instance != null)
            {
                Instance.UpdateEvent += listener;
            }
        }

        public static void RemoveUpdateListener(MonoUpdaterEvent listener)
        {
            if (Instance != null)
            {
                Instance.UpdateEvent -= listener;
            }
        }

        public static void AddFixedUpdateListener(MonoUpdaterEvent listener)
        {
            if (Instance != null)
            {
                Instance.FixedUpdateEvent += listener;
            }
        }

        public static void RemoveFixedUpdateListener(MonoUpdaterEvent listener)
        {
            if (Instance != null)
            {
                Instance.FixedUpdateEvent -= listener;
            }
        }

        void Update()
        {
            if (UpdateEvent != null)
            {
                try
                {
                    UpdateEvent();
                }
                catch (Exception e)
                {
                    Debuger.LogError("MonoHelper", "Update() Error:{0}\n{1}", e.Message, e.StackTrace);
                }
            }
        }

        void FixedUpdate()
        {
            if (FixedUpdateEvent != null)
            {
                try
                {
                    FixedUpdateEvent();
                }
                catch (Exception e)
                {
                    Debuger.LogError("MonoHelper", "FixedUpdate() Error:{0}\n{1}", e.Message, e.StackTrace);
                }
            }
        }

        //===========================================================

        public new static void StartCoroutine(IEnumerator routine)
        {
            MonoBehaviour mono = Instance;
            mono.StartCoroutine(routine);
        }
    }
}

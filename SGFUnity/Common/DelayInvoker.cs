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
using System.Linq;
using UnityEngine;
using System.Collections;

namespace SGF.Unity.Common
{

    public delegate void DelayFunction(object[] args);

    public class DelayInvoker : MonoSingleton<DelayInvoker>
    {
        private List<DelayHelper> m_lstHelper;
        private List<DelayHelper> m_lstUnscaledHelper;
        private static WaitForEndOfFrame ms_waitForEndOfFrame = new WaitForEndOfFrame();


        class DelayHelper
        {
            public object group;
            public float delay;
            public DelayFunction func;
            public object[] args;

            public void Invoke()
            {
                if (func != null)
                {
                    try
                    {
                        func(args);
                    }
                    catch(Exception e)
                    {
                        Debuger.LogError("DelayInvoker", "Invoke() Error:{0}\n{1}", e.Message, e.StackTrace);
                    }
                    
                }
            }
        }

        public static void DelayInvoke(object group, float delay, DelayFunction func, params object[] args)
        {
            DelayInvoker.Instance.DelayInvokeWorker(group, delay, func, args);
        }


        public static void DelayInvoke(float delay, DelayFunction func, params object[] args)
        {
            DelayInvoker.Instance.DelayInvokeWorker(null, delay, func, args);
        }

        public static void UnscaledDelayInvoke(float delay, DelayFunction func, params object[] args)
        {
            DelayInvoker.Instance.UnscaledDelayInvokeWorker(null, delay, func, args);
        }

        public static void CancelInvoke(object group)
        {
            DelayInvoker.Instance.CancelInvokeWorker(group);
        }

        //====================================================================

        private void DelayInvokeWorker(object group, float delay, DelayFunction func, params object[] args)
        {
            if (m_lstHelper == null)
            {
                m_lstHelper = new List<DelayHelper>();
            }

            DelayHelper helper = new DelayHelper();
            helper.group = group;
            helper.delay = delay;
            helper.func += func;
            helper.args = args;

            m_lstHelper.Add(helper);
        }

        private void UnscaledDelayInvokeWorker(object group, float delay, DelayFunction func, params object[] args)
        {
            if (m_lstUnscaledHelper == null)
            {
                m_lstUnscaledHelper = new List<DelayHelper>();
            }

            DelayHelper helper = new DelayHelper();
            helper.group = group;
            helper.delay = delay;
            helper.func += func;
            helper.args = args;

            m_lstUnscaledHelper.Add(helper);
        }



        private void CancelInvokeWorker(object group)
        {
            if (null != m_lstHelper)
            {
                if (group == null)
                {

                    for (int i = 0; i < m_lstHelper.Count; i++)
                    {
                        m_lstHelper[i] = null;
                    }
                    m_lstHelper.Clear();

                    return;
                }

                for (int i = 0; i < m_lstHelper.Count(); ++i)
                {

                    DelayHelper helper = m_lstHelper[i];

                    if (helper.group == group)
                    {
                        m_lstHelper.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        //====================================================================

        void Update()
        {
            if (null != m_lstHelper)
            {
                for (int i = 0; i < m_lstHelper.Count(); ++i)
                {
                    DelayHelper helper = m_lstHelper[i];
                    helper.delay -= UnityEngine.Time.deltaTime;
                    if (helper.delay <= 0)
                    {
                        m_lstHelper.RemoveAt(i);
                        i--;

                        helper.Invoke();
                    }
                }
            }

            if (null != m_lstUnscaledHelper)
            {
                for (int i = 0; i < m_lstUnscaledHelper.Count(); ++i)
                {
                    DelayHelper helper = m_lstUnscaledHelper[i];
                    helper.delay -= UnityEngine.Time.unscaledDeltaTime;
                    if (helper.delay <= 0)
                    {
                        m_lstUnscaledHelper.RemoveAt(i);
                        i--;

                        helper.Invoke();
                    }
                }

            }
        }

        void OnDisable()
        {
//            Debug.Log("DelayInvoker Release!!!");
            CancelInvoke(null);
            this.StopAllCoroutines();
        }

        //====================================================================

        public static void DelayInvokerOnEndOfFrame(DelayFunction func, params object[] args)
        {
            Instance.StartCoroutine(DelayInvokerOnEndOfFrameWorker(func, args));
        }

        private static IEnumerator DelayInvokerOnEndOfFrameWorker(DelayFunction func, params object[] args)
        {
            yield return ms_waitForEndOfFrame;

            //Profiler.BeginSample("DelayInvoker_DelayInvokerOnEndOfFrame");

            try
            {
                func(args);
            }
            catch (Exception e)
            {
                Debuger.LogError("DelayInvoker", "DelayInvokerOnEndOfFrame() Error:{0}\n{1}", e.Message, e.StackTrace);
            }

            //Profiler.EndSample();
        }


        public static void FixedTimeInvoke(int hours, int minitue)
        {
            
        }
    }



}
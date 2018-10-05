/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
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
using SGF.SEvent;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SGF.Unity.UI
{
    public abstract class UIPanel:MonoBehaviour, ILogTag
    {
        public virtual UITypeDef UIType { get { return UITypeDef.Unkown; } }

        private int m_layer = UILayerDef.Unkown;
        public int Layer { get { return m_layer; } set { m_layer = value; } }

        public bool AutoBindUIElement = false;

        [SerializeField]
        private AnimationClip m_openAniClip;

        [SerializeField]
        private AnimationClip m_closeAniClip;

        private float m_closeAniClipTime;
        private object m_closeArg;
        public Signal<object> onClose = new Signal<object>();


        /// <summary>
        /// 当前UI是否打开
        /// </summary>
        public bool IsOpen { get { return this.gameObject.activeSelf; } }



        void Awake()
        {
            LOG_TAG = this.GetType().Name;
            if (AutoBindUIElement)
            {
                UIElementBinder.BindAllUIElement(this);
            }

            OnAwake();
        }

        protected virtual void OnAwake()
        {

        }

        protected virtual void OnDestroy()
        {

        }

        protected virtual void OnEnable()
        {

        }

        protected virtual void OnDisable()
        {

        }

        


        private void Update()
        {
            if (m_closeAniClipTime > 0)
            {
                m_closeAniClipTime -= UnityEngine.Time.deltaTime;
                if (m_closeAniClipTime <= 0)
                {
                    CloseWorker();
                }
            }

            OnUpdate();
        }


        public void Open(object arg = null)
        {
            LOG_TAG = this.GetType().Name;

            this.Log("args:{0}", arg);

            if (!this.gameObject.activeSelf)
            {
                this.gameObject.SetActive(true);
            }
            
            OnOpen(arg);

            if (m_openAniClip != null)
            {
                var animation = GetComponent<Animation>();
                if (animation != null)
                {
                    animation.Play(m_openAniClip.name);
                }
                else
                {
                    Debuger.LogError("设置了OpenAniClip，但是未找到 Animation组件！");
                }
            }
        }


        public void Close(object arg = null)
        {
            this.Log("args:{0}", arg);
            m_closeArg = arg;
            m_closeAniClipTime = 0;

            if (m_closeAniClip != null)
            {
                var animation = GetComponent<Animation>();
                if (animation != null)
                {
                    animation.Play(m_closeAniClip.name);
                    m_closeAniClipTime = m_closeAniClip.length;
                }
                else
                {
                    Debuger.LogError("设置了CloseAniClip，但是未找到 Animation组件！");
                    CloseWorker();
                }
            }
            else
            {
                CloseWorker();
            }
            
        }



        private void CloseWorker()
        {

            if (this.gameObject.activeSelf)
            {
                this.gameObject.SetActive(false);
            }

            OnClose(m_closeArg);
            onClose.Invoke(m_closeArg);

            m_closeArg = null;
        }



        protected virtual void OnOpen(object arg = null)
        {
            Layer = UILayerDef.GetDefaultLayer(UIType);
        }

        protected virtual void OnClose(object arg = null)
        {
            
        }

        protected virtual void OnUpdate()
        {
            
        }



        public string LOG_TAG { get; protected set; }



        /// <summary>
        /// 方便寻找Panel上的UI控件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="controlName"></param>
        /// <returns></returns>
        public T Find<T>(string controlName) where T : MonoBehaviour
        {
            Transform target = this.transform.Find(controlName);
            if (target != null)
            {
                return target.GetComponent<T>();
            }
            else
            {
                Debuger.LogError("未找到UI控件：{0}", controlName);
                return default(T);
            }
        }



        #region UI事件监听辅助函数
        /// <summary>
        /// 为UIPanel内的脚本提供便捷的UI事件监听接口
        /// </summary>
        /// <param name="controlName"></param>
        /// <param name="listener"></param>
        public void AddUIClickListener(string controlName, Action<string> listener)
        {
            Transform target = this.transform.Find(controlName);
            if (target != null)
            {
                UIEventTrigger.Get(target).onClickWithName -= listener;
                UIEventTrigger.Get(target).onClickWithName += listener;
            }
            else
            {
                Debuger.LogError("未找到UI控件：{0}", controlName);
            }
        }

        /// <summary>
        /// 为UIPanel内的脚本提供便捷的UI事件监听接口
        /// </summary>
        /// <param name="controlName"></param>
        /// <param name="listener"></param>
        public void AddUIClickListener(string controlName, Action listener)
        {
            Transform target = this.transform.Find(controlName);
            if (target != null)
            {
                UIEventTrigger.Get(target).onClick -= listener;
                UIEventTrigger.Get(target).onClick += listener;
            }
            else
            {
                Debuger.LogError("未找到UI控件：{0}", controlName);
            }
        }



        /// <summary>
        /// 为UIPanel内的脚本提供便捷的UI事件监听接口
        /// </summary>
        /// <param name="target"></param>
        /// <param name="listener"></param>
        public void AddUIClickListener(UIBehaviour target, Action listener)
        {
            if (target != null)
            {
                UIEventTrigger.Get(target).onClick -= listener;
                UIEventTrigger.Get(target).onClick += listener;
            }
        }



        /// <summary>
        /// 移除UI控件的监听器
        /// </summary>
        /// <param name="controlName"></param>
        /// <param name="listener"></param>
        public void RemoveUIClickListener(string controlName, Action<string> listener)
        {
            Transform target = this.transform.Find(controlName);
            if (target != null)
            {
                if (UIEventTrigger.HasExistOn(target))
                {
                    UIEventTrigger.Get(target).onClickWithName -= listener;
                }
            }
            else
            {
                Debuger.LogError("未找到UI控件：{0}", controlName);
            }
        }

        /// <summary>
        /// 移除UI控件的监听器
        /// </summary>
        /// <param name="controlName"></param>
        /// <param name="listener"></param>
        public void RemoveUIClickListener(string controlName, Action listener)
        {
            Transform target = this.transform.Find(controlName);
            if (target != null)
            {
                if (UIEventTrigger.HasExistOn(target))
                {
                    UIEventTrigger.Get(target).onClick -= listener;
                }
            }
            else
            {
                Debuger.LogError("未找到UI控件：{0}", controlName);
            }
        }

        /// <summary>
        /// 移除UI控件的监听器
        /// </summary>
        /// <param name="target"></param>
        /// <param name="listener"></param>
        public void RemoveUIClickListener(UIBehaviour target, Action listener)
        {
            if (target != null)
            {
                if (UIEventTrigger.HasExistOn(target.transform))
                {
                    UIEventTrigger.Get(target).onClick -= listener;
                }
            }
        }


        /// <summary>
        /// 移除UI控件的所有监听器
        /// </summary>
        /// <param name="controlName"></param>
        public void RemoveUIClickListeners(string controlName)
        {
            Transform target = this.transform.Find(controlName);
            if (target != null)
            {
                if (UIEventTrigger.HasExistOn(target))
                {
                    UIEventTrigger.Get(target).onClick = null;
                }
            }
            else
            {
                Debuger.LogError("未找到UI控件：{0}", controlName);
            }
        }

        /// <summary>
        /// 移除UI控件的所有监听器
        /// </summary>
        /// <param name="target"></param>
        public void RemoveUIClickListeners(UIBehaviour target)
        {
            if (target != null)
            {
                if (UIEventTrigger.HasExistOn(target.transform))
                {
                    UIEventTrigger.Get(target).onClick = null;
                }
            }
        }

        #endregion 

    }
}
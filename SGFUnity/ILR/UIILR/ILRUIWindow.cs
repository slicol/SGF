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



using SGF.Unity.UI;

namespace SGF.Unity.ILR.UIILR
{

    public class ILRUIWindow : ILRUIPanel
    {

    }


    public class ILRUIWindowBridge : UIWindow
    {
        public string AssemblyName = "";
        public string Namespace = "";
        public string TypeName = "";


        private ILRUIPanel m_impl;

        protected override void OnAwake()
        {
            base.OnAwake();

            string fullName = Namespace + "." + TypeName;
            m_impl = ILRManager.CreateInstance(fullName, AssemblyName) as ILRUIPanel;
            if (m_impl == null)
            {
                Debuger.LogError("无法在Assembly[{0}]中创建实例:{1}", AssemblyName, fullName);
            }
            else
            {
                m_impl.OnAwakeInternal(this);
            }
        }


        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (m_impl != null)
            {
                m_impl.OnDestroyInternal();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (m_impl != null)
            {
                m_impl.OnEnableInternal();
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (m_impl != null)
            {
                m_impl.OnDisableInternal();
            }
        }


        protected override void OnOpen(object arg = null)
        {
            base.OnOpen(arg);
            if (m_impl != null)
            {
                m_impl.OnOpenInternal(arg);
            }
        }

        protected override void OnClose(object arg = null)
        {
            base.OnClose(arg);
            if (m_impl != null)
            {
                m_impl.OnCloseInternal(arg);
            }
        }

    }


}
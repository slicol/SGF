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
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

namespace SGF.Unity.ILR.UIILR
{
    public class ILRUIPanelAdaptor : CrossBindingAdaptor
    {
        public override Type BaseCLRType
        {
            get { return typeof(ILRUIPanel); }
        }

        public override Type AdaptorType
        {
            get { return typeof(Adaptor); }
        }

        public override object CreateCLRInstance(AppDomain appdomain, ILTypeInstance instance)
        {
            return new Adaptor(appdomain, instance); //创建一个新的实例
        }



        internal class Adaptor : ILRUIPage, CrossBindingAdaptorType
        {
            ILTypeInstance m_instance;
            AppDomain m_appdomain;

            public ILTypeInstance ILInstance
            {
                get { return m_instance; }
            }

            //缓存这个数组来避免调用时的GC Alloc
            private object[] m_args = new object[1];

            public Adaptor(AppDomain appdomain, ILTypeInstance instance)
            {
                this.m_appdomain = appdomain;
                this.m_instance = instance;
            }




            //==============================================================================
            private IMethod mToString;

            private bool mToStringInvokeing;

            public override string ToString()
            {
                if (mToString == null)
                {
                    mToString = m_instance.Type.GetMethod("ToString", 0);
                }

                if (mToString != null && !mToStringInvokeing)
                {
                    mToStringInvokeing = true;
                    var result = m_appdomain.Invoke(mToString, m_instance) as string;
                    mToStringInvokeing = false;
                    return result;
                }
                else
                {
                    return m_instance.Type.FullName;
                }

            }


            //==============================================================================

            private IMethod m_miOnAwake;

            private bool m_isOnAwakeInvoking;
            protected override void OnAwake()
            {
                if (m_miOnAwake == null)
                {
                    m_miOnAwake = m_instance.Type.GetMethod("OnAwake", 0);
                }
                if (m_miOnAwake != null && !m_isOnOpenInvoking)
                {
                    m_isOnAwakeInvoking = true;
                    m_appdomain.Invoke(m_miOnAwake, m_instance);
                    m_isOnAwakeInvoking = false;
                }
                else
                {
                    base.OnAwake();
                }
            }
            //==============================================================================

            private IMethod m_miOnOpen;

            private bool m_isOnOpenInvoking;
            protected override void OnOpen(object args = null)
            {
                Debuger.Log(args);

                if (m_miOnOpen == null)
                {
                    m_miOnOpen = m_instance.Type.GetMethod("OnOpen", 1);
                }
                if (m_miOnOpen != null && !m_isOnOpenInvoking)
                {
                    m_isOnOpenInvoking = true;
                    m_args[0] = args;
                    m_appdomain.Invoke(m_miOnOpen, m_instance, m_args);
                    m_isOnOpenInvoking = false;
                }
                else
                {
                    base.OnOpen(args);
                }
            }

            //==============================================================================

            private IMethod m_miOnClose;

            private bool m_isOnCloseInvoking;
            protected override void OnClose(object args = null)
            {
                Debuger.Log(args);

                if (m_miOnClose == null)
                {
                    m_miOnClose = m_instance.Type.GetMethod("OnClose", 1);
                }
                if (m_miOnClose != null && !m_isOnCloseInvoking)
                {
                    m_isOnCloseInvoking = true;
                    m_args[0] = args;
                    m_appdomain.Invoke(m_miOnClose, m_instance, m_args);
                    m_isOnCloseInvoking = false;
                }
                else
                {
                    base.OnClose(args);
                }
            }


            //==============================================================================



        }
    }
}
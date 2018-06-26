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
using SGF.Module;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

namespace SGF.Unity.ILR.ModuleILR
{
    public class GeneralModuleAdaptor:CrossBindingAdaptor
    {
        public override object CreateCLRInstance(AppDomain appdomain, ILTypeInstance instance)
        {
            return new Adaptor(appdomain, instance);
        }

        public override Type BaseCLRType { get { return typeof(GeneralModule); } }
        public override Type AdaptorType { get { return typeof(Adaptor); } }
    }



    class  Adaptor:GeneralModule, CrossBindingAdaptorType
    {
        //缓存这个数组来避免调用时的GC Alloc
        private object[] m_args = new object[1];

        ILTypeInstance m_instance;
        AppDomain m_appdomain;
        
        public ILTypeInstance ILInstance { get { return m_instance; } }

        public Adaptor(AppDomain appdomain, ILTypeInstance instance)
        {
            this.m_appdomain = appdomain;
            this.m_instance = instance;
            LOG_TAG = "Apdator[" + m_instance.Type.Name + "]";
        }


        //==============================================================================
        private IMethod mToString;

        private bool mToStringInvokeing;
        public override string ToString()
        {
            if (mToString == null)
            {
                mToString = m_instance.Type.GetMethod("ToString",0);
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


        private IMethod m_miCreate;

        private bool m_isCreateInvoking;
        public override void Create(object args = null)
        {
            //Debuger.Log(args);

            if (m_miCreate == null)
            {
                m_miCreate = m_instance.Type.GetMethod("Create", 1);
            }
            if (m_miCreate != null && !m_isCreateInvoking)
            {
                m_isCreateInvoking = true;
                m_args[0] = args;
                m_appdomain.Invoke(m_miCreate, m_instance, m_args);
                m_isCreateInvoking = false;
            }
            else
            {
                base.Create(args);
            }
        }


        private IMethod m_miRelease;

        private bool m_isReleaseInvoking;
        public override void Release()
        {
            //Debuger.Log();

            if (m_miRelease == null)
            {
                m_miRelease = m_instance.Type.GetMethod("Release", 0);
            }
            if (m_miRelease != null && !m_isReleaseInvoking)
            {
                m_isReleaseInvoking = true;
                m_appdomain.Invoke(m_miRelease, m_instance);
                m_isReleaseInvoking = false;
            }
            else
            {
                base.Release();
            }
        }



        private IMethod m_miOnHandleMessage;

        private bool m_isOnHandleMessageInvoking;
        protected override void OnModuleMessage(string msg, object[] args)
        {
            //Debuger.Log("msg:{0}, args:{1}", msg, args.ToListString());

            IMethod mi = m_instance.Type.GetMethod(msg, args.Length);
            if (mi != null)
            {
                m_appdomain.Invoke(mi, m_instance, args);
            }
            else
            {
                if (m_miOnHandleMessage == null)
                {
                    m_miOnHandleMessage = m_instance.Type.GetMethod("OnModuleMessage", 1);
                }
                if (m_miOnHandleMessage != null && !m_isOnHandleMessageInvoking)
                {
                    m_isOnHandleMessageInvoking = true;
                    m_appdomain.Invoke(m_miOnHandleMessage, m_instance, args);
                    m_isOnHandleMessageInvoking = false;
                }
                else
                {
                    base.OnModuleMessage(msg, args);
                }
            }
        }


        private IMethod m_miShow;

        private bool m_isShowInvoking;
        protected override void Show(object arg)
        {
            //Debuger.Log(arg);

            if (m_miShow == null)
            {
                m_miShow = m_instance.Type.GetMethod("Show", 1);
            }
            if (m_miShow != null && !m_isShowInvoking)
            {
                m_isShowInvoking = true;
                m_args[0] = arg;
                m_appdomain.Invoke(m_miShow, m_instance, m_args);
                m_isShowInvoking = false;
            }
            else
            {
                base.Show(arg);
            }
        }


    }
}
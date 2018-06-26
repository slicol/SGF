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
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

namespace SGF.Unity.ILR.UIILR
{

    public class ILRUIWindowAdaptor : CrossBindingAdaptor
    {
        public override Type BaseCLRType
        {
            get { return typeof(ILRUIWindow); }
        }

        public override Type AdaptorType
        {
            get { return typeof(Adaptor); }
        }

        public override object CreateCLRInstance(AppDomain appdomain, ILTypeInstance instance)
        {
            return new Adaptor(appdomain, instance); //创建一个新的实例
        }



        class Adaptor : ILRUIPanelAdaptor.Adaptor
        {
            private ILTypeInstance m_instance;
            private AppDomain m_appdomain;
            private object[] m_args = new object[1];

            public Adaptor(AppDomain appdomain, ILTypeInstance instance)
                : base(appdomain, instance)
            {
                this.m_appdomain = appdomain;
                this.m_instance = instance;
            }

            //==============================================================================
        }
    }
}


using System;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

namespace SGF.Unity.ILR.UIILR
{
    public class ILRUILoadingAdaptor : CrossBindingAdaptor
    {
        public override Type BaseCLRType
        {
            get { return typeof(ILRUILoading); }
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
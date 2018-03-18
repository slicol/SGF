using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

namespace SGF.Unity.ILR.DebugerILR
{
    public class ILogTagAdaptor:CrossBindingAdaptor
    {
        public override object CreateCLRInstance(AppDomain appdomain, ILTypeInstance instance)
        {
            return new Adaptor(appdomain, instance);
        }

        public override Type BaseCLRType { get { return typeof(ILogTag); } }
        public override Type AdaptorType { get { return typeof(Adaptor); } }



        class Adaptor : ILogTag, CrossBindingAdaptorType
        {
            private AppDomain m_appdomain;

            public ILTypeInstance ILInstance { get; private set; }


            public Adaptor(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
            {
                m_appdomain = appdomain;
                ILInstance = instance;
            }


            private IMethod mGetLogTag;
            private bool isGetLogTagInvoking = false;
            public string LOG_TAG
            {
                get
                {
                    if (mGetLogTag == null)
                    {
                        mGetLogTag = ILInstance.Type.GetMethod("get_LOG_TAG", 0);
                    }

                    if (mGetLogTag != null && !isGetLogTagInvoking)
                    {
                        isGetLogTagInvoking = true;
                        var result = m_appdomain.Invoke(mGetLogTag, ILInstance) as string;
                        isGetLogTagInvoking = true;
                        return result;
                    }

                    return "";

                }
            }


        }
    }
}
using System;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

namespace SGF.Unity.ILR
{
    public class DelegateConvertor
    {
        public static void Init(AppDomain appdomain)
        {
            appdomain.DelegateManager.RegisterMethodDelegate<int>();
            appdomain.DelegateManager.RegisterMethodDelegate<float>();
            appdomain.DelegateManager.RegisterMethodDelegate<object>();
            appdomain.DelegateManager.RegisterMethodDelegate<System.Object>();



        }
    }
}
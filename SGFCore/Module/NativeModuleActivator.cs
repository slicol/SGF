using System;
using System.Reflection;

namespace SGF.Module
{
    public class NativeModuleActivator:IModuleActivator
    {
        private string m_namespace = "";
        private string m_assemblyName = "";


        public NativeModuleActivator(string _namespace, string assemblyName)
        {
            m_namespace = _namespace;
            m_assemblyName = assemblyName;
        }


        public GeneralModule CreateInstance(string moduleName)
        {
            var fullname = m_namespace + "." + moduleName;

            var type = Type.GetType(fullname + "," + m_assemblyName);
            if (type != null)
            {
                return Activator.CreateInstance(type) as GeneralModule;
            }

            return null;
        }
    }
}
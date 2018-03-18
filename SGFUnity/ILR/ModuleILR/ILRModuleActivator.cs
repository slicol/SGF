using System;
using SGF.Module;

namespace SGF.Unity.ILR.ModuleILR
{
    public class ILRModuleActivator:IModuleActivator
    {
        private string m_namespace;
        private string m_assemblyName;


        public ILRModuleActivator(string _namespace, string assemblyName)
        {
            m_namespace = _namespace;
            m_assemblyName = assemblyName;
        }



        public GeneralModule CreateInstance(string moduleName)
        {
            Type type = null;
            string fullname = m_namespace + "." + moduleName;

            return ILRManager.CreateInstance(fullname, m_assemblyName) as GeneralModule;
        }
    }
}
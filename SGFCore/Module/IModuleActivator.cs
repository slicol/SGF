using System;

namespace SGF.Module
{
    public interface IModuleActivator
    {
        GeneralModule CreateInstance(string moduleName);

    }
}

using System;
using SGF;
using SGF.Module;


namespace SGFAppDemo
{
    public class ScriptMain
    {

        public static bool Init()
        {
            Debuger.Log();
            ModuleManager.Instance.CreateModule(ModuleDef.LoginModule);
            ModuleManager.Instance.CreateModule(ModuleDef.RoomModule);

            ModuleManager.Instance.ShowModule(ModuleDef.LoginModule);
            return true;
        }
    }
}

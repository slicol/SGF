

using System;
using SGF;
using SGF.Module;
using SGF.Unity.ILR;
using SGF.Unity.ILR.ModuleILR;
using SGFAppDemo.Framework;
using SGFAppDemo.Services;

namespace SGFAppDemo
{
    public class AppMain:SGFGameObject
    {
        public override void Active()
        {
            InitServices();
            UpdateVersion();
        }



        private void InitServices()
        {
            Debuger.Log();

            ILRManager.Instance.Init(RunMode.Native, false);
            ILRManager.Instance.AddSearchDirectory(AppDomain.CurrentDomain.BaseDirectory);
            ILRManager.Instance.LoadAssembly(ModuleDef.ScriptAssemblyName);

            ModuleManager.Instance.Init();
            ModuleManager.Instance.RegisterModuleActivator(new NativeModuleActivator(ModuleDef.Namespace, ModuleDef.NativeAssemblyName));
            ModuleManager.Instance.RegisterModuleActivator(new ILRModuleActivator(ModuleDef.Namespace, ModuleDef.ScriptAssemblyName));

            OnlineManager.Instance.Init();
        }

        private void UpdateVersion()
        {
            ModuleManager.Instance.CreateModule(ModuleDef.VersionModule);
            ModuleManager.Instance.ShowModule(ModuleDef.VersionModule);
            GlobalEvent.onVersionUpdateComplete.AddListener(OnVersionUpdateComplete);
        }

        private void OnVersionUpdateComplete()
        {
            GlobalEvent.onVersionUpdateComplete.RemoveAllListeners();

            //由脚本来初始业务逻辑，便于热更
            bool ret = (bool)ILRManager.Instance.Invoke("SGFAppDemo.ScriptMain", "Init");
            if (!ret)
            {
                Debuger.LogError("初始化业务逻辑失败！");
                Console.WriteLine("初始化业务逻辑失败！");
            }
        }
        
    }
}
/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
 * ILR管理器
 * 用来实现原生模块与ILR模块的无缝衔接
 * 你在用模块管理器管理模块时，不需要关心该模块是ILR的，还是原生的
 * ILR Manager
 * It is used to realize the seamless connection between the native module and the ILR module
 * When you manage the module with the module manager, you don't need to care if the module is ILR or native
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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using Mono.Cecil.Pdb;
using SGF.Utils;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;
using SGF.Extension;
using SGF.Unity.ILR.DebugerILR;
using SGF.Unity.ILR.ModuleILR;
using SGF.Unity.ILR.UIILR;

namespace SGF.Unity.ILR
{
    public interface ICLRMethodRedirector
    {
        void Init(AppDomain app);
    }

    public enum RunMode
    {
        Script = 0,
        Native = 1
    }


    public class ILRManager:Singleton<ILRManager>
    {
        public RunMode RunMode { get; private set; }
        public bool UsePdb { get; private set; }

        private List<string> m_listSearchDir = new List<string>();
        private AppDomain m_appdomain;
        private List<Assembly> m_listNativeAssembly = new List<Assembly>();


        public static object CreateInstance(string typeName, string assemblyName = "", params object[] args)
        {
            return Instance.CreateInstanceInternal(typeName, assemblyName, args);
        }

        public static object CreateInstance(string typeName, params object[] args)
        {
            return Instance.CreateInstanceInternal(typeName, "", args);
        }

        public static object CreateInstance(string typeName)
        {
            return Instance.CreateInstanceInternal(typeName, "", null);
        }


        //======================================================================================


        public void Init(RunMode runMode, bool usePdb)
        {
            Debuger.Log("RunMode:{0}, UsePdb:{1}", runMode, usePdb);

            RunMode = runMode;
            UsePdb = usePdb;

            //首先实例化ILRuntime的AppDomain，AppDomain是一个应用程序域，每个AppDomain都是一个独立的沙盒
            m_appdomain = new AppDomain();

            if (runMode == RunMode.Script)
            {
                //注册Adaptor
                RegisterCrossBindingAdaptor(new ILogTagAdaptor());
                RegisterCrossBindingAdaptor(new GeneralModuleAdaptor());
                RegisterCrossBindingAdaptor(new ILRUILoadingAdaptor());
                RegisterCrossBindingAdaptor(new ILRUIPageAdaptor());
                RegisterCrossBindingAdaptor(new ILRUIWindowAdaptor());
                RegisterCrossBindingAdaptor(new ILRUIWidgetAdaptor());
                RegisterCrossBindingAdaptor(new ILRUIPanelAdaptor());

                //注册重定向器
                RegisterCLRMethodRedirector(new DebugerMethodRedirector());
            }
        }


        public void Clean()
        {
            Debuger.Log();
            if (m_appdomain != null)
            {
                m_appdomain = null;
            }
            m_listNativeAssembly.Clear();
        }


        //========================================================================
        //注册函数
        //========================================================================
        public void RegisterCrossBindingAdaptor(CrossBindingAdaptor adaptor)
        {
            if (m_appdomain != null)
            {
                m_appdomain.RegisterCrossBindingAdaptor(adaptor);
            }
            else
            {
                Debuger.LogError("ILRuntime的AppDomain未创建！");
            }
        }

        public void RegisterCLRMethodRedirector(ICLRMethodRedirector redirector)
        {
            if (m_appdomain != null)
            {
                if (redirector != null)
                {
                    redirector.Init(m_appdomain);
                }
            }
            else
            {
                Debuger.LogError("ILRuntime的AppDomain未创建！");
            }
        }

        //========================================================================
        //加载Assembly
        //========================================================================

        #region 加载Assembly相关函数

        public void AddSearchDirectory(string path)
        {
            Debuger.Log(path);
            if (!m_listSearchDir.Contains(path))
            {
                m_listSearchDir.Add(path);
            }
        }


        private string FindAssemblyFullPath(string assemblyName)
        {
            for (int i = 0; i < m_listSearchDir.Count; i++)
            {
                string path = m_listSearchDir[i] + "/" + assemblyName + ".dll";
                if (File.Exists(path))
                {
                    return path;
                }
            }
            return null;
        }


        public void LoadAssembly(string assemblyName)
        {
            if (RunMode == RunMode.Native)
            {
                LoadAssemblyNative(assemblyName);
            }
            else
            {
                LoadAssemblyScript(assemblyName);
            }
        }


        private void LoadAssemblyNative(string assemblyName)
        {
            Debuger.Log(assemblyName);

            string dllPath = FindAssemblyFullPath(assemblyName);
            if (string.IsNullOrEmpty(dllPath))
            {
                Debuger.LogError("Assembly不存在：{0}", assemblyName);
                return;
            }

            Assembly assembly = Assembly.LoadFile(dllPath);
            if (assembly != null)
            {
                m_listNativeAssembly.Add(assembly);
            }
        }


        private void LoadAssemblyScript(string assemblyName)
        {
            Debuger.Log(assemblyName);

            string dllPath = FindAssemblyFullPath(assemblyName);
            if (string.IsNullOrEmpty(dllPath))
            {
                Debuger.LogError("Assembly不存在：{0}", assemblyName);
                return;
            }


            byte[] dllBytes = FileUtils.ReadFile(dllPath);
            byte[] pdbBytes = null;

            if (UsePdb)
            {
                pdbBytes = FileUtils.ReadFile(GetPdbPath(dllPath));
            }


            try
            {
                using (MemoryStream dll = new MemoryStream(dllBytes))
                {
                    if (pdbBytes != null)
                    {
                        using (MemoryStream pdb = new MemoryStream(pdbBytes))
                        {
                            Debuger.Log("LoadAssembly");
                            m_appdomain.LoadAssembly(dll, pdb, new PdbReaderProvider());
                        }
                    }
                    else
                    {
                        Debuger.Log("LoadAssembly");
                        m_appdomain.LoadAssembly(dll, null, null);
                    }

                }
            }
            catch (Exception e)
            {
                Debuger.LogError(e.Message);
            }
        }

        private string GetPdbPath(string dllPath)
        {
            int i = dllPath.LastIndexOf(".dll");
            return dllPath.Substring(0, i) + ".pdb";
        }
        #endregion


        //========================================================================
        //获取类型
        //========================================================================

        private Type GetNativeType(string typeName)
        {
            Type type = null;
            for (int i = 0; i < m_listNativeAssembly.Count; i++)
            {
                type = m_listNativeAssembly[i].GetType(typeName);
                if (type != null)
                {
                    break;
                }
            }
            return type;
        }


        private IType GetScriptType(string typeName)
        {
            IType type = m_appdomain.GetType(typeName);
            return type;
        }


        //========================================================================
        //函数调用
        //========================================================================

        public object Invoke(string typeName, string methodName, params object[] args)
        {
            Debuger.Log("{0}::{1}({2})", typeName, methodName, args.ToListString());

            if (RunMode == RunMode.Native)
            {
                Type type = GetNativeType(typeName);
                if (type != null)
                {
                    MethodInfo mi = type.GetMethod(methodName);
                    if (mi != null)
                    {
                        return mi.Invoke(null, args);
                    }
                    else
                    {
                        Debuger.LogError("从NativeAssembly中找不到该函数!");
                    }
                }
                else
                {
                    Debuger.LogError("从NativeAssembly中找不到该类型!");
                }
            }
            else
            {
                return m_appdomain.Invoke(typeName, methodName, null, args);
            }
            return null;

        }

        //========================================================================
        //创建实例
        //========================================================================
        private object CreateInstanceInternal(string typeName, string assemblyName, object[] args)
        {
            if (RunMode == RunMode.Native)
            {
                if (string.IsNullOrEmpty(assemblyName))
                {
                    return System.AppDomain.CurrentDomain.GetType().Assembly.CreateInstance(typeName);
                }
                else
                {
                    Type type = Type.GetType(typeName + "," + assemblyName);
                    if (type != null)
                    {
                        return Activator.CreateInstance(type, args);
                    }
                }
            }
            else
            {
                ILTypeInstance obj = m_appdomain.Instantiate(typeName, args);
                if (obj != null)
                {
                    return obj.CLRInstance;
                }
            }

            Debuger.LogError("找不到类型：" + typeName);
            return null;
        }



    }
}
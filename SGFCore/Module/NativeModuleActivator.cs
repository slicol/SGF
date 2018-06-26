/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
 * 模块管理器。
 * Module Manager.
 * 
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
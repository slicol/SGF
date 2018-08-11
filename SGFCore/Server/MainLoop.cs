/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
 * 服务器（模块）管理器
 * Server (module) manager
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

using SGF.Utils;
using System.Threading;

namespace SGF.Server
{
    public class MainLoop
    {
        public static void Run()
        {
            Debuger.Log();
            
            while (true)
            {
                ServerManager.Instance.Tick();
                ConsoleInput.Tick();
                Thread.Sleep(1);
            }
        }
    }
}
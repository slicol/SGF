/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
 * 通过事件的方式监听来自控制台的输入
 * Monitor input from the console by way of events
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
using SGF.SEvent;

namespace SGF.Utils
{


    public class ConsoleInput
    {
        enum InputState
        {
            Idle = 0,
            Inputing = 1
        }
        public readonly static Signal<string> onInputLine = new Signal<string>();
        public readonly static Signal<ConsoleKey> onInputKey = new Signal<ConsoleKey>();

        private static InputState ms_InputState = InputState.Idle;
        private static string ms_InputBuffer = "";

        public static void Tick()
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo info = Console.ReadKey();

                if (ms_InputState == InputState.Inputing)
                {
                    if (info.Key == ConsoleKey.Enter)
                    {
                        ms_InputState = InputState.Idle;
                        Console.WriteLine();//输入框消失
                        onInputLine.Invoke(ms_InputBuffer);
                    }
                    else if (info.Key == ConsoleKey.Escape)
                    {
                        ms_InputState = InputState.Idle;
                        Console.WriteLine();//输入框消失
                    }
                    else if (info.Key >= ConsoleKey.A && info.Key <= ConsoleKey.Z)
                    {
                        ms_InputBuffer += info.KeyChar;
                    }
                    else if (info.Key >= ConsoleKey.D0 && info.Key <= ConsoleKey.D9)
                    {
                        ms_InputBuffer += info.KeyChar;
                    }
                    else 
                    {
                        ms_InputBuffer += info.KeyChar;
                    }
                }
                else if(ms_InputState == InputState.Idle)
                {
                    onInputKey.Invoke(info.Key);
                    
                    if (info.Key == ConsoleKey.Enter)
                    {
                        ms_InputState = InputState.Inputing;
                        ms_InputBuffer = "";
                        //显示一个输入框
                        Console.Write("Input:");
                    }
                }
            }
        }





    }
}
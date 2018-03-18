using System;
using System.Text;
using SGF.Event;

namespace SGF
{


    public class ConsoleInput
    {
        enum InputState
        {
            Idle = 0,
            Inputing = 1
        }
        public readonly static SGFEvent<string> onInputLine = new SGFEvent<string>();
        public readonly static SGFEvent<ConsoleKey> onInputKey = new SGFEvent<ConsoleKey>();

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
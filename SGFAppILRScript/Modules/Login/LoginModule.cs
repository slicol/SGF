using System;
using SGF;
using SGF.Module;
using SGF.Unity.Common;
using SGF.Utils;
using SGFAppDemo.Services;


namespace SGFAppDemo.Modules
{
    public class LoginModule : GeneralModule
    {
        protected override void Show(object arg)
        {
            base.Show(arg);
            Debuger.Log("显示登录界面!(模拟一下)");
            Console.WriteLine("输入命令进行操作");
            Console.WriteLine("命令格式：Login %username%");
            Console.WriteLine("命令格式：Logout");
            Console.WriteLine("命令格式：ShowRoom");
            ConsoleInput.onInputLine.AddListener(OnInputLine);
        }
        
        private void OnInputLine(string line)
        {
            if (line.StartsWith("Login"))
            {
                string[] args = line.Split(' ');
                if (args.Length > 1)
                {
                    string username = args[1];
                    OnlineManager.Instance.Login(username);
                }
                else
                {
                    Debuger.LogError("输入格式错误!");
                }
            }
            else if (line == "Logout")
            {
                OnlineManager.Instance.Logout();
            }
            else if (line == "ShowRoom")
            {
                ModuleManager.Instance.ShowModule(ModuleDef.RoomModule);
            }

        }
        
    }
}
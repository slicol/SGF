using System;
using SGF;
using SGF.Server;
using SGF.Time;
using SGFServerDemo.ZoneServer;

namespace SGFServerDemo
{
    class Program
    {
        public const int SVR_ZONE = 1;
        public const int SVR_GAME = 1;

        static void Main(string[] args)
        {
            InitDebuger();

            SGFTime.DateTimeAppStart = DateTime.Now;

            ServerManager.Instance.StartServer(SVR_ZONE);
            ServerManager.Instance.StartServer(SVR_GAME);

            MainLoop.Run();

            ServerManager.Instance.StopAllServer();
            Console.WriteLine("GameOver");
            Console.ReadKey();
        }

        private static void InitDebuger()
        {
            Debuger.Init(AppDomain.CurrentDomain.BaseDirectory + "/ServerLog/");
            Debuger.EnableLog = false;
            Debuger.EnableSave = true;
            Debuger.Log();
        }
    }
}

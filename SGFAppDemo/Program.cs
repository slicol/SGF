
using System;
using SGF;
using SGF.Time;
using SGFAppDemo.Framework;

namespace SGFAppDemo
{
    class Program
    {

        static void Main(string[] args)
        {
            InitDebuger();



            SGFTime.DateTimeAppStart = DateTime.Now;
            SGFGameObject.Instance(typeof(AppMain));
            SGFGameMainLoop.Run();
            SGFGameObject.DestroyAll();

            Console.WriteLine("GameOver");
            Console.ReadKey();
        }

        private static void InitDebuger()
        {
            Debuger.Init();
            Debuger.EnableLog = true;
            Debuger.EnableSave = true;
            Debuger.Log();
        }
    }
}

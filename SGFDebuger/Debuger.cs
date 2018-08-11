////////////////////////////////////////////////////////////////////
//                            _ooOoo_                             //
//                           o8888888o                            //
//                           88" . "88                            //
//                           (| ^_^ |)                            //
//                           O\  =  /O                            //
//                        ____/`---'\____                         //
//                      .'  \\|     |//  `.                       //
//                     /  \\|||  :  |||//  \                      //
//                    /  _||||| -:- |||||-  \                     //
//                    |   | \\\  -  /// |   |                     //
//                    | \_|  ''\---/''  |   |                     //
//                    \  .-\__  `-`  ___/-. /                     //
//                  ___`. .'  /--.--\  `. . ___                   //
//                ."" '<  `.___\_<|>_/___.'  >'"".                //
//              | | :  `- \`.;`\ _ /`;.`/ - ` : | |               //
//              \  \ `-.   \_ __\ /__ _/   .-` /  /               //
//        ========`-.____`-.___\_____/___.-`____.-'========       //
//                             `=---='                            //
//        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^      //
//            佛祖保佑       无BUG        不修改                   //
////////////////////////////////////////////////////////////////////
/*
 * 描述：
 * 作者：slicol
*/
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace SGF
{
    public interface IDebugerConsole
    {
        void Log(string msg, object context = null);
        void LogWarning(string msg, object context = null);
        void LogError(string msg, object context = null);
    }

    public class UnityDebugerConsole : IDebugerConsole
    {
        private object[] m_tmpArgs = new[] { "" };
        private MethodInfo m_miLog;
        private MethodInfo m_miLogWarning;
        private MethodInfo m_miLogError;

        public UnityDebugerConsole()
        {
            Type type = Type.GetType("UnityEngine.Debug, UnityEngine");
            m_miLog = type.GetMethod("Log", new Type[] { typeof(object) });
            m_miLogWarning = type.GetMethod("LogWarning", new Type[] { typeof(object) });
            m_miLogError = type.GetMethod("LogError", new Type[] { typeof(object) });

        }

        public void Log(string msg, object context = null)
        {
            m_tmpArgs[0] = msg;
            m_miLog.Invoke(null, m_tmpArgs);
        }

        public void LogWarning(string msg, object context = null)
        {
            m_tmpArgs[0] = msg;
            m_miLogWarning.Invoke(null, m_tmpArgs);
        }

        public void LogError(string msg, object context = null)
        {
            m_tmpArgs[0] = msg;
            m_miLogError.Invoke(null, m_tmpArgs);
        }
    }

    public class SystemColorConsole : IDebugerConsole
    {
        public void Log(string msg, object context = null)
        {
            Console.WriteLine(msg);
        }

        public void LogWarning(string msg, object context = null)
        {
            var old = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(msg);
            Console.ForegroundColor = old;
        }

        public void LogError(string msg, object context = null)
        {
            var old = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ForegroundColor = old;

        }
    }


    public interface ILogTag
    {
        string LOG_TAG { get; }
    }

    public static class Debuger
    {
        public static bool EnableLog = true;
        public static bool EnableLogVerbose = false;
        public static bool EnableTime = true;
        public static bool EnableDate = false;
        public static bool EnableSave = false;
        public static bool EnableStack = false;
        public static bool EnableErrorStack = true;
        public static string LogFileDir = "";
        public static string LogFileName = "";
        public static string Prefix = "> ";
        public static StreamWriter LogFileWriter = null;
        private static IDebugerConsole m_console;

        public static void Init(string logFileDir = null, IDebugerConsole console = null)
        {
            LogFileDir = logFileDir;
            m_console = console;
            if (string.IsNullOrEmpty(LogFileDir))
            {
                string path = System.AppDomain.CurrentDomain.BaseDirectory;
                LogFileDir = path + "/DebugerLog/";
            }
        }

        public static void Internal_Log(string msg, object context = null)
        {
            if (m_console != null)
            {
                m_console.Log(msg, context);
            }
            else
            {
                Console.WriteLine(msg);
            }
        }

        public static void Internal_LogWarning(string msg, object context = null)
        {
            if (m_console != null)
            {
                m_console.LogWarning(msg, context);
            }
            else
            {
                Console.WriteLine(msg);
            }
        }

        public static void Internal_LogError(string msg, object context = null)
        {
            if (m_console != null)
            {
                m_console.LogError(msg, context);
            }
            else
            {
                Console.WriteLine(msg);
            }
        }


        #region ////Log(...)////

        //----------------------------------------------------------------------
        //Log
        //----------------------------------------------------------------------

        public static void Log(string message = "")
        {
            if (!Debuger.EnableLog)
            {
                return;
            }

            message = GetLogText(GetLogCaller(true), message);
            Internal_Log(Prefix + message);
            LogToFile("[I]" + message);
        }

        public static void Log(object message)
        {
            if (!Debuger.EnableLog)
            {
                return;
            }

            message = GetLogText(GetLogCaller(true), message.ToString());
            Internal_Log(Prefix + message);
            LogToFile("[I]" + message);
        }

        public static void Log(string format, params object[] args)
        {
            if (!Debuger.EnableLog)
            {
                return;
            }

            string message = GetLogText(GetLogCaller(true), string.Format(format, args));
            Internal_Log(Prefix + message);
            LogToFile("[I]" + message);
        }

        public static void Log(this ILogTag obj, string message = "")
        {
            if (!Debuger.EnableLog)
            {
                return;
            }

            message = GetLogText(GetLogTag(obj), GetLogCaller(), message);
            Internal_Log(Prefix + message);
            LogToFile("[I]" + message);

        }

        public static void Log(this ILogTag obj, string format, params object[] args)
        {
            if (!Debuger.EnableLog)
            {
                return;
            }

            string message = GetLogText(GetLogTag(obj), GetLogCaller(), string.Format(format, args));
            Internal_Log(Prefix + message);
            LogToFile("[I]" + message);
        }

        //----------------------------------------------------------------------
        //LogVerbose
        //----------------------------------------------------------------------
        [Conditional("ENABLE_LOG")]
        public static void LogVerbose(string message = "")
        {
            if (!EnableLog || !EnableLogVerbose)
            {
                return;
            }

            message = GetLogText(GetLogCaller(true), message);
            Internal_Log(Prefix + message);
            LogToFile("[I]" + message);
        }

        [Conditional("ENABLE_LOG")]
        public static void LogVerbose(object message)
        {
            if (!EnableLog || !EnableLogVerbose)
            {
                return;
            }

            message = GetLogText(GetLogCaller(true), message.ToString());
            Internal_Log(Prefix + message);
            LogToFile("[I]" + message);
        }

        [Conditional("ENABLE_LOG")]
        public static void LogVerbose(string format, params object[] args)
        {
            if (!EnableLog || !EnableLogVerbose)
            {
                return;
            }

            string message = GetLogText(GetLogCaller(true), string.Format(format, args));
            Internal_Log(Prefix + message);
            LogToFile("[I]" + message);
        }



        [Conditional("ENABLE_LOG")]
        public static void LogVerbose(this ILogTag obj, string message = "")
        {
            if (!EnableLog || !EnableLogVerbose)
            {
                return;
            }

            message = GetLogText(GetLogTag(obj), GetLogCaller(), message);
            Internal_Log(Prefix + message);
            LogToFile("[I]" + message);

        }

        [Conditional("ENABLE_LOG")]
        public static void LogVerbose(this ILogTag obj, string format, params object[] args)
        {
            if (!EnableLog || !EnableLogVerbose)
            {
                return;
            }

            string message = GetLogText(GetLogTag(obj), GetLogCaller(), string.Format(format, args));
            Internal_Log(Prefix + message);
            LogToFile("[I]" + message);
        }

        //----------------------------------------------------------------------
        //LogWarning
        //----------------------------------------------------------------------

        public static void LogWarning(string message)
        {
            message = GetLogText(GetLogCaller(true), message);
            Internal_LogWarning(Prefix + message);
            LogToFile("[W]" + message);
        }

        public static void LogWarning(object message)
        {
            message = GetLogText(GetLogCaller(true), message.ToString());
            Internal_LogWarning(Prefix + message);
            LogToFile("[W]" + message);
        }

        public static void LogWarning(string format, params object[] args)
        {
            string message = GetLogText(GetLogCaller(true), string.Format(format, args));
            Internal_LogWarning(Prefix + message);
            LogToFile("[W]" + message);
        }


        public static void LogWarning(this ILogTag obj, string message)
        {
            message = GetLogText(GetLogTag(obj), GetLogCaller(), message);
            Internal_LogWarning(Prefix + message);
            LogToFile("[W]" + message);
        }


        public static void LogWarning(this ILogTag obj, string format, params object[] args)
        {
            string message = GetLogText(GetLogTag(obj), GetLogCaller(), string.Format(format, args));
            Internal_LogWarning(Prefix + message);
            LogToFile("[W]" + message);
        }

        //----------------------------------------------------------------------
        //LogWarning
        //----------------------------------------------------------------------

        public static void LogError(string message)
        {
            message = GetLogText(GetLogCaller(true), message);
            Internal_LogError(Prefix + message);
            LogToFile("[E]" + message, EnableErrorStack);
        }

        public static void LogError(object message)
        {
            message = GetLogText(GetLogCaller(true), message.ToString());
            Internal_LogError(Prefix + message);
            LogToFile("[E]" + message, EnableErrorStack);
        }


        public static void LogError(string format, params object[] args)
        {
            string message = GetLogText(GetLogCaller(true), string.Format(format, args));
            Internal_LogError(Prefix + message);
            LogToFile("[E]" + message, EnableErrorStack);
        }


        public static void LogError(this ILogTag obj, string message)
        {
            message = GetLogText(GetLogTag(obj), GetLogCaller(), message);
            Internal_LogError(Prefix + message);
            LogToFile("[E]" + message, EnableErrorStack);
        }


        public static void LogError(this ILogTag obj, string format, params object[] args)
        {
            string message = GetLogText(GetLogTag(obj), GetLogCaller(), string.Format(format, args));
            Internal_LogError(Prefix + message);
            LogToFile("[E]" + message, EnableErrorStack);
        }
        #endregion

        //----------------------------------------------------------------------
        //工具函数
        //----------------------------------------------------------------------

        private static string GetLogText(string tag, string methodName, string message)
        {
            string str = "";
            if (Debuger.EnableDate)
            {
                DateTime now = DateTime.Now;
                str = now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " ";
            }
            else if (Debuger.EnableTime)
            {
                DateTime now = DateTime.Now;
                str = now.ToString("HH:mm:ss.fff") + " ";
            }

            str = str + tag + "::" + methodName + "() " + message;
            return str;
        }

        private static string GetLogText(string caller, string message)
        {
            string str = "";
            if (Debuger.EnableDate)
            {
                DateTime now = DateTime.Now;
                str = now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " ";
            }
            else if (Debuger.EnableTime)
            {
                DateTime now = DateTime.Now;
                str = now.ToString("HH:mm:ss.fff") + " ";
            }

            str = str + caller + "() " + message;
            return str;
        }


        private static string GetLogTag(ILogTag obj)
        {
            return obj.LOG_TAG;
            /*
            ILogTag tag = obj as ILogTag;
            if (tag != null)
            {
                return tag.LOG_TAG;
            }
            return obj.GetType().Name;
            */
        }

        private static Assembly ms_Assembly;
        private static string GetLogCaller(bool bIncludeClassName = false)
        {
            StackTrace st = new StackTrace(2, false);
            if (st != null)
            {
                if (null == ms_Assembly)
                {
                    ms_Assembly = typeof(Debuger).Assembly;
                }

                int currStackFrameIndex = 0;
                while (currStackFrameIndex < st.FrameCount)
                {
                    StackFrame oneSf = st.GetFrame(currStackFrameIndex);
                    MethodBase oneMethod = oneSf.GetMethod();

                    if (oneMethod.Module.Assembly != ms_Assembly)
                    {
                        if (bIncludeClassName)
                        {
                            return oneMethod.DeclaringType.Name + "::" + oneMethod.Name;
                        }
                        else
                        {
                            return oneMethod.Name;
                        }
                    }

                    currStackFrameIndex++;
                }

            }

            return "";
        }


        //----------------------------------------------------------------------
        public static string CheckLogFileDir()
        {
            if (string.IsNullOrEmpty(LogFileDir))
            {
                Internal_LogError("Debuger::CheckLogFileDir() LogFileDir is NULL!");
                return "";
            }

            try
            {
                if (!Directory.Exists(LogFileDir))
                {
                    Directory.CreateDirectory(LogFileDir);
                }
            }
            catch (Exception e)
            {
                Internal_LogError("Debuger::CheckLogFileDir() " + e.Message + e.StackTrace);
                return "";
            }

            return LogFileDir;
        }



        public static string GenLogFileName()
        {
            DateTime now = DateTime.Now;
            string filename = now.GetDateTimeFormats('s')[0].ToString();//2005-11-05T14:06:25
            filename = filename.Replace("-", "_");
            filename = filename.Replace(":", "_");
            filename = filename.Replace(" ", "");
            filename += ".log";

            return filename;
        }



        private static void LogToFile(string message, bool EnableStack = false)
        {
            if (!EnableSave)
            {
                return;
            }

            if (LogFileWriter == null)
            {
                LogFileName = GenLogFileName();
                LogFileDir = CheckLogFileDir();
                if (string.IsNullOrEmpty(LogFileDir))
                {
                    return;
                }

                string fullpath = LogFileDir + LogFileName;
                try
                {
                    LogFileWriter = File.AppendText(fullpath);
                    LogFileWriter.AutoFlush = true;
                }
                catch (Exception e)
                {
                    LogFileWriter = null;
                    Internal_LogError("Debuger::LogToFile() " + e.Message + e.StackTrace);
                    return;
                }
            }

            if (LogFileWriter != null)
            {
                try
                {
                    LogFileWriter.WriteLine(message);
                    if ((EnableStack || Debuger.EnableStack))
                    {
                        StackTrace st = new StackTrace(2, false);
                        LogFileWriter.WriteLine(st.ToString());
                    }
                }
                catch (Exception)
                {
                    return;
                }
            }
        }
    }
}

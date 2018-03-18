using System;
using System.Collections;
using System.Text;

namespace SGF.Extension
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// 对象如果为Null，抛出异常
        /// </summary>
        /// <param name="o"></param>
        /// <param name="message">异常消息</param>
        public static void ThrowIfNull(this object o, string message)
        {
            if (o == null) throw new NullReferenceException(message);
        }



    }

}
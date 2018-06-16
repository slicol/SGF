using System;

namespace SGF.Extension
{
    public static class StringExtensions
    {
        public static int ToInt(this string target, int defaultValue = 0)
        {
            int.TryParse(target, out defaultValue);
            return defaultValue;
        }

        public static uint ToUInt(this string target, uint defaultValue = 0)
        {
            uint.TryParse(target, out defaultValue);
            return defaultValue;
        }

        public static long ToLong(this string target, long defaultValue = 0)
        {
            long.TryParse(target, out defaultValue);
            return defaultValue;
        }

        public static ulong ToULong(this string target, ulong defaultValue = 0)
        {
            ulong.TryParse(target, out defaultValue);
            return defaultValue;
        }

        public static double ToDouble(this string target, double defaultValue = 0)
        {
            double.TryParse(target, out defaultValue);
            return defaultValue;
        }

        public static float ToFloat(this string target, float defaultValue = 0)
        {
            float.TryParse(target, out defaultValue);
            return defaultValue;
        }
    }
}
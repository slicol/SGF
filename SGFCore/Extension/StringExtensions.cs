/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
 * 提供String的扩展方法
 * Provide String extension method
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
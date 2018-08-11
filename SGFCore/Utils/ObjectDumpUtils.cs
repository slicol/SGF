/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
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

using System.Collections;
using System.Text;

namespace SGF.Utils
{
    public class ObjectDumpUtils
    {
        public static void Dump(string name, object obj, StringBuilder result, string prefix = "")
        {
            if (obj == null)
            {
                return;
            }


            var type = obj.GetType();
            if (type == typeof(int) ||
                type == typeof(uint) ||
                type == typeof(short) ||
                type == typeof(ushort) ||
                type == typeof(long) ||
                type == typeof(ulong) ||
                type == typeof(byte) ||
                type == typeof(float) ||
                type == typeof(double) ||
                type == typeof(string) ||
                type == typeof(bool)
            )
            {
                result.AppendFormat("{0}{1}:{2}\n", prefix, name, obj);
                return;
            }

            result.AppendFormat("{0}{1}:\n", prefix, name);

            if (obj is IList)
            {
                var list = obj as IList;
                for (int i = 0; i < list.Count; i++)
                {
                    var tmp = list[i];
                    Dump("[" + i + "]", tmp, result, prefix + "    ");
                }
                return;
            }

            var fields = type.GetFields();
            for (int i = 0; i < fields.Length; i++)
            {
                var fi = fields[i];
                var f = fi.GetValue(obj);

                if (fi.Name == "MaxValue" || fi.Name == "MinValue")
                {
                    continue;
                }

                Dump(fi.Name, f, result, prefix + "    ");
            }
        }
    }
}
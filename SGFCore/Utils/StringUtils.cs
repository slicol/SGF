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

using System;
using System.Collections;
using System.Text;

namespace SGF.Utils
{
    public static class StringUtils
    {
        private static StringBuilder ms_temp = new StringBuilder();

        public static string ToString(Array target)
        {
            ms_temp.Length = 0;
            ms_temp.Append("[");
            if (target.Length > 0)
            {
                ms_temp.Append(target.GetValue(0));
            }

            for (int i = 1; i < target.Length; i++)
            {
                ms_temp.Append(",");
                ms_temp.Append(target.GetValue(i));
            }
            ms_temp.Append("]");
            return ms_temp.ToString();
        }

        public static string ToString(IList target)
        {
            ms_temp.Length = 0;
            ms_temp.Append("[");
            if (target.Count > 0)
            {
                ms_temp.Append(target[0]);
            }

            for (int i = 1; i < target.Count; i++)
            {
                ms_temp.Append(",");
                ms_temp.Append(target[i]);
            }
            ms_temp.Append("]");
            return ms_temp.ToString();
        }

    }
}
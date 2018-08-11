/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
 * 封装Dictionary以提供简捷而安全的使用方式
 * Encapsulate Dictionary to provide simple and safe use
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
using System.Collections.Generic;

namespace SGF.Utils
{ 
    public class DictionarySafe<TKey, TValue> : Dictionary<TKey, TValue> 
    {
        public new TValue this[TKey key]
        {
            set { base[key] = value; }
            get
            {
                TValue value = default(TValue);
                TryGetValue(key, out value);
                return value;
            }
        }


    }
}

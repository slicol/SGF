/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
 * 将Dictionary与List封装在一起，以满足既需要快速遍历，又需要快速按Key存取的需求
 * Wrap the Dictionary and List together to meet the need for fast traversal and fast key access
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
using System.Linq;


namespace SGF.Utils
{
    public class MapList<TKey, TValue>
    {
        private Dictionary<TKey, TValue> m_map;
        private List<TValue> m_list;

        public MapList(int capacity)
        {
            m_map = new Dictionary<TKey, TValue>(capacity);
            m_list = new List<TValue>(capacity);
        }

        public MapList()
        {
            m_map = new Dictionary<TKey, TValue>();
            m_list = new List<TValue>();
        }

        public List<TValue> AsList()
        {
            return m_list;
        }

        public Dictionary<TKey, TValue> AsDictionary()
        {
            return m_map;
        }

        public List<TValue> ToList()
        {
            return m_list.ToList();
        }

        public TValue[] ToArray()
        {
            return m_list.ToArray();
        }

        public new TValue this[TKey indexKey]
        {
            set
            {
                if (m_map.ContainsKey(indexKey))
                {
                    TValue v = m_map[indexKey];
                    m_map[indexKey] = value;
                    m_list.Remove(v);
                    m_list.Add(value);
                }
                else
                {
                    m_map.Add(indexKey, value);
                    m_list.Add(value);
                }

            }
            get
            {
                TValue value = default(TValue);
                m_map.TryGetValue(indexKey, out value);
                return value;
                
            }
        }

        public bool Add(TKey key, TValue value)
        {
            if (m_map.ContainsKey(key))
            {
                return false;
            }
            m_map.Add(key, value);
            m_list.Add(value);
            return true;
        }

        public bool Remove(TKey key)
        {
            if (m_map.ContainsKey(key))
            {
                TValue v = m_map[key];
                m_list.Remove(v);
                return m_map.Remove(key);
            }
            return false;
        }

        public bool RemoveAt(int index, TKey key)
        {
            m_list.RemoveAt(index);
            return m_map.Remove(key);
        }



        public void Clear()
        {
            m_map.Clear();
            m_list.Clear();
        }

        public int Count
        {
            get { return m_list.Count; }
        }
        public bool ContainsKey(TKey key)
        {
            return m_map.ContainsKey(key);
        }


    }
}

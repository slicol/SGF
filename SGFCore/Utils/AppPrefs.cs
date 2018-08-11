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
using MiniJSON.Safe;

namespace SGF.Utils
{
    public static class AppPrefs
    {
        private static string m_path;
        private static JsonObject m_jobj = new JsonObject();

        public static void Init(string path)
        {
            m_path = path;
            var str = FileUtils.ReadString(m_path);
            if (!string.IsNullOrEmpty(str))
            {
                try
                {
                    m_jobj = Json.Deserialize(str) as JsonObject;
                }
                catch (Exception e)
                {
                    
                }
            }

            if(m_jobj == null) m_jobj = new JsonObject();
        }
        
        //
        public static float GetFloat(string key, float defaultValue = 0)
        {
            var obj = m_jobj[GetKey(key)];
            if (obj == null) return defaultValue;
            return (float)Convert.ToDouble(obj);
        }

        public static int GetInt(string key, int defaultValue = 0)
        {
            var obj = m_jobj[GetKey(key)];
            if (obj == null) return defaultValue;
            return (int)Convert.ToInt32(obj);
        }

        public static string GetString(string key, string defaultValue = "")
        {
            var obj = m_jobj[GetKey(key)];
            if (obj == null) return defaultValue;
            return (string)obj;
            
        }

        //
        public static bool HasKey(string key)
        {
            return m_jobj.ContainsKey(GetKey(key));
        }

        //
        public static void SetFloat(string key, float value)
        {
            m_jobj[GetKey(key)] = value;
        }

        //
        public static void SetInt(string key, int value)
        {
            m_jobj[GetKey(key)] = value;
        }

        //
        public static void SetString(string key, string value)
        {
            m_jobj[GetKey(key)] = value;
        }

        public static void DeleteAll()
        {
            m_jobj.Clear();
        }

        //
        public static void DeleteKey(string key)
        {
            if (m_jobj.ContainsKey(GetKey(key)))
            {
                m_jobj.Remove(GetKey(key));
            }
        }

        public static void Save()
        {
            var str = Json.Serialize(m_jobj);
            FileUtils.SaveFile(m_path, str);
        }

        private static string GetKey(string key)
        {
            return key;
        }

    }
}
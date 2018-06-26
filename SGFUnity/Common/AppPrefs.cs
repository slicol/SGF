/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
 * AppPrefs
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

using UnityEngine;

namespace SGF.Unity.Common
{
    public class AppPrefs
    {
        private static string ms_prefix = "";
        public static void Init(string keyPrefix)
        {
            ms_prefix = keyPrefix;
        }

        public static float GetFloat(string key)
        {
            return PlayerPrefs.GetFloat(GetKey(key));
        }

        //
        public static float GetFloat(string key, float defaultValue)
        {
            return PlayerPrefs.GetFloat(GetKey(key), defaultValue);
        }

        public static int GetInt(string key)
        {
            return PlayerPrefs.GetInt(GetKey(key));
        }

        //
        public static int GetInt(string key, int defaultValue)
        {
            return PlayerPrefs.GetInt(GetKey(key), defaultValue);
        }

        //
        public static string GetString(string key)
        {
            return PlayerPrefs.GetString(GetKey(key));
        }

        //
        public static string GetString(string key, string defaultValue)
        {
            return PlayerPrefs.GetString(GetKey(key), defaultValue);
        }

        //
        public static bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(GetKey(key));
        }

        //
        public static void SetFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(GetKey(key), value);
        }

        //
        public static void SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(GetKey(key), value);
        }

        //
        public static void SetString(string key, string value)
        {
            PlayerPrefs.SetString(GetKey(key), value);
        }

        public static void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
        }

        //
        public static void DeleteKey(string key)
        {
            PlayerPrefs.DeleteKey(key);
        }

        public static void Save()
        {
            PlayerPrefs.Save();
        }

        public static string GetKey(string key)
        {
            return ms_prefix + "_" + key;
        }
    }
}


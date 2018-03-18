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


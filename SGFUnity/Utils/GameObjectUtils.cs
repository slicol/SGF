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
using System;
using UnityEngine;




namespace SGF.Unity.Common
{
    public class GameObjectUtils
    {
        public static T EnsureComponent<T>(GameObject target) where T : Component
        {
            T comp = target.GetComponent<T>();
            if (comp == null)
            {
                return target.AddComponent<T>();
            }
            return comp;
        }

        public static Component EnsureComponent(GameObject target, Type type)
        {
            Component comp = target.GetComponent(type);
            if (comp == null)
            {
                return target.AddComponent(type);
            }
            return comp;
        }

        public static T FindComponent<T>(GameObject target, string path) where T : Component
        {
            GameObject obj = FindGameObject(target, path);
            if (obj != null)
            {
                return obj.GetComponent<T>();
            }
            return default(T);
        }

        public static GameObject FindGameObject(GameObject target, string path)
        {
            if (target != null)
            {
                Transform t = target.transform.Find(path);
                if (t != null)
                {
                    return t.gameObject;
                }
            }

            return null;

        }


        private static GameObject FindGameObject2(GameObject target, string path)
        {
            if (target == null)
            {
                return null;
            }

            string[] array = path.Split('.');
            Transform current = target.transform;

            for (int i = 0; i < array.Length; ++i)
            {
                string name = array[i];
                Transform child = current.Find(name);
                if (child != null)
                {
                    current = child;
                }
                else
                {
                    char[] c = name.ToCharArray();
                    if (Char.IsLower(c[0]))
                    {
                        c[0] = Char.ToUpper(c[0]);
                    }
                    else
                    {
                        c[0] = Char.ToLower(c[0]);
                    }

                    name = new string(c);
                    child = current.Find(name);
                    if (child != null)
                    {
                        current = child;
                    }
                    else
                    {
                        current = null;
                        break;
                    }
                }
            }

            if (current == null)
            {
                return null;
            }

            return current.gameObject;

        }

        public static GameObject FindGameObjbyName(string name, GameObject root)
        {
            if (root == null)
            {
                return GameObject.Find(name);
            }

            Transform[] childs = root.GetComponentsInChildren<Transform>();

            foreach (Transform trans in childs)
            {
                if (trans.gameObject.name.Equals(name))
                {
                    return trans.gameObject;
                }
            }

            return null;
        }


        public static GameObject FindFirstGameObjByPrefix(string prefix, GameObject root)
        {
            Transform[] childs;
            if (root != null)
            {
                childs = root.GetComponentsInChildren<Transform>();
            }
            else
            {
                childs = GameObject.FindObjectsOfType<Transform>();
            }

            foreach (Transform trans in childs)
            {
                if (trans.gameObject.name.Length >= prefix.Length)
                {
                    if (trans.gameObject.name.Substring(0, prefix.Length) == prefix)
                    {
                        return trans.gameObject;
                    }
                }

            }

            return null;
        }


        public static void SetActiveRecursively(GameObject target, bool bActive)
        {
#if (!UNITY_3_5)
            for (int n = target.transform.childCount - 1; 0 <= n; n--)
                if (n < target.transform.childCount)
                    SetActiveRecursively(target.transform.GetChild(n).gameObject, bActive);
            target.SetActive(bActive);
#else
		target.SetActiveRecursively(bActive);
#endif
        }
        public static void SetLayerRecursively(GameObject target, int layer)
        {
#if (!UNITY_3_5)
            for (int n = target.transform.childCount - 1; 0 <= n; n--)
            {
                if (n < target.transform.childCount)
                {
                    SetLayerRecursively(target.transform.GetChild(n).gameObject, layer);
                }
            }
            target.layer = layer;
#else
		target.SetActiveRecursively(bActive);
#endif
        }


    }

}

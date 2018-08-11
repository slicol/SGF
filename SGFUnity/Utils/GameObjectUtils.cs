/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
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
using UnityEngine;




namespace SGF.Unity.Utils
{
    public static class GameObjectUtils
    {
        public static T EnsureComponent<T>(this GameObject target) where T : Component
        {
            T comp = target.GetComponent<T>();
            if (comp == null)
            {
                return target.AddComponent<T>();
            }
            return comp;
        }

        public static Component EnsureComponent(this GameObject target, Type type)
        {
            Component comp = target.GetComponent(type);
            if (comp == null)
            {
                return target.AddComponent(type);
            }
            return comp;
        }

        public static T FindComponent<T>(this GameObject target, string path) where T : Component
        {
            GameObject obj = FindGameObject(target, path);
            if (obj != null)
            {
                return obj.GetComponent<T>();
            }
            return default(T);
        }

        public static GameObject FindGameObject(this GameObject target, string path)
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

        public static GameObject FindGameObjbyName(this GameObject root, string name)
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


        public static GameObject FindFirstGameObjByPrefix(this GameObject root, string prefix)
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


        public static void SetActiveRecursively(this GameObject target, bool bActive)
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
        public static void SetLayerRecursively(this GameObject target, int layer)
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

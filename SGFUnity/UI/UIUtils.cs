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


using SGF.Unity.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SGF.Unity.UI
{
    /// <summary>
    /// 为UI操作提供基础封装，使UI操作更方便
    /// </summary>
    public static class UIUtils
    {
        /// <summary>
        /// 设置一个UI元素是否可见
        /// </summary>
        /// <param name="ui"></param>
        /// <param name="value"></param>
        public static void SetActive(UIBehaviour ui, bool value)
        {
            if (ui != null && ui.gameObject != null)
            {
                GameObjectUtils.SetActiveRecursively(ui.gameObject, value);
            }
        }


        public static void SetButtonText(Button btn, string text)
        {
            Text objText = btn.transform.GetComponentInChildren<Text>();
            if (objText != null)
            {
                objText.text = text;
            }
        }

        public static string GetButtonText(Button btn)
        {
            Text objText = btn.transform.GetComponentInChildren<Text>();
            if (objText != null)
            {
                return objText.text;
            }
            return "";
        }

        public static void SetChildText(UIBehaviour ui, string text)
        {
            Text objText = ui.transform.GetComponentInChildren<Text>();
            if (objText != null)
            {
                objText.text = text;
            }
        }


        /// <summary>
        /// 方便寻找Panel上的UI控件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="controlName"></param>
        /// <returns></returns>
        public static T Find<T>(MonoBehaviour parent, string controlName) where T : MonoBehaviour
        {
            Transform target = parent.transform.Find(controlName);
            if (target != null)
            {
                return target.GetComponent<T>();
            }
            else
            {
                Debuger.LogError("未找到UI控件：{0}", controlName);
                return default(T);
            }
        }
    }
}
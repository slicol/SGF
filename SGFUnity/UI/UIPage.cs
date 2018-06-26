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


using UnityEngine;
using UnityEngine.UI;

namespace SGF.Unity.UI
{
    public abstract class UIPage:UIPanel
    {
        public override UITypeDef UIType { get { return UITypeDef.Page; } }

        /// <summary>
        /// 返回按钮，大部分Page都会有返回按钮
        /// </summary>
        [SerializeField]
        private Button m_btnGoBack;


        /// <summary>
        /// 当UIPage被激活时调用
        /// </summary>
        protected override void OnEnable()
        {
            Debuger.Log();
            AddUIClickListener(m_btnGoBack, OnBtnGoBack);
        }

        /// <summary>
        /// 当UI不可用时调用
        /// </summary>
        protected override void OnDisable()
        {
            Debuger.Log();
            RemoveUIClickListeners(m_btnGoBack);
        }

        /// <summary>
        /// 当点击“返回”时调用
        /// 但是并不是每一个Page都有返回按钮
        /// </summary>
        private void OnBtnGoBack()
        {
            Debuger.Log();
            UIManager.Instance.GoBackPage();
        }




    }
}
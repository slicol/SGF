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
    public abstract class UIWindow:UIPanel
    {
        public override UITypeDef UIType { get { return UITypeDef.Window; } }

        [SerializeField]
        private Button m_btnClose;

        /// <summary>
        /// 当UI可用时调用
        /// </summary>
        protected override void OnEnable()
        {
            Debuger.Log();
            AddUIClickListener(m_btnClose, OnBtnClose);
        }

        /// <summary>
        /// 当UI不可用时调用
        /// </summary>
        protected override void OnDisable()
        {
            Debuger.Log();
            RemoveUIClickListeners(m_btnClose);
        }

        private void OnBtnClose()
        {
            Close(0);
        }

    }
}
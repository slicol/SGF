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


using SGF.Unity.UI.UILib.Control;
using UnityEngine;
using UnityEngine.UI;

namespace SGF.Unity.UI.UILib
{
    public class UISimpleLoading:UILoading
    {
        public CtlProgressBar progressBar;
        public Image rotateIcon;

        private UILoadingArg m_arg;
        public UILoadingArg arg { get { return m_arg; } }
        protected override void OnOpen(object arg = null)
        {
            base.OnOpen(arg);
            m_arg = arg as UILoadingArg;
            if(m_arg == null)
            {
                m_arg = new UILoadingArg();
            }
            UpdateText();
        }

        protected override void OnClose(object arg = null)
        {
            m_arg = null;
            base.OnClose(arg);
        }

        public void ShowProgress(string title, float progress)
        {
            m_arg.title = title;
            m_arg.progress = progress;
        }

        public void ShowProgress(float progress)
        {
            m_arg.progress = progress;
        }

        private void UpdateText()
        {
            if (txtTitle != null)
            {
                txtTitle.text = m_arg.title + "(" + (int)(m_arg.progress * 100) + "%)";
            }
            if (txtTips != null)
            {
                txtTips.text = m_arg.tips;
            }
        }

        private void UpdateProgress()
        {
            if (progressBar != null)
            {
                progressBar.SetData(m_arg.progress);
            }
        }

        protected override void OnUpdate()
        {
            if (m_arg != null)
            {
                UpdateText();
                UpdateProgress();
            }

            if (rotateIcon != null)
            {
                rotateIcon.transform.Rotate(new Vector3(0, 0, -45 * UnityEngine.Time.deltaTime));
            }
        }

    }
}
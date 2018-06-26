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


using UnityEngine.UI;

namespace SGF.Unity.UI
{
    public class UILoadingArg
    {
        public string title = "";
        public string tips = "";
        public float progress = 0;//0~1

        public override string ToString()
        {
            return string.Format("title:{0}, tips:{1}, progress:{2}", title, tips, progress);
        }
    }

    public abstract class UILoading:UIPanel
    {
        public override UITypeDef UIType { get { return UITypeDef.Loading; } }

        public Text txtTitle;
        public Text txtTips;

        private UILoadingArg m_arg;
        public UILoadingArg arg { get { return m_arg; } }


        protected override void OnOpen(object arg = null)
        {
            base.OnOpen(arg);

            m_arg = arg as UILoadingArg;
            if (m_arg == null)
            {
                m_arg = new UILoadingArg();
            }
            UpdateText();
        }

        public void ShowProgress(string title, float progress)
        {
            m_arg.tips = title;
            m_arg.progress = progress;
        }

        public void ShowProgress(float progress)
        {
            m_arg.progress = progress;
        }


        protected override void OnUpdate()
        {
            base.OnUpdate();

            if (m_arg != null)
            {
                UpdateText();
                UpdateProgress();
            }
        }

        protected virtual void UpdateProgress()
        {
            
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

    }
}
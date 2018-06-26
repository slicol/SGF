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


using UnityEngine.EventSystems;

namespace SGF.Unity.UI.UILib
{
    public class UIMsgTips:UIWidget
    {
        public UIBehaviour ctlTextBar;
        private const float MaxYOffset = 20f;
        private float m_alpha = 1;
        private float m_yOffset = MaxYOffset;

        protected override void OnOpen(object arg = null)
        {
            base.OnOpen(arg);

            UIUtils.SetChildText(ctlTextBar, arg as string);

            m_yOffset = MaxYOffset;
            m_alpha = 1;
            UpdateView();
        }

        void Update()
        {
            m_alpha -= 0.01f;
            if (m_alpha < 0)
            {
                m_alpha = 0;
                this.Close();
            }

            m_yOffset -= 0.1f;
            if (m_yOffset < 0)
            {
                m_yOffset = 0;
            }

            UpdateView();
        }

        private void UpdateView()
        {

            ctlTextBar.transform.SetLocalY(MaxYOffset - m_yOffset);
        }

    }
}
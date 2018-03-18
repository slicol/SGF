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
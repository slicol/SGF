using UnityEngine;
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
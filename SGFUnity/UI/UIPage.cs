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
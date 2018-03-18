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
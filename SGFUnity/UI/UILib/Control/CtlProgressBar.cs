using UnityEngine.UI;

namespace SGF.Unity.UI.UILib.Control
{
    public class CtlProgressBar:UIControl
    {
        public Image imgProgressValue;
        public float progress = 0;


        void Start()
        {
            SetData(0f);
        }

        public override void SetData(object data)
        {
            this.progress = (float)data;
            imgProgressValue.fillAmount = progress;

        }
    }
}
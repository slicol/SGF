using SGF.Unity.UI;

namespace SGF.Unity.UI
{
    public abstract class UIWidget : UIPanel
    {
        public override UITypeDef UIType { get { return UITypeDef.Widget; } }


    }
}
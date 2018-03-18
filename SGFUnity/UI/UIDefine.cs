namespace SGF.Unity.UI
{
    public enum UITypeDef
    {
        Unkown = 0,
        Page = 1,
        Window=2,
        Widget = 3,
        Loading =4
    }


    public class UILayerDef
    {
        public const int Background = 0;
        public const int Page = 1000;//-1999
        public const int NormalWindow = 2000;//-2999
        public const int TopWindow = 3000;//-3999
        public const int Widget = 4000;//-4999
        public const int Loading = 5000;
        public const int Unkown = 9999;

        public static int GetDefaultLayer(UITypeDef type)
        {
            switch (type)
            {
                case UITypeDef.Loading: return Loading;
                case UITypeDef.Widget: return Widget;
                case UITypeDef.Window: return NormalWindow;
                case UITypeDef.Page: return Page;
                case UITypeDef.Unkown: return Unkown;
                default: return Unkown;
            }
        }

    }


}
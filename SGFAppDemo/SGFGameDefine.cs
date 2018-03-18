namespace SGFAppDemo
{
    public static class SGFGameDefine
    {
        public static int FPS = 30;
        public static int FRAME_TICK_INTERVAL{get { return (int)(FRAME_INTERVAL * 10000); } }
        public static float FRAME_INTERVAL { get { return 1000 / FPS; } }

    }
}
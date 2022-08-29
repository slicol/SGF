using SGF.SEvent;

namespace SGFAppDemo
{
    /// <summary>
    /// 全局事件
    /// 有些事件不确定应该是由谁发出
    /// 就可以通过全局事件来收和发
    /// </summary>
    public static class GlobalEvent
    {
        public static Signal onVersionUpdateComplete = new Signal();
        public static Signal onFixedUpdate = new Signal();
        public static Signal<float> onUpdate = new Signal<float>();
    }
}
////////////////////////////////////////////////////////////////////
//                            _ooOoo_                             //
//                           o8888888o                            //
//                           88" . "88                            //
//                           (| ^_^ |)                            //
//                           O\  =  /O                            //
//                        ____/`---'\____                         //
//                      .'  \\|     |//  `.                       //
//                     /  \\|||  :  |||//  \                      //
//                    /  _||||| -:- |||||-  \                     //
//                    |   | \\\  -  /// |   |                     //
//                    | \_|  ''\---/''  |   |                     //
//                    \  .-\__  `-`  ___/-. /                     //
//                  ___`. .'  /--.--\  `. . ___                   //
//                ."" '<  `.___\_<|>_/___.'  >'"".                //
//              | | :  `- \`.;`\ _ /`;.`/ - ` : | |               //
//              \  \ `-.   \_ __\ /__ _/   .-` /  /               //
//        ========`-.____`-.___\_____/___.-`____.-'========       //
//                             `=---='                            //
//        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^      //
//            佛祖保佑       无BUG        不修改                   //
////////////////////////////////////////////////////////////////////
/*
 * 描述：
 * 作者：slicol
*/
using System;
using System.Collections.Generic;
using SGF.Math;
using SGF.Network.protocol;


namespace SGF.Network
{
    public static class NetDebuger
    {
        public const string LOG_TAG = "NetDebuger";
        public static string DbgFileName { get; set; }
        public static string DbgFileDir { get; set; }

        //===========================================================
        public static bool EnableLog = false;
        public static bool EnableProfiler = false;

        //===========================================================
        //弱网络测试
        public static bool EnableWeakNet = false;

        public static float PacketLossRate = 0;//丢包率
        public static float JitterRate = 0;//抖动率
        public static int JitterDelayMin = 0;//MS
        public static int JitterDelayMax = 0;
        //===========================================================

        //=========================================================================
        //=========================================================================
        #region Profiler采样
        private static List<NetSampleItem> m_ListNetSamples = new List<NetSampleItem>();

        public static List<NetSampleItem> SampleList
        {
            get { return m_ListNetSamples; }
        }

        public static void ClearSample()
        {
            m_ListNetSamples.Clear();
        }

        public static void AddSample(string tag, string name, int data1 = 0, int data2 = 0, int data3 = 0)
        {
            if (EnableProfiler)
            {
                NetSampleItem item = new NetSampleItem();
                item.name = tag + ":" + name;
                item.time = DateTime.Now.Ticks;
                item.data1 = data1;
                item.data2 = data2;
                item.data3 = data3;

                m_ListNetSamples.Add(item);
            }
        }
        #endregion

        //=========================================================================
        //弱网络测试
        #region 弱网络测试
        public static int GetJitterDelay()
        {
            if (IsNetJitter())
            {
                return SGFRandom.Default.Range(JitterDelayMin, JitterDelayMax);
            }
            return 0;
        }

        public static bool IsNetJitter()
        {
            return EnableWeakNet && SGFRandom.Default.Rnd() < JitterRate;
        }

        public static bool IsPacketLoss()
        {
            return EnableWeakNet && SGFRandom.Default.Rnd() < PacketLossRate;
        }

        public static void WeakNetSimulate(object target, byte[] buffer, int size, Action<byte[], int> handler)
        {
            
        }

        public static void WeakNetCancel(object target)
        {

        }

        private static void OnDelayInvoke(object[] args)
        {
            Action<byte[], int> handler = (Action<byte[], int>)args[0];

            if (handler != null)
            {
                handler((byte[])args[1], (int)args[2]);
            }
        }

        #endregion
    }
}



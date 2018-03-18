
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


namespace SGF.Common
{
    public class MapList<TKey, TValue>
    {
        private Dictionary<TKey, TValue> m_map = new Dictionary<TKey, TValue>();
        private List<TValue> m_list = new List<TValue>();

        public List<TValue> AsList()
        {
            return m_list;
        }

        public Dictionary<TKey, TValue> AsDictionary()
        {
            return m_map;
        }

        public TValue[] ToArray()
        {
            return m_list.ToArray();
        }

        public new TValue this[TKey indexKey]
        {
            set
            {
                if (m_map.ContainsKey(indexKey))
                {
                    TValue v = m_map[indexKey];
                    m_map[indexKey] = value;
                    m_list.Remove(v);
                    m_list.Add(value);
                }
                else
                {
                    m_map.Add(indexKey, value);
                    m_list.Add(value);
                }

            }
            get
            {
                TValue value = default(TValue);
                m_map.TryGetValue(indexKey, out value);
                return value;
                
            }
        }

        public bool Add(TKey key, TValue value)
        {
            if (m_map.ContainsKey(key))
            {
                return false;
            }
            m_map.Add(key, value);
            m_list.Add(value);
            return true;
        }

        public bool Remove(TKey key)
        {
            if (m_map.ContainsKey(key))
            {
                TValue v = m_map[key];
                m_list.Remove(v);
                return m_map.Remove(key);
            }
            return false;
        }


        public void Clear()
        {
            m_map.Clear();
            m_list.Clear();
        }

        public int Count
        {
            get { return m_list.Count; }
        }
        public bool ContainsKey(TKey key)
        {
            return m_map.ContainsKey(key);
        }


    }
}

using System;
using System.Collections.Generic;

namespace SGF.Common
{ 
    public class DictionarySafe<TKey, TValue> : Dictionary<TKey, TValue> 
    {
        public new TValue this[TKey key]
        {
            set { base[key] = value; }
            get
            {
                TValue value = default(TValue);
                TryGetValue(key, out value);
                return value;
            }
        }


    }
}

using SGF.SEvent;
using SGF.Timers;
using SGF.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;


namespace SGF.MvvmLite
{
    public class MvvmBinding : IDisposable
    {
        //====================================================================
        #region ////MvvmBinding Async////
        private static Timer m_timerAsync;
        private static List<Binder> m_listAsyncBinders = new List<Binder>();

        private static void PumpAsyncBinder(Binder binder)
        {
            m_listAsyncBinders.Add(binder);
            PumpAsync();
        }
        
        private static void PumpAsync()
        {
            if(m_timerAsync == null)
            {
                m_timerAsync = new Timer();
                m_timerAsync.Interval = 1;
                m_timerAsync.AutoReset = false;
                m_timerAsync.Elapsed += (sender, e) => TickAsync();
            }

            m_timerAsync.Enabled = true;
        }

        private static void TickAsync()
        {
            var binders = m_listAsyncBinders.ToArray();
            m_listAsyncBinders.Clear();

            for (int i = 0; i < binders.Length; ++i)
            {
                binders[i].Validate();
            }
        }


        #endregion
        //====================================================================

        class Binder
        {
            private object target;
            public string src;
            private FieldInfo field;
            private PropertyInfo property;
            public Signal<object> onValidate = new Signal<object>();


            public Binder(object target, string src,  MemberInfo  member)
            {
                this.target = target;
                this.src = src;
                this.field = member as FieldInfo;
                this.property = member as PropertyInfo;
            }

            public void Validate()
            {
                object value = null;
                if (field != null)
                {
                    value = field.GetValue(target);
                }
                else if(property != null)
                {
                    value = property.GetValue(target, null);
                }
                onValidate.InvokeSafe(value);
            }

            public void Clear()
            {
                target = null;
                field = null;
                property = null;
                onValidate.RemoveAllListeners();
            }
            
        }

        //====================================================================
        private bool m_disposed = false;
        private DictionarySafe<string, Binder> m_binders = new DictionarySafe<string, Binder>();

        public MvvmBinding()
        {

        }

        ~MvvmBinding()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (m_disposed) return;
            m_disposed = true;

            foreach (var pair in m_binders)
            {
                pair.Value.Clear();
            }
            m_binders.Clear();
        }
        

        protected void Validate(string src)
        {
            var binder = m_binders[src];
            if (binder != null)
            {
                binder.Validate();
            }
        }

        protected void ValidateAsync(string src)
        {
            var binder = m_binders[src];
            if (binder != null)
            {
                PumpAsyncBinder(binder);
            }
        }


        private MemberInfo GetMemberInfo(string src)
        {
            var members = this.GetType().GetMembers();
            for (int i = 0; i < members.Length; ++i)
            {
                var m = members[i];
                if (m.Name == src)
                {
                    return m;
                }
            }
            return null;
        }

        public void Bind(string src, Action<object> handler)
        {
            var binder = m_binders[src];
            if (binder == null)
            {
                var member = GetMemberInfo(src);
                if(member == null)
                {
                    Debuger.LogError("Bind Error, Member Not Exist: {0}", src);
                    return;
                }

                binder = new Binder(this, src, member);                
                m_binders.Add(src, binder);
            }
            binder.onValidate.AddListener(handler);
        }





        public void UnBind(string src, Action<object> handler)
        {
            var binder = m_binders[src];
            if (binder == null)
            {
                return;
            }
            binder.onValidate.RemoveListener(handler);
        }



    }
}

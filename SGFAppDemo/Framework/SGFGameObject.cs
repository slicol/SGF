using System;
using System.Collections.Generic;

namespace SGFAppDemo.Framework
{
    public class SGFGameObject
    {
        private static List<SGFGameObject> m_listInstancedObject = new List<SGFGameObject>();
        private static List<SGFGameObject> m_listActivedObject = new List<SGFGameObject>();
        
        public static SGFGameObject Instance(Type type)
        {
            
            if (type.IsSubclassOf(typeof(SGFGameObject)))
            {
                SGFGameObject obj = Activator.CreateInstance(type) as SGFGameObject;
                obj.InstanceInternal();
                return obj;
            }
            return null;
        }

        public static void Destroy(SGFGameObject obj)
        {
            obj.DestroyInternal();
        }

        public static SGFGameObject[] GetActivedGameObjects()
        {
            return m_listActivedObject.ToArray();
        }

        public static SGFGameObject[] GetInstancedGameObjects()
        {
            return m_listInstancedObject.ToArray();
        }

        public static void DestroyAll()
        {
            SGFGameObject[] list = m_listInstancedObject.ToArray();
            int cnt = list.Length;
            for (int i = 0; i < cnt; i++)
            {
                list[i].DestroyInternal();
            }
            m_listInstancedObject.Clear();
        }

        

        private bool m_instanced = false;
        private bool m_actived = false;

        
        protected SGFGameObject()
        {
            InstanceInternal();
        }

        private void InstanceInternal()
        {
            if (!m_instanced)
            {
                m_instanced = true;
                m_listInstancedObject.Add(this);
                Instance();
                SetActive(true);
            }
        }

        private void DestroyInternal()
        {
            if (m_instanced)
            {
                if (m_actived)
                {
                    m_actived = false;
                    m_listActivedObject.Remove(this);
                    Deactive();
                }

                m_instanced = false;
                m_listInstancedObject.Remove(this);
                Destroy();
            }
        }

        public virtual void Instance()
        {
            
        }

        public virtual void Destroy()
        {
            DestroyInternal();
        }

        public void SetActive(bool value)
        {
            if (m_actived != value)
            {
                m_actived = value;
                if (m_actived)
                {
                    if (!m_listActivedObject.Contains(this))
                    {
                        m_listActivedObject.Add(this);
                    }
                    Active();
                }
                else
                {
                    m_listActivedObject.Remove(this);
                    Deactive();
                }
            }
        }

        public virtual void Active()
        {

        }

        public virtual void Deactive()
        {

        }

        public virtual void Update(float deltaTime)
        {
            
        }

        public virtual void FixedUpdate()
        {

        }


    }
}
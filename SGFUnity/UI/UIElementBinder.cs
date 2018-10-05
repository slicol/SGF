using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SGF.Unity.UI
{

    public class UIElementAttribute : Attribute
    {
        
    }

    public class UIElementBinder
    {
        public static void BindAllUIElement(MonoBehaviour parent)
        {
            var fis = parent.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            for (int i = 0; i < fis.Length; i++)
            {
                var fi = fis[i];
                var tokens = fi.GetCustomAttributes(typeof(UIElementAttribute), true);
                if (tokens.Length > 0)
                {
                    BindUIElement(parent, fi);
                }
            }
        }

        private static void BindUIElement(MonoBehaviour parent, FieldInfo fi)
        {
            Transform element = parent.transform.Find(fi.Name);
            string uiName = fi.Name;
            if (element == null)
            {
                if (uiName.StartsWith("m_"))
                {
                    uiName = uiName.Substring(2);
                    element = parent.transform.Find(uiName);
                }
                else if (uiName.StartsWith("_"))
                {
                    uiName = uiName.Substring(1);
                    element = parent.transform.Find(uiName);
                }
            }

            if (element == null)
            {
                var c = uiName[0];
                c = Char.IsLower(c) ? Char.ToUpper(c) : Char.ToLower(c);
                uiName = c + uiName.Substring(1);
                element = parent.transform.Find(uiName);
            }


            if (element != null)
            {
                var value = element.GetComponent(fi.FieldType);
                fi.SetValue(parent, value);
            }
            else
            {
                Debuger.LogError("Canot Find UIElement:{0}", fi.Name);
            }
        }

        public static void BindUIElement(MonoBehaviour parent, string uiName)
        {
            var fis = parent.GetType().GetFields();
            for (int i = 0; i < fis.Length; i++)
            {
                var fi = fis[i];
                if (fi.Name == uiName)
                {
                    BindUIElement(parent, fi);    
                }
            }
        }
    }
}
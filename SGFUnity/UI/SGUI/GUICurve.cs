using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SGF.Unity.UI.SGUI
{
    public class GUICurve
    {
        public bool AntiAlias = false;
        public object state = null;

        private List<Vector2> m_source;
        private float m_maxX = float.MinValue;
        private float m_minX = float.MaxValue;
        private float m_maxY = float.MinValue;
        private float m_minY = float.MaxValue;

        public float MinX { get { return m_minX; } }
        public float MinY { get { return m_minY; } }
        public float MaxX { get { return m_maxX; } }
        public float MaxY { get { return m_maxY; } }

        

        public GUICurve(List<Vector2> source)
        {
            m_source = new List<Vector2>();
            if (source != null)
            {
                for (int i = 0; i < source.Count; i++)
                {
                    AddPoint(source[i]);
                }
            }
        }

        public GUICurve()
        {
            m_source = new List<Vector2>();
        }

        public int Length { get { return m_source.Count; } }
        public List<Vector2> Source { get { return m_source; } }

        public void SetRangeY(float min, float max)
        {
            m_minY = min;
            m_maxY = max;
        }

        public void AddPoint(Vector2 point)
        {
            m_source.Add(point);

            if (m_maxX < point.x)
            {
                m_maxX = point.x;
            }

            if (m_maxY < point.y)
            {
                m_maxY = point.y;
            }

            if (m_minX > point.x)
            {
                m_minX = point.x;
            }

            if (m_minY > point.y)
            {
                m_minY = point.y;
            }
        }

        public void AddRangePoint(List<Vector2> listPoints)
        {
            if (listPoints == null || listPoints.Count == 0)
            {
                return;
            }

            for (int i = 0; i < listPoints.Count; i++)
            {
                AddPoint(listPoints[i]);
            }
        }

        public void Clear()
        {
            m_maxX = float.MinValue;
            m_minX = float.MaxValue;
            m_maxY = float.MinValue;
            m_minY = float.MaxValue;
            m_source.Clear();
        }
        //==================================================================================

        public Color lineColor = Color.red;
        public Color pointColor = Color.red;
        public int lineWidth = 2;
        public int pointRadius = 0;
        public int minScope = 0;
        public int maxScope = 0;


        public void Draw(Rect rect, Color color, int width, int minScope = 0, int maxScope = 0)
        {
            Draw(rect, color, width, Color.white, 0, minScope, maxScope);
        }


        public void Draw(Rect rect, Color color, int width, Color pointColor, int pointRadius, int minScope = 0, int maxScope = 0)
        {
            if (m_source == null || m_source.Count < 2)
            {
                return;
            }

            if (minScope < 0 || minScope >= m_source.Count)
            {
                minScope = 0;
            }

            if (maxScope <= 0 || maxScope > m_source.Count)
            {
                maxScope = m_source.Count;
            }

            if (maxScope - minScope < 2)
            {
                return;
            }




            float kx = rect.width / (m_maxX - m_minX);
            float ky = rect.height / (m_maxY - m_minY);

            Vector2 v1 = m_source[minScope];
            v1.x = (v1.x - m_minX) * kx;
            v1.y = rect.height - (v1.y - m_minY) * ky;
            v1.x += rect.x;
            v1.y += rect.y;

            Vector2 v2;

            for (int i = (int)minScope + 1; i < maxScope; i++)
            {
                v2 = m_source[i];
                v2.x = (v2.x - m_minX) * kx;
                v2.y = rect.height - (v2.y - m_minY) * ky;

                v2.x += rect.x;
                v2.y += rect.y;

                Drawing.DrawLine(v1, v2, color, width, false);

                if (pointRadius > 0)
                {
                    Drawing.DrawCircle(v2, pointRadius, pointColor, width, false, 3);
                }

                v1 = v2;
            }
        }


        public void Draw(Rect rect)
        {
            Draw(rect, lineColor, lineWidth, pointColor, pointRadius, minScope, maxScope);
        }

        public void DrawLayout(params GUILayoutOption[] options)
        {
            GUILayout.Box("", options);
            Rect rect = GUILayoutUtility.GetLastRect();
            Draw(rect, lineColor, lineWidth, pointColor, pointRadius, minScope, maxScope);
        }


    }
}

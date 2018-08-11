/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
 * Licensed under the MIT License (the "License"); 
 * you may not use this file except in compliance with the License. 
 * You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, 
 * software distributed under the License is distributed on an "AS IS" BASIS, 
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. 
 * See the License for the specific language governing permissions and limitations under the License.
*/


using System;
using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace SGF.Unity.UI.UILib.Control
{
	public class CtlList:UIControl
	{

		enum Direction
		{
			Horizontal,
			Vertical
		}


		[SerializeField]
		private RectTransform m_itemTemplate;

		//页面的显示大小
		[SerializeField]
		private Vector2 m_pageRect;

		[SerializeField]
		private Direction m_direction = Direction.Horizontal;

		[SerializeField, Range(4, 10)]
		private int m_bufferSize;

		private ScrollRect m_scrollRect;
		private RectTransform m_content;
	    private RectTransform m_viewport;

		private List<RectTransform> m_listItemObject = new List<RectTransform>();
	    private IList m_datas;
		private int m_dataCount = 0;

		//宽高
		public Vector2 ItemViewRect { get { return m_itemTemplate != null ? m_itemTemplate.sizeDelta : new Vector2(100, 100); }}
		public float ItemViewSize { get { return m_direction == Direction.Horizontal ? ItemViewRect.x : ItemViewRect.y; } }

		//行列
		public Vector2 PageRect{get{return m_pageRect;}}
		public int PageSize { get { return m_direction == Direction.Horizontal ? (int)m_pageRect.x : (int)m_pageRect.y; } }


		//Content的位置
		private float m_prevPos = 0;
		public float Position { get { return m_direction == Direction.Horizontal ? m_content.anchoredPosition.x : m_content.anchoredPosition.y; } }

		private int m_currentIndex;//页面的第一行（列）在整个content中的位置

		//页面的实际大小（行列）
		private Vector2 m_pageRealRect = Vector2.zero;
		public Vector2 PageRealRect
		{
			get
			{
				if (m_pageRealRect == Vector2.zero)
				{
					float rows, cols;
					if (m_direction == Direction.Horizontal)
					{
						rows = m_pageRect.x;
						cols = m_pageRect.y + (float)m_bufferSize;
					}
					else
					{
						rows = m_pageRect.x + (float)m_bufferSize;
						cols = m_pageRect.y;
					}
					m_pageRealRect = new Vector2(rows, cols);
				}
				return m_pageRealRect;
			}
		}


		/// 由Data数量获取多少行多少列
		private Vector2 MaxRectWithDatas
		{
			get
			{ 
				int num = m_datas != null ? m_datas.Count:0;
				return m_direction == Direction.Horizontal ?
					new Vector2(m_pageRect.x, Mathf.CeilToInt(num / m_pageRect.x)) :
					new Vector2(Mathf.CeilToInt(num / m_pageRect.y), m_pageRect.y);
			}
		}



		protected override void Awake()
		{
            base.Awake();

			m_scrollRect = GetComponent<ScrollRect >();
			m_scrollRect.horizontal = m_direction == Direction.Horizontal;
			m_scrollRect.vertical = m_direction == Direction.Vertical;

			m_content = m_scrollRect.content;
		    m_viewport = m_scrollRect.viewport;

		}

	    void Start()
	    {
	        if (m_direction == Direction.Vertical)
	        {
	            m_itemTemplate.sizeDelta = new Vector2(m_viewport.rect.width, m_itemTemplate.sizeDelta.y);
	        }
	        m_itemTemplate.gameObject.SetActive(false);
        }

	    public void Clear()
	    {
	        SetData(new List<object>());
	    }


		public override void SetData(object data)
		{
            
			object oldData = m_datas;
			m_datas = data as IList;
			m_dataCount = m_datas.Count;

			int pageItemCount = (int)m_pageRect.x * (int)m_pageRect.y;


			if (m_datas.Count > pageItemCount)
			{
				SetContentBound(MaxRectWithDatas);
			}
			else
			{
				SetContentBound(m_pageRect);
			}


			int pageRealItemCount = (int)PageRealRect.x * (int)PageRealRect.y;

			if (m_datas.Count > pageRealItemCount)
			{
				while (m_listItemObject.Count < pageRealItemCount)
				{
					CreateItem(m_listItemObject.Count);
				}
			}
			else
			{
				while (m_listItemObject.Count > m_datas.Count)
				{
					RemoveItem(m_listItemObject.Count - 1);
				}

				while (m_listItemObject.Count < m_datas.Count)
				{
					CreateItem(m_listItemObject.Count);
				}
			}

			if (oldData != data)
			{
				ValidateItems ();
			}

		}



		private void CreateItem(int index)
		{
			RectTransform item = Instantiate(m_itemTemplate);
			item.SetParent(m_content.transform, false);
			item.anchorMax = Vector2.up;
			item.anchorMin = Vector2.up;
			item.pivot = Vector2.up;
			item.name = "item" + index;

			item.anchoredPosition = m_direction == Direction.Horizontal ?
				new Vector2(Mathf.Floor(index / PageRealRect.x) * ItemViewRect.x, -(index % PageRealRect.x) * ItemViewRect.y) :
				new Vector2((index % PageRealRect.y) * ItemViewRect.x, -Mathf.Floor(index / PageRealRect.y) * ItemViewRect.y);
			
			
			m_listItemObject.Add(item);
			item.gameObject.SetActive(true);

			UpdateItem(index, item.gameObject);
		}

		private void RemoveItem(int index)
		{
			RectTransform item = m_listItemObject[index];
			m_listItemObject.RemoveAt(index);
			Destroy(item.gameObject);
		}



		/// 设置content的大小
		private void SetContentBound(Vector2 bound)
		{
			m_content.sizeDelta = new Vector2(bound.y * ItemViewRect.x, bound.x * ItemViewRect.y);
		}

		public float MaxPrevPos
		{
			get
			{
				float result;
				Vector2 max = MaxRectWithDatas;
				if (m_direction == Direction.Horizontal)
				{
					result = max.y - m_pageRect.y;
				}
				else
				{
					result = max.x - m_pageRect.x;
				}
				return result * ItemViewSize;
			}
		}



		private void ValidateItems()
		{
			for (int i = 0; i < m_listItemObject.Count; ++i) 
			{
				UpdateItem (i, m_listItemObject [i].gameObject);

			}
		}

		void Update()
		{
		    if (m_datas == null)
		    {
		        return;
		    }

			if (m_datas != null && m_dataCount != m_datas.Count)
			{
				SetData(m_datas);
			}


			float dirAdjust = m_direction == Direction.Horizontal ? 1f : -1f;

			while (dirAdjust * Position - m_prevPos < -ItemViewSize * 2)
			{
				if (m_prevPos <= -MaxPrevPos) return;

				m_prevPos -= ItemViewSize;

				List<RectTransform> range = m_listItemObject.GetRange(0, PageSize);
				m_listItemObject.RemoveRange(0, PageSize);
				m_listItemObject.AddRange(range);
				for (int i = 0; i < range.Count; i++)
				{
					MoveItemToIndex(m_currentIndex * PageSize + m_listItemObject.Count + i, range[i]);
				}
				m_currentIndex++;
			}

			while (dirAdjust * Position - m_prevPos > -ItemViewSize)
			{
				if (Mathf.RoundToInt(m_prevPos) >= 0) return;

				m_prevPos += ItemViewSize;

				m_currentIndex--;

				if (m_currentIndex < 0) return;

				List<RectTransform> range = m_listItemObject.GetRange(m_listItemObject.Count - PageSize, PageSize);
				m_listItemObject.RemoveRange(m_listItemObject.Count - PageSize, PageSize);
				m_listItemObject.InsertRange(0, range);
				for (int i = 0; i < range.Count; i++)
				{
					MoveItemToIndex(m_currentIndex * PageSize + i, range[i]);
				}
			}
		}

		private void MoveItemToIndex(int index, RectTransform item)
		{
			item.anchoredPosition = GetItemPosByIndex(index);

		    UpdateItem(index, item.gameObject);
		}

		private Vector2 GetItemPosByIndex(int index)
		{
			float x, y;
			if (m_direction == Direction.Horizontal)
			{
				x = index % m_pageRect.x;
				y = Mathf.FloorToInt(index / m_pageRect.x);
			}
			else
			{
				x = Mathf.FloorToInt(index / m_pageRect.y);
				y = index % m_pageRect.y;
			}

			return new Vector2(y * ItemViewRect.x, -x * ItemViewRect.y);
		}

		private void UpdateItem(int index, GameObject item)
		{
			item.SetActive(index < m_datas.Count);

			if (item.activeSelf)
			{
				CtlListItem itemComp = item.GetComponent < CtlListItem > ();
				if (itemComp != null)
				{
					itemComp.UpdateItem(index, m_datas[index]);
				}
			}
		}


	}
}

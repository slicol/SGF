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



using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SGF.Unity.UI.Drag
{
    public class Draggable:MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public static GameObject HolderPrefab;

        private DropZone m_zone;
        private DropZone m_holderZone;
        private GameObject m_holder;
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (HolderPrefab == null)
            {
                m_holder = new GameObject(this.name + "(Holder)");
            }
            else
            {
                m_holder = GameObject.Instantiate(HolderPrefab);
                m_holder.name = this.name + "(Holder)";
            }
            m_holder.transform.SetParent(this.transform.parent);
            m_holder.transform.SetSiblingIndex(this.transform.GetSiblingIndex());

            if (this.GetComponent<LayoutElement>() != null)
            {
                LayoutElement le = m_holder.AddComponent<LayoutElement>();
                le.preferredWidth = this.GetComponent<LayoutElement>().preferredWidth;
                le.preferredHeight = this.GetComponent<LayoutElement>().preferredHeight;
                le.flexibleWidth = 0;
                le.flexibleHeight = 0;
            }

            m_zone = this.transform.parent.GetComponent<DropZone>();
            m_holderZone = m_zone;

            //如果开始拖动，将它移出公共区域
            this.transform.SetParent(this.transform.parent.parent);
            SetBlocksRaycasts(false);
        }

        public void OnDrag(PointerEventData eventData)
        {
            this.transform.position = eventData.position;

            //在 拖动过程中，可以会经过一些Zone
            if (m_holderZone != null)
            {
                if (m_holder.transform.parent != m_holderZone)
                {
                    m_holder.transform.SetParent(m_holderZone.transform);
                }

                int newSiblingIndex = m_holderZone.transform.childCount;

                for (int i = 0; i < m_holderZone.transform.childCount; i++)
                {
                    if (this.transform.position.x < m_holderZone.transform.GetChild(i).position.x)
                    {
                        newSiblingIndex = i;

                        if (m_holder.transform.GetSiblingIndex() < newSiblingIndex)
                            newSiblingIndex--;

                        break;
                    }
                }


                m_holder.transform.SetSiblingIndex(newSiblingIndex);
            }
        }


        public virtual void OnEndDrag(PointerEventData eventData)
        {
            this.transform.SetParent(m_zone.transform);
            this.transform.SetSiblingIndex(m_holder.transform.GetSiblingIndex());
            SetBlocksRaycasts(true);
            Destroy(m_holder);
        }


        public void EnterZone(DropZone zone)
        {
            m_holderZone = zone;
        }

        public void ExitZone(DropZone zone)
        {
            if (m_holderZone == zone)
            {
                m_holderZone = m_zone;
            }
        }

        public void DropZone(DropZone zone)
        {
            m_zone.OnDropOut(this);
            m_zone = zone;
            m_zone.OnDropIn(this);
        }

        private void SetBlocksRaycasts(bool value)
        {
            var group = this.GetComponent<CanvasGroup>();
            if (group != null)
            {
                group.blocksRaycasts = value;
            }
        }
    }
}
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

namespace SGF.Unity.UI.Drag
{
    public class DropZone:MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public virtual void OnDrop(PointerEventData eventData)
        {
            Draggable d = eventData.pointerDrag.GetComponent<Draggable>();
            if (d != null)
            {
                d.DropZone(this);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null)
                return;

            Draggable d = eventData.pointerDrag.GetComponent<Draggable>();
            if (d != null)
            {
                d.EnterZone(this);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null)
                return;

            Draggable d = eventData.pointerDrag.GetComponent<Draggable>();
            if (d != null)
            {
                d.ExitZone(this);
            }
        }

        public virtual void OnDropIn(Draggable item)
        {
        }

        public virtual void OnDropOut(Draggable item)
        {
        }
    }
}
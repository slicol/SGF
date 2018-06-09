using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SGF.Unity.UI.Drag
{
    public class DropZone:MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public void OnDrop(PointerEventData eventData)
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
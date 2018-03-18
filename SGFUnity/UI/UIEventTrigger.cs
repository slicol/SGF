using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SGF.Unity.UI
{
    public class UIEventTrigger:EventTrigger
    {
        public Action onClick;
        public Action<GameObject> onClickWithObject;
        public Action<string> onClickWithName;
        public Action<PointerEventData> onClickWithEvent;
        public Action<PointerEventData> onDown;
        public Action<PointerEventData> onEnter;
        public Action<PointerEventData> onExit;
        public Action<PointerEventData> onUp;
        public Action<PointerEventData> onBeginDrag;
        public Action<PointerEventData> onDrag;
        public Action<PointerEventData> onEndDrag;
        public Action<BaseEventData> onSelect;
        public Action<BaseEventData> onUpdateSelect;



        static public UIEventTrigger Get(GameObject go)
        {
            UIEventTrigger listener = go.GetComponent<UIEventTrigger>();
            if (listener == null) listener = go.AddComponent<UIEventTrigger>();
            return listener;
        }

        static public UIEventTrigger Get(UIBehaviour control)
        {
            UIEventTrigger listener = control.gameObject.GetComponent<UIEventTrigger>();
            if (listener == null) listener = control.gameObject.AddComponent<UIEventTrigger>();
            return listener;
        }

        static public UIEventTrigger Get(Transform transform)
        {
            UIEventTrigger listener = transform.gameObject.GetComponent<UIEventTrigger>();
            if (listener == null) listener = transform.gameObject.AddComponent<UIEventTrigger>();
            return listener;
        }

        static public bool HasExistOn(Transform transform)
        {
            return transform.gameObject.GetComponent<UIEventTrigger>() != null;
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (onClickWithObject != null) onClickWithObject(gameObject);
            if (onClick != null) onClick();
            if (onClickWithEvent != null) onClickWithEvent(eventData);
            if (onClickWithName != null) onClickWithName(gameObject.name);
        }
        public override void OnPointerDown(PointerEventData eventData)
        {
            if (onDown != null) onDown(eventData);
        }
        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (onEnter != null) onEnter(eventData);
        }
        public override void OnPointerExit(PointerEventData eventData)
        {
            if (onExit != null) onExit(eventData);
        }
        public override void OnPointerUp(PointerEventData eventData)
        {
            if (onUp != null) onUp(eventData);
        }
        public override void OnSelect(BaseEventData eventData)
        {
            if (onSelect != null) onSelect(eventData);
        }
        public override void OnUpdateSelected(BaseEventData eventData)
        {
            if (onUpdateSelect != null) onUpdateSelect(eventData);
        }
        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (onBeginDrag != null) { onBeginDrag(eventData); }
        }
        public override void OnDrag(PointerEventData eventData)
        {
            if (onDrag != null) { onDrag(eventData); }
        }
        public override void OnEndDrag(PointerEventData eventData)
        {
            if (onEndDrag != null) { onEndDrag(eventData); }
        }




    }
}
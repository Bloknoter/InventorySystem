using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


namespace InventoryEngine.GUI
{
    public class DragNDropUIElement : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public delegate void ElementListener(DragNDropUIElement element, PointerEventData eventData);

        public event ElementListener BeginDrag;
        public event ElementListener EndDrag;

        public event ElementListener Dragging;
        public event ElementListener Dropped;

        public event ElementListener PointerEnter;
        public event ElementListener PointerExit;

        [SerializeField]
        private DNDElementVisualStateControllerBase m_visualization;

        public DNDElementVisualStateControllerBase Visualization => m_visualization;

        public void OnBeginDrag(PointerEventData eventData)
        {
            BeginDrag?.Invoke(this, eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            EndDrag?.Invoke(this, eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Dragging?.Invoke(this, eventData);
        }

        public void OnDrop(PointerEventData eventData)
        {
            Dropped?.Invoke(this, eventData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            PointerEnter?.Invoke(this, eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            PointerExit?.Invoke(this, eventData);
        }
    }
}

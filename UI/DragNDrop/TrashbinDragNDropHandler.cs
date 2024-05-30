using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using InventoryEngine.Transitions;


namespace InventoryEngine.GUI
{
    public class TrashbinDragNDropHandler : MonoBehaviour
    {
        [SerializeField]
        private DragNDropUIElement m_dropArea;

        [SerializeField]
        private bool m_throwAwayInstead;

        private ItemsTrashbinBase m_trashbin;

        private DragNDropSystem m_dragNDropSystem;
        private uint m_myUnitId;

        public void Initialize(ItemsTrashbinBase itemsTrashbin)
        {
            m_trashbin = itemsTrashbin;
        }

        public void SetupDragNDropSsytem(DragNDropSystem dragNDropSystem)
        {
            m_dragNDropSystem = dragNDropSystem;

            m_myUnitId = m_dragNDropSystem.AddUnit(new TrashbinTransitionUnit(m_trashbin, m_throwAwayInstead));

            m_dragNDropSystem.Finished += OnDragNDropFinished;
            StartListeningElement(m_dropArea);
        }

        private void StartListeningElement(DragNDropUIElement element)
        {
            element.PointerEnter += OnPointerEnterElement;
            element.PointerExit += OnPointerExitElement;
        }

        private void StopListeningElement(DragNDropUIElement element)
        {
            element.PointerEnter -= OnPointerEnterElement;
            element.PointerExit -= OnPointerExitElement;
        }

        private void OnPointerEnterElement(DragNDropUIElement element, PointerEventData eventData)
        {
            if (!m_dragNDropSystem.IsProcessActive)
                return;

            m_dragNDropSystem.UpdateDestination(m_myUnitId, null);

            m_dropArea.Visualization.ShowDragNDropState(true, null, m_dragNDropSystem.DraggingData);
        }

        private void OnPointerExitElement(DragNDropUIElement element, PointerEventData eventData)
        {
            if (!m_dragNDropSystem.IsProcessActive)
                return;

            element.Visualization.HideDragNDropState();
            m_dragNDropSystem.UpdateDestinationAsNull();
        }

        private void OnDragNDropFinished(bool succeed)
        {
            m_dropArea.Visualization.HideDragNDropState();
        }

        public void Clear()
        {
            StopListeningElement(m_dropArea);

            m_dragNDropSystem.Finished -= OnDragNDropFinished;
            m_dragNDropSystem.RemoveUnit(m_myUnitId);
        }
    }
}

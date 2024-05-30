using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using InventoryEngine;
using InventoryEngine.Transitions;

namespace InventoryEngine.GUI
{
    public class SlotInventoryDragNDropHandler : MonoBehaviour, ISlotInvAdditionalContent
    {
        private DragNDropSystem m_dragNDropSystem;
        private uint m_myUnitId;

        private ISlotInventory m_inventory;
        private SlotInventoryDisplay m_inventoryDisplay;

        private List<DragNDropUIElement> m_slots = new List<DragNDropUIElement>();
        private DragNDropUIElement m_draggableElement;

        public void Initialize(SlotInventoryDisplay inventoryDisplay, ISlotInventory inventory)
        {
            m_inventoryDisplay = inventoryDisplay;
            m_inventory = inventory;
        }

        public void SetupDragNDropSystem(DragNDropSystem dragndropSystem)
        {
            m_dragNDropSystem = dragndropSystem;
            m_myUnitId = m_dragNDropSystem.AddUnit(new SlotTransitionInventoryUnit(m_inventory));

            m_dragNDropSystem.Finished += OnDragNDropFinished;
        }

        public void OnSlotAdded(ItemContainerDisplay containerDisplay)
        {
            var uiElement = containerDisplay.GetComponent<DragNDropUIElement>();
            if (uiElement == null)
            {
                Debug.LogError($"Cannot perform drag n drop operations on container display: no required component found", containerDisplay);
                return;
            }

            StartListeningElement(uiElement);
            m_slots.Add(uiElement);
        }

        public void OnSlotRemoved(ItemContainerDisplay containerDisplay)
        {
            var uiElement = containerDisplay.GetComponent<DragNDropUIElement>();
            if (uiElement == null)
                return;

            StopListeningElement(uiElement);
            m_slots.Remove(uiElement);

            if (m_draggableElement == uiElement && m_dragNDropSystem.IsProcessActive)
                m_dragNDropSystem.CancelDragNDrop();
        }

        private void StartListeningElement(DragNDropUIElement element)
        {
            element.PointerEnter += OnPointerEnterElement;
            element.PointerExit += OnPointerExitElement;

            element.BeginDrag += OnBeginDragElement;
            element.Dragging += OnDraggingElement;
            element.EndDrag += OnEndDragElement;
        }

        private void StopListeningElement(DragNDropUIElement element)
        {
            element.PointerEnter -= OnPointerEnterElement;
            element.PointerExit -= OnPointerExitElement;

            element.BeginDrag -= OnBeginDragElement;
            element.Dragging -= OnDraggingElement;
            element.EndDrag -= OnEndDragElement;
        }

        private void OnPointerEnterElement(DragNDropUIElement element, PointerEventData eventData)
        {
            if (!m_dragNDropSystem.IsProcessActive)
                return;

            var itemContainerDisplay = element.GetComponent<ItemContainerDisplay>();

            var locationData = new SlotItemLocationData();
            locationData.SlotIndex = FindSlotIndex(itemContainerDisplay.ItemContainer);
            m_dragNDropSystem.UpdateDestination(m_myUnitId, locationData);

            bool canDrop = m_dragNDropSystem.State == DragNDropSystem.OperationState.PossibleToDrop || m_dragNDropSystem.State == DragNDropSystem.OperationState.PossibleToSwipe;
            element.Visualization.ShowDragNDropState(canDrop, locationData, m_dragNDropSystem.DraggingData);
        }

        private void OnPointerExitElement(DragNDropUIElement element, PointerEventData eventData)
        {
            if (!m_dragNDropSystem.IsProcessActive)
                return;

            element.Visualization.HideDragNDropState();
            m_dragNDropSystem.UpdateDestinationAsNull();
        }

        private void OnBeginDragElement(DragNDropUIElement element, PointerEventData eventData)
        {
            var itemContainerDisplay = element.GetComponent<ItemContainerDisplay>();
            if (itemContainerDisplay.ItemContainer.Item == null)
                return;

            m_draggableElement = element;
            m_draggableElement.Visualization.HideContent();

            var locationData = new SlotItemLocationData();
            locationData.SlotIndex = FindSlotIndex(itemContainerDisplay.ItemContainer);

            m_dragNDropSystem.StartDragNDrop(m_myUnitId, locationData);
            m_dragNDropSystem.DraggingData.VisualSize = itemContainerDisplay.VisualItemSize;
        }

        private void OnDraggingElement(DragNDropUIElement element, PointerEventData eventData)
        {
            if (!m_dragNDropSystem.IsProcessActive)
                return;

            m_dragNDropSystem.DraggingData.VisualPosition = eventData.position + m_dragNDropSystem.DraggingData.VisualSize / 2;
        }

        private void OnEndDragElement(DragNDropUIElement element, PointerEventData eventData)
        {
            if (!m_dragNDropSystem.IsProcessActive)
                return;

            m_dragNDropSystem.PerformDragNDrop();
        }

        private int FindSlotIndex(IItemContainer itemContainer)
        {
            for (int i = 0; i < m_inventory.Size; ++i)
            {
                if(m_inventory.SlotAt(i) == itemContainer)
                    return i;
            }
            return -1;
        }

        private void OnDragNDropFinished(bool succeed)
        {
            for (int i = 0; i < m_slots.Count; ++i)
            {
                m_slots[i].Visualization.HideDragNDropState();
                m_slots[i].Visualization.ShowContent();
            }

            m_draggableElement = null;
        }

        public void Clear()
        {
            m_dragNDropSystem.Finished -= OnDragNDropFinished;

            m_dragNDropSystem.RemoveUnit(m_myUnitId);
        }
    }
}

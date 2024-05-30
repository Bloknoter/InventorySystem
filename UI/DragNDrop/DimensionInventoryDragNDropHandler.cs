//#define DEBUGGING_EVENTS

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using InventoryEngine.Transitions;

namespace InventoryEngine.GUI
{
    public class DimensionInventoryDragNDropHandler : MonoBehaviour, IDimensionInvAdditionalContent
    {
        [SerializeField]
        private DragNDropUIElement m_contentArea;

        [SerializeField]
        private RectTransform m_contentAreaTransform;

        private DragNDropSystem m_dragNDropSystem;
        private uint m_myUnitId;

        private DimensionInventoryDisplay m_inventoryDisplay;
        private IDimensionInventory m_inventory;
        private List<DragNDropUIElement> m_items = new List<DragNDropUIElement>();

        private DragNDropUIElement m_draggableElement;

        private Vector3 m_worldCornerPos;
        private Vector2Int m_currentInventoryPointerPos = new Vector2Int(-1, -1);

        public void Initialize(DimensionInventoryDisplay inventoryDisplay, IDimensionInventory inventory)
        {
            m_inventoryDisplay = inventoryDisplay;
            m_inventory = inventory;

            var deltaFromCenter = new Vector3(m_contentAreaTransform.rect.width / 2 * -m_inventoryDisplay.HorItemsAxisDirection,
                m_contentAreaTransform.rect.height / 2 * -m_inventoryDisplay.VerItemsAxisDirection, 0);
            m_worldCornerPos = m_contentAreaTransform.position + deltaFromCenter;
        }

        public void SetupDragNDropSystem(DragNDropSystem dragndropSystem)
        {
            m_dragNDropSystem = dragndropSystem;
            m_myUnitId = m_dragNDropSystem.AddUnit(new DimensionTransitionInventoryUnit(m_inventory));

            m_dragNDropSystem.PrePerform += OnPreDragNDropPerform;
            m_dragNDropSystem.Finished += OnDragNDropFinished;
            StartListeningElement(m_contentArea);
        }

        public void OnItemContainerAdded(ItemContainerDisplay containerDisplay)
        {
            var uiElement = containerDisplay.GetComponent<DragNDropUIElement>();
            if(uiElement == null)
            {
                Debug.LogError($"Cannot perform drag n drop operations on container display: no required component found", containerDisplay);
                return;
            }

            StartListeningElement(uiElement);
            m_items.Add(uiElement);
        }

        public void OnItemContainerRemoved(ItemContainerDisplay containerDisplay)
        {
            var uiElement = containerDisplay.GetComponent<DragNDropUIElement>();
            if (uiElement == null)
                return;

            StopListeningElement(uiElement);
            m_items.Remove(uiElement);

            if(m_draggableElement == uiElement && m_dragNDropSystem.IsProcessActive)
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

        private void UpdateTick(float delta)
        {
            var inventoryPos = CalculateInventoryPos(Input.mousePosition);
            if (m_currentInventoryPointerPos != inventoryPos)
            {
                m_currentInventoryPointerPos = inventoryPos;
                var locationData = new DimensionItemLocationData();
                locationData.Position = CalculateInventoryPos(Input.mousePosition);
                m_dragNDropSystem.UpdateDestination(m_myUnitId, locationData);

                bool canDrop = m_dragNDropSystem.State == DragNDropSystem.OperationState.PossibleToDrop || m_dragNDropSystem.State == DragNDropSystem.OperationState.PossibleToSwipe;
                m_contentArea.Visualization.ShowDragNDropState(canDrop, locationData, m_dragNDropSystem.DraggingData);
            }
        }

        private void OnPointerEnterElement(DragNDropUIElement element, PointerEventData eventData)
        {
            if (!m_dragNDropSystem.IsProcessActive)
                return;

#if UNITY_EDITOR && DEBUGGING_EVENTS
            Debug.Log($"Element enter: {element.gameObject.name}", element.gameObject);
#endif

            GlobalScene.Instance.TickManager.AddObjectToTick(UpdateTick, TickManager.TickTypes.Update);

            var locationData = new DimensionItemLocationData();
            locationData.Position = CalculateInventoryPos(Input.mousePosition);
            m_dragNDropSystem.UpdateDestination(m_myUnitId, locationData);

            bool canDrop = m_dragNDropSystem.State == DragNDropSystem.OperationState.PossibleToDrop || m_dragNDropSystem.State == DragNDropSystem.OperationState.PossibleToSwipe;
            m_contentArea.Visualization.ShowDragNDropState(canDrop, locationData, m_dragNDropSystem.DraggingData);
        }

        private void OnPointerExitElement(DragNDropUIElement element, PointerEventData eventData)
        {
            if (!m_dragNDropSystem.IsProcessActive)
                return;

#if UNITY_EDITOR && DEBUGGING_EVENTS
            Debug.Log($"Element exit: {element.gameObject.name}", element.gameObject);
#endif

            GlobalScene.Instance.TickManager.RemoveObjectToTick(UpdateTick);

            element.Visualization.HideDragNDropState();
            m_dragNDropSystem.UpdateDestinationAsNull();
        }

        private void OnBeginDragElement(DragNDropUIElement element, PointerEventData eventData)
        {
            if (element == m_contentArea)
                return;

#if UNITY_EDITOR && DEBUGGING_EVENTS
            Debug.Log($"Element begin drag: {element.gameObject.name}", element.gameObject);
#endif

            m_draggableElement = element;
            m_draggableElement.Visualization.HideContent();

            GlobalScene.Instance.TickManager.AddObjectToTick(UpdateTick, TickManager.TickTypes.Update);

            var locationData = new DimensionItemLocationData();
            locationData.Position = CalculateInventoryPos(eventData.position);

            var containerDisplay = element.gameObject.GetComponent<ItemContainerDisplay>();

            m_dragNDropSystem.StartDragNDrop(m_myUnitId, locationData);
            m_dragNDropSystem.DraggingData.VisualSize = containerDisplay.VisualItemSize;
        }

        private void OnDraggingElement(DragNDropUIElement element, PointerEventData eventData)
        {
            if (!m_dragNDropSystem.IsProcessActive)
                return;

#if UNITY_EDITOR && DEBUGGING_EVENTS
            Debug.Log($"Element end drag: {element.gameObject.name}", element.gameObject);
#endif

            m_dragNDropSystem.DraggingData.VisualPosition = eventData.position + m_dragNDropSystem.DraggingData.VisualSize / 2;
        }

        private void OnEndDragElement(DragNDropUIElement element, PointerEventData eventData)
        {
            if (!m_dragNDropSystem.IsProcessActive)
                return;

#if UNITY_EDITOR && DEBUGGING_EVENTS
            Debug.Log($"Element end drag: {element.gameObject.name}", element.gameObject);
#endif

            m_dragNDropSystem.PerformDragNDrop();
        }

        private void OnPreDragNDropPerform()
        {
            if (m_draggableElement != null)
            {
                m_draggableElement.Visualization.ShowContent();
                m_draggableElement = null;
            }
        }

        private void OnDragNDropFinished(bool succeed)
        {
            GlobalScene.Instance.TickManager.RemoveObjectToTick(UpdateTick);

            m_contentArea.Visualization.HideDragNDropState();

            for(int i = 0; i < m_items.Count; ++i)
            {
                m_items[i].Visualization.HideDragNDropState();
                m_items[i].Visualization.ShowContent();
            }

            m_draggableElement = null;
        }

        private Vector2Int CalculateInventoryPos(Vector3 pointerPosition)
        {
            var localPointerPos = Vector2.zero;
            localPointerPos.x = (pointerPosition.x - m_worldCornerPos.x) * m_inventoryDisplay.HorItemsAxisDirection;
            localPointerPos.y = (pointerPosition.y - m_worldCornerPos.y) * m_inventoryDisplay.VerItemsAxisDirection;

            int inventoryPosX = (int)(localPointerPos.x / m_inventoryDisplay.CellSize.x);
            int inventoryPosY = (int)(localPointerPos.y / m_inventoryDisplay.CellSize.y);

            return new Vector2Int(inventoryPosX, inventoryPosY);
        }

        public void Clear()
        {
            StopListeningElement(m_contentArea);

            for (int i = 0; i < m_items.Count; ++i)
                StopListeningElement(m_items[i]);

            m_items.Clear();

            m_currentInventoryPointerPos = new Vector2Int(-1, -1);

            m_dragNDropSystem.PrePerform -= OnPreDragNDropPerform;
            m_dragNDropSystem.Finished -= OnDragNDropFinished;
            m_dragNDropSystem.RemoveUnit(m_myUnitId);
        }
    }
}

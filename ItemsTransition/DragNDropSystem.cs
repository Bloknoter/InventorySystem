using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InventoryEngine.Transitions
{
    public class DragNDropSystem
    {
        public enum OperationState
        {
            WaitingForStart,
            NotPossible,
            PossibleToDrop,
            PossibleToSwipe
        }

        public delegate void DragNDropSystemListener();
        public delegate void DragNDropSystemResultListener(bool isSucceed);

        public event DragNDropSystemListener Started;
        public event DragNDropSystemListener DestinationUpdated;
        public event DragNDropSystemListener PrePerform;
        public event DragNDropSystemResultListener Finished;

        public class UnitData
        {
            public readonly uint UnitId;
            public readonly ItemTransitionUnitBase Unit;
            public readonly object ItemLocationData;

            public UnitData(uint unitId, ItemTransitionUnitBase unit, object itemLocationData)
            {
                UnitId = unitId;
                Unit = unit;
                ItemLocationData = itemLocationData;
            }
        }

        private Dictionary<uint, ItemTransitionUnitBase> m_units = new Dictionary<uint, ItemTransitionUnitBase>();

        private uint m_newUnitId = 0;

        private ItemToTransitData m_draggingData;
        private OperationState m_operationState = OperationState.WaitingForStart;

        private UnitData m_source;
        private UnitData m_destination;

        public ItemToTransitData DraggingData => m_draggingData;
        public bool IsProcessActive => m_draggingData != null;
        public OperationState State => m_operationState;

        public UnitData Source => m_source;
        public UnitData Destination => m_destination;

        public uint AddUnit(ItemTransitionUnitBase unit)
        {
            m_units.Add(m_newUnitId, unit);
            m_newUnitId++;
            return m_newUnitId - 1;
        }

        public void RemoveUnit(uint unitId) 
        {
            if(!m_units.ContainsKey(unitId))
                return;

            m_units.Remove(unitId);
        }

        public void StartDragNDrop(uint unitId, object itemLocationData)
        {
            if (IsProcessActive)
            {
                Debug.LogError($"Starting process when already started");
                return;
            }

            if (!m_units.ContainsKey(unitId))
            {
                Debug.LogError($"Can't find unit {unitId}. DragNDrop start failed");
                return;
            }

            m_source = new UnitData(unitId, m_units[unitId], itemLocationData);
            m_draggingData = m_source.Unit.GetItemToTransitData(m_source.ItemLocationData);

            Started?.Invoke();
            DestinationUpdated?.Invoke();
        }

        public void UpdateDestinationAsNull()
        {
            if (!IsProcessActive)
            {
                Debug.LogError($"Trying to update destination when process is not started");
                return;
            }

            m_destination = null;
            m_operationState = OperationState.NotPossible;
            DestinationUpdated?.Invoke();
        }

        public void UpdateDestination(uint unitId, object itemLocationData)
        {
            if (!IsProcessActive)
            {
                Debug.LogError($"Trying to update destination when process is not started");
                return;
            }

            if (!m_units.ContainsKey(unitId))
            {
                Debug.LogError($"Can't find unit {unitId}. DragNDrop update failed");
                return;
            }

            m_destination = new UnitData(unitId, m_units[unitId], itemLocationData);

            if (m_source.UnitId == m_destination.UnitId)
            {
                if (m_destination.Unit.CanDragNDropLocaly(m_draggingData, m_source.ItemLocationData, m_destination.ItemLocationData))
                    m_operationState = OperationState.PossibleToDrop;
                else
                    m_operationState = OperationState.NotPossible;
            }
            else
            {

                var transitionType = m_destination.Unit.PredictTransitionType(m_draggingData, m_destination.ItemLocationData);

                if (transitionType == TransitionType.Drop)
                {
                    if (m_destination.Unit.CanDrop(m_draggingData, m_destination.ItemLocationData))
                        m_operationState = OperationState.PossibleToDrop;
                    else
                        m_operationState = OperationState.NotPossible;
                }
                else if (transitionType == TransitionType.Swipe)
                {
                    var destinationItemData = m_destination.Unit.GetItemToTransitData(m_destination.ItemLocationData);
                    if (m_source.Unit.CanSwipe(destinationItemData, m_source.ItemLocationData) && m_destination.Unit.CanSwipe(m_draggingData, m_destination.ItemLocationData))
                        m_operationState = OperationState.PossibleToSwipe;
                    else
                        m_operationState = OperationState.NotPossible;
                }
            }

            DestinationUpdated?.Invoke();
        }

        public void CancelDragNDrop()
        {
            ClearDraggingProcess();
            Finished?.Invoke(false);
        }

        public void PerformDragNDrop()
        {
            if (!IsProcessActive)
            {
                Debug.LogError($"Trying to perform drag & drop when process is not started");
                return;
            }

            bool result = false;

            if (m_destination == null)
            {
                ClearDraggingProcess();
                Finished?.Invoke(result);
                return;
            }

            PrePerform?.Invoke();

            if (m_source.UnitId == m_destination.UnitId)
            {
                if (m_destination.Unit.CanDragNDropLocaly(m_draggingData, m_source.ItemLocationData, m_destination.ItemLocationData))
                {
                    m_destination.Unit.DragNDropLocaly(m_draggingData, m_source.ItemLocationData, m_destination.ItemLocationData);
                    result = true;
                }

                ClearDraggingProcess();
                Finished?.Invoke(result);
                return;
            }

            var transitionType = m_destination.Unit.PredictTransitionType(m_draggingData, m_destination.ItemLocationData);

            if(transitionType == TransitionType.Drop)
            {
                if(m_destination.Unit.CanDrop(m_draggingData, m_destination.ItemLocationData))
                {
                    var draggingItemData = m_source.Unit.GetItemToTransitData(m_source.ItemLocationData);
                    var left = m_destination.Unit.Drop(m_draggingData, m_destination.ItemLocationData);
                    m_source.Unit.RemoveDragged(draggingItemData.Amount - left, m_source.ItemLocationData, true);

                    result = true;
                }
            }
            else if(transitionType == TransitionType.Swipe)
            {
                var destinationItemData = m_destination.Unit.GetItemToTransitData(m_destination.ItemLocationData);
                if (m_source.Unit.CanSwipe(destinationItemData, m_source.ItemLocationData) && m_destination.Unit.CanSwipe(m_draggingData, m_destination.ItemLocationData))
                {
                    var sourceItemData = m_source.Unit.GetItemToTransitData(m_source.ItemLocationData);
                    m_source.Unit.RemoveDragged(sourceItemData.Amount, m_source.ItemLocationData, false);

                    m_destination.Unit.RemoveDragged(destinationItemData.Amount, m_destination.ItemLocationData, false);

                    m_source.Unit.Drop(destinationItemData, m_source.ItemLocationData);
                    m_destination.Unit.Drop(sourceItemData, m_destination.ItemLocationData);

                    result = true;
                }
            }

            ClearDraggingProcess();
            Finished?.Invoke(result);
        }

        private void ClearDraggingProcess()
        {
            m_draggingData = null;
            m_source = null;
            m_destination = null;

            m_operationState = OperationState.WaitingForStart;
        }

        public void Clear()
        {
            if (IsProcessActive)
            {
                ClearDraggingProcess();
                Finished?.Invoke(false);
            }

            m_units.Clear();
            m_newUnitId = 0;
        }
    }
}

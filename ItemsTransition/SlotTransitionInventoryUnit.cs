using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InventoryEngine.Transitions
{
    public class SlotTransitionInventoryUnit : ItemTransitionUnitBase
    {
        private ISlotInventory m_inventory;

        public SlotTransitionInventoryUnit(ISlotInventory inventory)
        {
            m_inventory = inventory;
        }

        public override ItemToTransitData GetItemToTransitData(object itemLocationData)
        {
            if (itemLocationData is SlotItemLocationData locationData)
            {
                var slot = m_inventory.SlotAt(locationData.SlotIndex);
                return new ItemToTransitData(slot.Item, slot.Amount);
            }

            Debug.LogError($"Invalid item location data format");
            return null;
        }

        public override TransitionType PredictTransitionType(ItemToTransitData transitionData, object itemLocationData)
        {
            if (itemLocationData is SlotItemLocationData locationData)
            {
                var slot = m_inventory.SlotAt(locationData.SlotIndex);

                if (slot.IsEmpty())
                    return TransitionType.Drop;

                if (slot.Item.Thing == transitionData.Item.Thing && slot.Amount < slot.Item.Thing.MaxStackAmount)
                    return TransitionType.Drop;

                return TransitionType.Swipe;
            }

            Debug.LogError($"Invalid item location data format");
            return TransitionType.Error;
        }

        public override bool CanDrop(ItemToTransitData transitionData, object itemLocationData)
        {
            if (itemLocationData is SlotItemLocationData locationData)
            {
                return m_inventory.CanAddToSlot(locationData.SlotIndex, transitionData.Item.Thing, 1);
            }

            Debug.LogError($"Invalid item location data format");
            return false;
        }

        public override bool CanSwipe(ItemToTransitData itemToSwipeOnData, object itemLocationData)
        {
            if (itemLocationData is SlotItemLocationData locationData)
            {
                if(!m_inventory.IsPreferable(itemToSwipeOnData.Item.Thing))
                    return false;

                var slot = m_inventory.SlotAt(locationData.SlotIndex);
                if(slot.IsEmpty())
                    return false;

                return true;
            }

            Debug.LogError($"Invalid item location data format");
            return false;
        }

        public override int Drop(ItemToTransitData transitionData, object itemLocationData)
        {
            if (itemLocationData is SlotItemLocationData locationData)
            {
                return m_inventory.AddToSlot(locationData.SlotIndex, transitionData.Item, transitionData.Amount);
            }

            Debug.LogError($"Invalid item location data format");
            return transitionData.Amount;
        }

        public override void RemoveDragged(int amount, object itemLocationData, bool destroyItemIfZero)
        {
            if (itemLocationData is SlotItemLocationData locationData)
            {
                m_inventory.RemoveFromSlot(locationData.SlotIndex, amount, destroyItemIfZero);
                return;
            }

            Debug.LogError($"Invalid item location data format");;
        }

        public override bool CanDragNDropLocaly(ItemToTransitData transitionData, object sourceItemLocationData, object destinationItemLocationData)
        {
            // because we can stack same things and can swipe different,
            // so drag n drop could be performed with any pair of slots
            return true;
        }

        public override void DragNDropLocaly(ItemToTransitData transitionData, object sourceItemLocationData, object destinationItemLocationData)
        {
            if (sourceItemLocationData is SlotItemLocationData sourceLocationData && destinationItemLocationData is SlotItemLocationData destinationLocationData)
            {
                if (sourceLocationData.SlotIndex == destinationLocationData.SlotIndex)
                    return;

                var sourceSlot = m_inventory.SlotAt(sourceLocationData.SlotIndex);
                var destinationSlot = m_inventory.SlotAt(destinationLocationData.SlotIndex);

                if (destinationSlot.IsEmpty())
                {
                    sourceSlot.ExchangeInfoWithAnotherSlot(destinationSlot);
                    return;
                }

                if(sourceSlot.Item.Thing != destinationSlot.Item.Thing)
                {
                    sourceSlot.ExchangeInfoWithAnotherSlot(destinationSlot);
                    return;
                }

                if (destinationSlot.Amount == destinationSlot.Item.Thing.MaxStackAmount)
                {
                    sourceSlot.ExchangeInfoWithAnotherSlot(destinationSlot);
                    return;
                }

                sourceSlot.Amount = m_inventory.AddToSlot(destinationLocationData.SlotIndex, sourceSlot.Item, sourceSlot.Amount);
                return;
            }

            Debug.LogError($"Invalid item location data format");
        }
    }
}

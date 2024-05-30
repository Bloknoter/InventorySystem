using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InventoryEngine.Transitions
{
    public class DimensionTransitionInventoryUnit : ItemTransitionUnitBase
    {
        private IDimensionInventory m_inventory;

        public DimensionTransitionInventoryUnit(IDimensionInventory inventory)
        {
            m_inventory = inventory;
        }

        public override ItemToTransitData GetItemToTransitData(object itemLocationData)
        {
            if(itemLocationData is DimensionItemLocationData locationData)
            {
                var itemData = m_inventory.DimensionItemInfoAtPos(locationData.Position.x, locationData.Position.y);
                return new ItemToTransitData(itemData.Item, itemData.Amount, new Vector2Int(locationData.Position.x - itemData.Position.x, locationData.Position.y - itemData.Position.y));
            }

            Debug.LogError($"Invalid item location data format");
            return null;
        }

        public override TransitionType PredictTransitionType(ItemToTransitData transitionData, object itemLocationData)
        {
            if (itemLocationData is DimensionItemLocationData locationData)
            {
                var itemData = m_inventory.DimensionItemInfoAtPos(locationData.Position.x, locationData.Position.y);
                if (itemData.Item == null)
                    return TransitionType.Drop;

                if (itemData.Item.Thing == transitionData.Item.Thing)
                {
                    if (itemData.Amount < itemData.Item.Thing.MaxStackAmount)
                        return TransitionType.Drop;
                    else
                        return TransitionType.Swipe;
                }

                return TransitionType.Swipe;
            }

            Debug.LogError($"Invalid item location data format");
            return TransitionType.Error;
        }

        public override bool CanDrop(ItemToTransitData transitionData, object itemLocationData)
        {
            if (itemLocationData is DimensionItemLocationData locationData)
            {
                return m_inventory.CanAddToPos(locationData.Position.x, locationData.Position.y, transitionData.Item.Thing, 1);
            }

            Debug.LogError($"Invalid item location data format");
            return false;
        }

        public override bool CanSwipe(ItemToTransitData itemToSwipeOnData, object itemLocationData)
        {
            if (itemLocationData is DimensionItemLocationData locationData)
            {
                if(!m_inventory.IsPreferable(itemToSwipeOnData.Item.Thing))
                    return false;

                var itemSize = DimensionItemContainer.DefaultSize;
                var sizeProp = itemToSwipeOnData.Item.GetProperty<DimensionItemSize>();
                if (sizeProp != null)
                    itemSize = sizeProp.Size;

                HashSet<Item> itemsDetected = new HashSet<Item>();
                bool canSwipe = true;
                for(int x = 0; x < itemSize.x;++x)
                {
                    for (int y = 0; y < itemSize.y; ++y)
                    {
                        var pos = locationData.Position;
                        pos.x += x;
                        pos.y += y;

                        if (m_inventory.IsPosValid(pos.x, pos.y))
                        {
                            var itemInfoAtPos = m_inventory.DimensionItemInfoAtPos(pos.x, pos.y);
                            if(itemInfoAtPos.Item != null)
                                itemsDetected.Add(itemInfoAtPos.Item);

                            if (itemsDetected.Count > 1)
                            {
                                canSwipe = false;
                                break;
                            }
                        }
                        else
                        {
                            canSwipe = false;
                            break;
                        }
                    }

                    if (!canSwipe)
                        break;
                }

                return canSwipe;
            }

            Debug.LogError($"Invalid item location data format");
            return false;
        }

        public override int Drop(ItemToTransitData transitionData, object itemLocationData)
        {
            if (itemLocationData is DimensionItemLocationData locationData)
            {
                return m_inventory.AddToPos(locationData.Position.x, locationData.Position.y, transitionData.Item, transitionData.Amount);
            }

            Debug.LogError($"Invalid item location data format");
            return transitionData.Amount;
        }

        public override bool CanDragNDropLocaly(ItemToTransitData transitionData, object sourceItemLocationData, object destinationItemLocationData)
        {
            if (sourceItemLocationData is DimensionItemLocationData sourceLocationData && destinationItemLocationData is DimensionItemLocationData destinationLocationData)
            {
                var itemData = m_inventory.DimensionItemInfoAtPos(sourceLocationData.Position.x, sourceLocationData.Position.y);
                bool canMove = m_inventory.CanMoveItem(itemData.Position.x, itemData.Position.y, destinationLocationData.Position.x, destinationLocationData.Position.y, false);
                return canMove;
            }

            Debug.LogError($"Invalid item location data format");
            return false;
        }

        public override void DragNDropLocaly(ItemToTransitData transitionData, object sourceItemLocationData, object destinationItemLocationData)
        {
            if (sourceItemLocationData is DimensionItemLocationData sourceLocationData && destinationItemLocationData is DimensionItemLocationData destinationLocationData)
            {
                var itemData = m_inventory.DimensionItemInfoAtPos(sourceLocationData.Position.x, sourceLocationData.Position.y);
                m_inventory.MoveItem(itemData.Position.x, itemData.Position.y, destinationLocationData.Position.x, destinationLocationData.Position.y, false);
                return;
            }

            Debug.LogError($"Invalid item location data format");
        }

        public override void RemoveDragged(int amount, object itemLocationData, bool destroyItemIfZero)
        {
            if (itemLocationData is DimensionItemLocationData locationData)
            {
                m_inventory.RemoveFromPos(locationData.Position.x, locationData.Position.y, amount, destroyItemIfZero);
                return;
            }

            Debug.LogError($"Invalid item location data format");
        }

    }
}

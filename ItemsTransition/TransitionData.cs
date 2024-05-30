using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InventoryEngine.Transitions
{
    public class ItemToTransitData
    {
        public readonly Item Item;
        public readonly int Amount;
        public Vector2 VisualPosition;
        public Vector2 VisualSize;
        public readonly object AdditionalData;

        public ItemToTransitData(Item item, int amount, object additionalData = null)
        {
            Item = item;
            Amount = amount;
            AdditionalData = additionalData;
        }
    }

    public enum TransitionType
    {
        Drop, Swipe, Error
    }

    public struct DimensionItemLocationData
    {
        public Vector2Int Position;
    }

    public struct SlotItemLocationData
    {
        public int SlotIndex;
    }
}

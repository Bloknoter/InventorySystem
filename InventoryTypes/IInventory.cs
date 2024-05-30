using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InventoryEngine
{
    public interface IInventory
    {
        public int ItemsCount { get; }

        EventCaller<IInventory> OnContentChanged { get; }

        ItemInfo ItemInfoAt(int itemIndex);

        bool IsPreferable(Thing thing);

        bool CanAdd(Thing thing, int amount);

        bool CanAdd(Dictionary<Thing, int> things);

        /// <summary> Returns the amount of items that can't be added </summary> 
        int Add(Item newItem, int amount);

        bool Contains(Thing thing, int amount);

        /// <summary> Clears inventory
        /// <paramref name="destroyItems">
        /// Is it required all items to be destroyed?
        /// </paramref>
        /// </summary> 
        void ClearAll(bool destroyItems = true);

        int GetAmountOf(Thing thing);

        /// <summary> Returns the amount of things that can't be removed because there is not enough amount of things </summary> 
        int Remove(Thing thing, int amount, bool destroyItems = true);

        object GetSerializedData();

        void SetSerializedData(object serializedData);
    }

    public struct ItemInfo
    {
        public Item Item;
        public int Amount;

        public ItemInfo(Item item = null, int amount = 0)
        {
            Item = item;
            Amount = amount;
        }
    }
}
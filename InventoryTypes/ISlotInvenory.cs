using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InventoryEngine
{
    public interface ISlotInventory : IInventory
    {
        int Size { get; }

        EventCaller<ISlotInventory, Slot> OnSlotAdded { get; }
        EventCaller<ISlotInventory, Slot> OnSlotRemoved { get; }

        Slot SlotAt(int index);

        void AddSlots(int amount);

        void RemoveSlots(int amount);

        bool CanAddToSlot(int index, Thing thing, int amount);

        /// <summary> Returns the amount of things that can't be added </summary> 
        int AddToSlot(int index, Item newItem, int amount);

        /// <summary> Returns the amount of things that can't be removed because there is no such amount of things </summary> 
        int RemoveFromSlot(int index, int amount, bool destroyItemIfZero = true);

        Slot GetFirstEmptySlot();
    }
}
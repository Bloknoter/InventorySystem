using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InventoryEngine
{
    public interface IItemContainer
    {
        IInventory Parent { get; }
        Item Item { get; }
        int Amount { get; set; }

        EventCaller<IItemContainer> OnContainerChanged { get; }

        void RemoveOne();

        void Clear();
    }
}
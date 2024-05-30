using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InventoryEngine.GUI
{
    public interface ISlotInvAdditionalContent
    {
        void Initialize(SlotInventoryDisplay inventoryDisplay, ISlotInventory inventory);

        void OnSlotAdded(ItemContainerDisplay containerDisplay);

        void OnSlotRemoved(ItemContainerDisplay containerDisplay);

        void Clear();
    }
}

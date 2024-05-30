using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InventoryEngine.GUI
{
    public interface IDimensionInvAdditionalContent
    {
        void Initialize(DimensionInventoryDisplay inventoryDisplay, IDimensionInventory inventory);

        void OnItemContainerAdded(ItemContainerDisplay containerDisplay);

        void OnItemContainerRemoved(ItemContainerDisplay containerDisplay);

        void Clear();
    }
}

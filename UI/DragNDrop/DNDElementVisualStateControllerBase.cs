using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InventoryEngine.GUI
{
    public abstract class DNDElementVisualStateControllerBase : MonoBehaviour
    {
        // DO NOT use GameObject.SetActive(false)
        public abstract void HideContent();

        public abstract void ShowContent();

        public abstract void ShowDragNDropState(bool canDrop, object locationData, Transitions.ItemToTransitData itemToTransitData);

        public abstract void HideDragNDropState();
    }
}

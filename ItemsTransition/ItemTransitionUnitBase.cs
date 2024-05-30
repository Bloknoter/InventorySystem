using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InventoryEngine.Transitions
{
    public abstract class ItemTransitionUnitBase
    {
        public abstract ItemToTransitData GetItemToTransitData(object itemLocationData);

        public abstract TransitionType PredictTransitionType(ItemToTransitData transitionData, object itemLocationData);

        public abstract bool CanDrop(ItemToTransitData transitionData, object itemLocationData);

        public abstract bool CanSwipe(ItemToTransitData itemToSwipeData, object locationData);

        public abstract int Drop(ItemToTransitData transitionData, object itemLocationData);

        public abstract void RemoveDragged(int amount, object itemLocationData, bool destroyItemIfZero);

        public abstract bool CanDragNDropLocaly(ItemToTransitData transitionData, object sourceItemLocationData, object destinationItemLocationData);

        public abstract void DragNDropLocaly(ItemToTransitData transitionData, object sourceItemLocationData, object destinationItemLocationData);
    }
}

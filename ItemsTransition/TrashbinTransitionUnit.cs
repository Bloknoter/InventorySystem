using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InventoryEngine.Transitions
{
    public class TrashbinTransitionUnit : ItemTransitionUnitBase
    {
        private ItemsTrashbinBase m_trashbin;
        private bool m_throwAwayInstead;

        public TrashbinTransitionUnit(ItemsTrashbinBase trashbin, bool throwAwayInstead) 
        {
            m_trashbin = trashbin;
            m_throwAwayInstead = throwAwayInstead;
        }

        public override ItemToTransitData GetItemToTransitData(object itemLocationData)
        {
            Debug.LogError($"Can't recover items from ItemsTrashbin; no items to transit");
            return null;
        }

        public override TransitionType PredictTransitionType(ItemToTransitData transitionData, object itemLocationData)
        {
            return TransitionType.Drop;
        }

        public override bool CanDrop(ItemToTransitData transitionData, object itemLocationData)
        {
            if (m_throwAwayInstead)
                return m_trashbin.CanThrowAway(transitionData.Item, transitionData.Amount);
            else
                return true;
        }

        public override bool CanSwipe(ItemToTransitData itemToSwipeData, object locationData)
        {
            return false;
        }

        public override int Drop(ItemToTransitData transitionData, object itemLocationData)
        {
            if (m_throwAwayInstead)
                m_trashbin.ThrowAwayItem(transitionData.Item, transitionData.Amount);
            else
                m_trashbin.UtilizeItem(transitionData.Item);

            return 0;
        }

        public override void RemoveDragged(int amount, object itemLocationData, bool destroyItemIfZero)
        {
            Debug.LogError($"Can't recover items from ItemsTrashbin; nothing to remove");
        }

        public override bool CanDragNDropLocaly(ItemToTransitData transitionData, object sourceItemLocationData, object destinationItemLocationData)
        {
            return false;
        }

        public override void DragNDropLocaly(ItemToTransitData transitionData, object sourceItemLocationData, object destinationItemLocationData)
        {
            Debug.LogError($"Can't move items inside ItemsTrashbin");
        }
    }
}

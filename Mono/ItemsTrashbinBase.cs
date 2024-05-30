using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InventoryEngine
{
    public abstract class ItemsTrashbinBase : MonoBehaviour
    {
        public virtual void UtilizeItem(Item item)
        {
            item.DestroyItem();
        }

        public abstract bool CanThrowAway(Item item, int amount);

        public abstract void ThrowAwayItem(Item item, int amount);
    }
}

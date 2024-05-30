using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InventoryEngine.Caching
{
    public class ContainerCache
    {
        public Thing Thing;
        public int Amount;

        public ContainerCache(Thing thing, int amount)
        {
            Thing = thing;
            Amount = amount;
        }
    }
}

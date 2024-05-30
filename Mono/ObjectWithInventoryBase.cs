using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InventoryEngine
{
    public abstract class ObjectWithInventoryBase : MonoBehaviour
    {
        [SerializeField]
        private string m_invetoryIdentifier;

        public string InventoryIdentifier => m_invetoryIdentifier;

        public abstract IInventory Inventory { get; }

        public static ObjectWithInventoryBase FindInventory(GameObject owner, string identifier)
        {
            var inventories = owner.GetComponents<ObjectWithInventoryBase>();
            for (int i = 0; i < inventories.Length; ++i)
            {
                if (inventories[i].InventoryIdentifier == identifier)
                    return inventories[i];
            }

            return null;
        }
    }
}

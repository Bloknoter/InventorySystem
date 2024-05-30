using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;


namespace InventoryEngine
{
    public class ObjectWithSlotInventory : ObjectWithInventoryBase
    {
        [SerializeField]
        private List<ItemBlueprint> m_startItems;

        [Min(1)]
        [SerializeField]
        private int m_startSize = 1;

        [Header("Pass the name of thing property you want to be used for inventory. Example: ThingProperty. (Could be empty)")]
        [SerializeField]
        private string m_inventoryType;

        private const string c_namespacePrefix = "InventoryEngine.";

        private ISlotInventory m_inventory;

        public ISlotInventory SlotInventory => m_inventory;
        public override IInventory Inventory => m_inventory;

        private void Start()
        {
            if (m_inventory == null)
            {
                Type genericClass = typeof(SlotInventory<>);
                Type typeArgument = GetInventoryType();
                Type classType = genericClass.MakeGenericType(typeArgument);

                object created = Activator.CreateInstance(classType, m_startSize);
                m_inventory = (ISlotInventory)created;

                for (int i = 0; i < m_startItems.Count; ++i)
                {
                    m_inventory.Add(Item.Create(m_startItems[i].Thing), m_startItems[i].Amount);
                }
            }
        }

        private Type GetInventoryType()
        {
            Type inventoryType;

            if (string.IsNullOrEmpty(m_inventoryType))
                inventoryType = typeof(ThingProperty);
            else
                inventoryType = Type.GetType(c_namespacePrefix + m_inventoryType);

            if (inventoryType == null)
            {
                Debug.LogError($"Cannot create type for inventory from type name '{c_namespacePrefix + m_inventoryType}'. Default type will be used");
                inventoryType = typeof(ThingProperty);
            }

            return inventoryType;
        }

        public static ObjectWithSlotInventory FindSlotInventory(GameObject owner, string identifier)
        {
            var inventories = owner.GetComponents<ObjectWithSlotInventory>();
            for (int i = 0; i < inventories.Length; ++i)
            {
                if (inventories[i].InventoryIdentifier == identifier)
                    return inventories[i];
            }

            return null;
        }
    }
}

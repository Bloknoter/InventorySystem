using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventoryEngine
{
    public abstract class ThingProperty : ScriptableObject
    {
        [SerializeField]
        private bool m_savable = true;

        private string m_uniqueID;

        private Item m_item;
        private bool m_isCreatedAsNew;


        public bool Savable => m_savable;
        public string UniqueID => m_uniqueID;

        public Item Item => m_item;
        public bool IsCreatedAsNew => m_isCreatedAsNew;

        public void InitializeFromItem(Item item, bool isCreatedAsNew)
        {
            m_item = item;
            m_isCreatedAsNew = isCreatedAsNew;

            m_uniqueID = GetUniquePropID();
        }

        protected abstract string GetUniquePropID();

        public virtual void LoadFromSave(object data)
        {

        }

        public virtual void Initialize()
        {

        }

        public virtual void PostInitialize()
        {

        }

        public virtual void MergeInto(ThingProperty thingProperty, int baseAmount, int mergingAmount)
        {

        }

        public virtual ThingProperty Clone()
        {
            var prop = Instantiate(this);
            prop.m_item = null;
            return prop;
        }

        public virtual object CreateDataForSave()
        {
            return null;
        }

        public virtual void OnDestroyItem()
        {

        }
    }
}

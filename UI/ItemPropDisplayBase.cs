using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InventoryEngine.GUI
{
    public class ItemPropDisplayBase : MonoBehaviour
    {
        protected IItemContainer m_container;

        public virtual void Initialize(IItemContainer itemContainer)
        {
            m_container = itemContainer;
        }

        public virtual void Clear()
        {
            m_container = null;
        }
    }
}
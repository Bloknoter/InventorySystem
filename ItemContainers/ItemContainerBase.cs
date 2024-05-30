using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InventoryEngine
{
    public class ItemContainerBase : IItemContainer
    {
        public EventCaller<IItemContainer> OnContainerChanged { get; private set; } = new EventCaller<IItemContainer>();

        private IInventory m_parent;

        protected Item m_item;
        protected int m_amount = 0;

        public IInventory Parent => m_parent;

        public Item Item => m_item;

        public int Amount 
        {
            get => m_amount;
            set
            {
                if (value <= 0 && m_item != null)
                {
                    m_item.DestroyItem();
                }
                else
                {
                    int prev = m_amount;

                    if (m_item != null)
                        m_amount = value;
                    else
                        Debug.LogError("[Amount.set] You are trying to add some amount to slot, but slot is empty ('item' variable is null) !");

                    if (prev != value)
                        CallChangedEvent();
                }
            }
        }

        public ItemContainerBase(IInventory parent, Item item, int amount)
        {
            if (parent == null)
                throw new ArgumentNullException($"[{nameof(ItemContainerBase)}.ctor]: Creating item container: 'parent' argument is null!");

            m_parent = parent;

            if (item == null || amount <= 0)
            {
                m_item = null;
                m_amount = 0;
            }
            else
            {
                m_item = item;
                m_amount = amount;

                if(m_item.ItemContainer != null)
                    m_item.ItemContainer.Clear();

                m_item.ItemContainer = this;
            }

            SetupItemListeners(m_item);
        }

        protected void SetupItemListeners(Item item)
        {
            if (item != null)
            {
                item.OnPreDestroy += OnDestroyItem;
            }
        }

        protected void RemoveItemListeners(Item item)
        {
            if (item != null)
            {
                item.OnPreDestroy -= OnDestroyItem;
            }
        }

        protected void CallChangedEvent()
        {
            OnContainerChanged.Invoke(this);
        }

        private void OnDestroyItem()
        {
            Clear();
        }

        public void RemoveOne()
        {
            Amount--;
        }

        public void Clear()
        {
            if (m_item == null)
                return;

            RemoveItemListeners(m_item);
            m_item.ItemContainer = null;
            m_item = null;
            m_amount = 0;
            CallChangedEvent();
        }
    }
}

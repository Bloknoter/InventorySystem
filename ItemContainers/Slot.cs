using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace InventoryEngine
{
    public class Slot : ItemContainerBase
    {
        public Slot(ISlotInventory parent) : base(parent, null, 0)
        {
            
        }

        public void SetItem(Item newItem, int newAmount)
        {
            if (!IsEmpty())
            {
                RemoveItemListeners(m_item);
                m_item.ItemContainer = null;
            }

            var prevItem = m_item;
            var prevAmount = m_amount;

            m_item = null;
            m_amount = 0;

            if (newItem.ItemContainer != null)
                newItem.ItemContainer.Clear();

            if (newAmount > 0)
            {
                if (!Parent.IsPreferable(newItem.Thing))
                {
                    Debug.LogError($"[{nameof(Slot)}.{nameof(SetItem)}]: You are trying to set item (thing: {newItem.Thing.Name}) that is typically incompatible with inventory type! Assignment is cancelled");
                    return;
                }

                m_item = newItem;
                m_amount = newAmount;
                m_item.ItemContainer = this;
                SetupItemListeners(newItem);
            }
            else
            {
                Debug.LogError($"[{nameof(Slot)}.{nameof(SetItem)}]: You are trying to set item with zero or negative amount (thing: {newItem.Thing.Name})");
                CallChangedEvent();
                return;
            }

            if (m_item != prevItem || m_amount != prevAmount)
                CallChangedEvent();
        }

        public bool IsEmpty() => m_item == null || m_amount <= 0;

        public void ExchangeInfoWithAnotherSlot(Slot anotherSlot)
        {
            if(this == anotherSlot) 
                return;

            if (IsEmpty() && anotherSlot.IsEmpty())
                return;

            if (!IsEmpty() && !anotherSlot.Parent.IsPreferable(m_item.Thing))
            {
                Debug.LogError($"[{nameof(Slot)}.{nameof(ExchangeInfoWithAnotherSlot)}]: You are trying to set item (thing: {m_item.Thing.Name}) that is typically incompatible with inventory type! Assignment is cancelled");
                return;
            }

            if (!anotherSlot.IsEmpty() && !Parent.IsPreferable(anotherSlot.m_item.Thing))
            {
                Debug.LogError($"[{nameof(Slot)}.{nameof(ExchangeInfoWithAnotherSlot)}]: You are trying to set item (thing: {anotherSlot.m_item.Thing}) that is typically incompatible with inventory type! Assignment is cancelled");
                return;
            }

            int myAmount = m_amount;
            Item myItem = m_item;

            RemoveItemListeners(myItem);
            anotherSlot.RemoveItemListeners(anotherSlot.m_item);

            m_item = anotherSlot.m_item;
            m_amount = anotherSlot.m_amount;

            anotherSlot.m_item = myItem;
            anotherSlot.m_amount = myAmount;
            
            if(m_item != null)
                m_item.ItemContainer = this;
            if (anotherSlot.m_item != null)
                anotherSlot.m_item.ItemContainer = anotherSlot;

            SetupItemListeners(m_item);
            anotherSlot.SetupItemListeners(anotherSlot.m_item);

            CallChangedEvent();
            anotherSlot.CallChangedEvent();
        }

        public override string ToString()
        {
            if (!IsEmpty())
                return $"Slot info\n   Item: {m_item.Thing.Name} \n   Amount: {m_amount} \n";
            else
                return $"Slot info\n   Empty";
        }
    }
}

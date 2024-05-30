using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InventoryEngine
{
    public class UnlimitedInventory : IInventory
    {
        public EventCaller<IInventory> OnContentChanged { get; private set; } = new EventCaller<IInventory>();

        private List<ItemContainerBase> m_items = new List<ItemContainerBase>();

        public int ItemsCount => m_items.Count;

        private void CreateAndSetupNewContainer(Item item, int amount)
        {
            var itemContainer = new ItemContainerBase(this, item, amount);
            itemContainer.OnContainerChanged.AddListener(OnContainerChanged);
            m_items.Add(itemContainer);
        }

        private void ClearAndRemoveContainer(ItemContainerBase itemContainer)
        {
            itemContainer.OnContainerChanged.RemoveListener(OnContainerChanged);
            m_items.Remove(itemContainer);
        }

        public bool IsPreferable(Thing thing)
        {
            return true;
        }

        public ItemInfo ItemInfoAt(int itemIndex)
        {
            return new ItemInfo(m_items[itemIndex].Item, m_items[itemIndex].Amount);
        }

        public bool CanAdd(Thing thing, int amount)
        {
            return true;
        }

        public bool CanAdd(Dictionary<Thing, int> things)
        {
            return true;
        }

        public int Add(Item newItem, int amount)
        {
            if (amount <= 0)
                return 0;

            if (newItem == null)
                return amount;

            for (int i = 0; i < m_items.Count; i++)
            {
                if (m_items[i].Item.Thing == newItem.Thing)
                {
                    if (newItem.Thing.MaxStackAmount - m_items[i].Amount >= amount)
                    {
                        newItem.MergeInto(m_items[i].Item, m_items[i].Amount, amount);
                        newItem.DestroyItem();
                        m_items[i].Amount += amount;
                        OnContentChanged?.Invoke(this);
                        return 0;
                    }
                    else
                    {
                        int addedAmount = newItem.Thing.MaxStackAmount - m_items[i].Amount;
                        newItem.MergeInto(m_items[i].Item, m_items[i].Amount, addedAmount);
                        amount -= addedAmount;
                        m_items[i].Amount += addedAmount;
                    }
                }
            }

            if (amount > 0)
            {
                int insertionCount = amount / newItem.Thing.MaxStackAmount + 1;
                for(int i = 0; i < insertionCount; ++i)
                {
                    if (amount <= 0)
                        break;

                    if(amount > newItem.Thing.MaxStackAmount)
                    {
                        CreateAndSetupNewContainer(newItem.Clone(), newItem.Thing.MaxStackAmount);
                        amount -= newItem.Thing.MaxStackAmount;
                    }
                    else
                    {
                        CreateAndSetupNewContainer(newItem, amount);
                    }
                }

                OnContentChanged?.Invoke(this);
            }

            return 0;
        }

        public bool Contains(Thing thing, int amount)
        {
            for (int i = 0; i < m_items.Count; i++)
            {
                if (m_items[i].Item.Thing == thing)
                {
                    if (m_items[i].Amount >= amount)
                        return true;
                    else
                        amount -= m_items[i].Amount;
                }
            }

            return false;
        }

        public void ClearAll(bool destroyItems = true)
        {
            for (int i = 0; i < m_items.Count; i++)
            {
                m_items[i].OnContainerChanged.RemoveListener(OnContainerChanged);
                if (destroyItems)
                    m_items[i].Item.DestroyItem();
                else
                    m_items[i].Clear();
            }

            m_items.Clear();
        }

        public int GetAmountOf(Thing thing)
        {
            int amount = 0;

            for (int i = 0; i < m_items.Count; i++)
            {
                if (m_items[i].Item.Thing == thing)
                {
                    amount += m_items[i].Amount;
                }
            }

            return amount;
        }

        public int Remove(Thing thing, int amount, bool destroyItems = true)
        {
            for (int i = 0; i < m_items.Count; ++i)
            {
                if (amount <= 0)
                    break;

                if (m_items[i].Item.Thing == thing)
                {
                    if (m_items[i].Amount > amount)
                    {
                        m_items[i].Amount -= amount;
                        return 0;
                    }
                    else
                    {
                        amount -= m_items[i].Amount;
                        m_items[i].OnContainerChanged.RemoveListener(OnContainerChanged);
                        if (destroyItems)
                            m_items[i].Item.DestroyItem();
                        else
                            m_items[i].Clear();

                        m_items.RemoveAt(i);
                        i--;
                    }
                }
            }

            return amount;
        }

        public object GetSerializedData()
        {
            object[] data = new object[m_items.Count];

            for (int i = 0; i < m_items.Count; i++)
            {
                Dictionary<string, object> slotData = new Dictionary<string, object>();
                slotData.Add("item", m_items[i].Item.GetSerializedData());
                slotData.Add("amount", m_items[i].Amount);
                data[i] = slotData;
            }

            return data;
        }

        public void SetSerializedData(object serializedData)
        {
            if (serializedData == null)
                return;

            object[] data = (object[])serializedData;

            ClearAll();

            for (int i = 0; i < data.Length; i++)
            {
                Dictionary<string, object> slotData = (Dictionary<string, object>)data[i];
                if (slotData["item"] != null)
                {
                    CreateAndSetupNewContainer(Item.Create(slotData["item"]), (int)slotData["amount"]);
                }
            }
        }

        private void OnContainerChanged(IItemContainer container)
        {
            if (container.Item == null)
                ClearAndRemoveContainer((ItemContainerBase)container);

            OnContentChanged?.Invoke(this);
        }
    }
}

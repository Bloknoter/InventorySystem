using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InventoryEngine
{
    public class SlotInventory<T> : ISlotInventory where T : ThingProperty
    {
        public EventCaller<IInventory> OnContentChanged { get; private set; } = new EventCaller<IInventory>();
        public EventCaller<ISlotInventory, Slot> OnSlotAdded { get; private set; } = new EventCaller<ISlotInventory, Slot>();
        public EventCaller<ISlotInventory, Slot> OnSlotRemoved { get; private set; } = new EventCaller<ISlotInventory, Slot>();

        private const string c_itemSaveID = "item";
        private const string c_amountSaveID = "amount";

        private List<Slot> m_slots = new List<Slot>();

        public int Size => m_slots.Count;

        public int ItemsCount
        {
            get
            {
                int amount = 0;
                for(int i = 0; i < m_slots.Count;i++)
                {
                    if (!m_slots[i].IsEmpty())
                        ++amount;
                }
                return amount;
            }
        }

        public SlotInventory(int size)
        {
            AddSlots(size);
        }

        public void AddSlots(int amount)
        {
            for(int i = 0; i < amount;++i)
            {
                var slot = new Slot(this);
                slot.OnContainerChanged.AddListener(OnSlotChanged);
                m_slots.Add(slot);

                OnSlotAdded.Invoke(this, slot);
            }
        }

        public void RemoveSlots(int amount)
        {
            amount = Mathf.Min(amount, m_slots.Count);
            for (int i = 0; i < amount; ++i)
            {
                var slot = m_slots[m_slots.Count - 1];
                slot.OnContainerChanged.RemoveListener(OnSlotChanged);
                m_slots.RemoveAt(m_slots.Count - 1);

                OnSlotRemoved.Invoke(this, slot);
            }
        }

        public Slot SlotAt(int index)
        {
            if (index < 0 || index >= Size)
            {
                Debug.LogError($"[SlotInventory.SlotAt] You are trying to get slot at {index} but the slot range is [0 - {Size})");
                return null;
            }
            return m_slots[index];
        }

        public bool IsPreferable(Thing thing)
        {
            return typeof(T) == typeof(ThingProperty) || thing.GetPropertyOfType<T>() != null;
        }

        public ItemInfo ItemInfoAt(int itemIndex)
        {
            int currentIndex = 0;
            for (int i = 0; i < m_slots.Count; i++)
            {
                if (!m_slots[i].IsEmpty())
                {
                    if (currentIndex == itemIndex)
                        return new ItemInfo(m_slots[i].Item, m_slots[i].Amount);
                    else
                        ++currentIndex;
                }
            }

            Debug.LogError($"[SlotInventory.ItemInfoAt] You are trying to get item info at {itemIndex} but the items' list range is [0 - {currentIndex})");
            return new ItemInfo();
        }

        public bool CanAdd(Thing thing, int amount)
        {
            if (!IsPreferable(thing))
                return false;

            for (int i = 0; i < m_slots.Count; i++)
            {
                if (!m_slots[i].IsEmpty())
                {
                    if (m_slots[i].Item.Thing == thing)
                    {
                        amount -= thing.MaxStackAmount - m_slots[i].Amount;
                        if (amount <= 0)
                            break;
                    }
                }
                else
                {
                    if (thing.MaxStackAmount > amount)
                    {
                        amount = 0;
                        break;
                    }
                    else
                        amount -= thing.MaxStackAmount;
                }
            }

            if (amount > 0)
                return false;

            return true;
        }

        public bool CanAdd(Dictionary<Thing, int> things)
        {
            throw new System.NotImplementedException("CanAdd for dictionary of things is not working properly");

            // checking every thing in separate way is not correct
            // it must check like ALL things can be added at the same time
            foreach (var thingWithAmount in things)
            {
                if(!CanAdd(thingWithAmount.Key, thingWithAmount.Value))
                    return false;
            }

            return true;
        }

        /// <summary> Returns the amount of items that can't be added </summary> 
        public int Add(Item newItem, int amount)
        {
            if(amount <= 0)
                return 0;

            if (newItem == null)
                return amount;

            if (!IsPreferable(newItem.Thing))
                return amount;

            List<int> emptySlots = new List<int>();
            for (int i = 0; i < m_slots.Count; i++)
            {
                if (!m_slots[i].IsEmpty())
                {
                    if (m_slots[i].Item.Thing == newItem.Thing)
                    {
                        if (newItem.Thing.MaxStackAmount - m_slots[i].Amount >= amount)
                        {
                            newItem.MergeInto(m_slots[i].Item, m_slots[i].Amount, amount);
                            newItem.DestroyItem();
                            m_slots[i].Amount += amount;
                            return 0;
                        }
                        else
                        {
                            int addedAmount = newItem.Thing.MaxStackAmount - m_slots[i].Amount;
                            newItem.MergeInto(m_slots[i].Item, m_slots[i].Amount, addedAmount);
                            amount -= addedAmount;
                            m_slots[i].Amount += addedAmount;
                        }
                    }
                }
                else
                {
                    emptySlots.Add(i);
                }
            }

            if (amount > 0)
            {
                for (int i = 0; i < emptySlots.Count; i++)
                {
                    if (newItem.Thing.MaxStackAmount >= amount)
                    {
                        m_slots[emptySlots[i]].SetItem(newItem, amount);
                        return 0;
                    }
                    else
                    {
                        amount -= newItem.Thing.MaxStackAmount;
                        m_slots[emptySlots[i]].SetItem(newItem.Clone(), newItem.Thing.MaxStackAmount);
                    }
                }
            }

            return amount;
        }

        /// <summary> Returns the amount of things that can't be removed because there is no such amount of things </summary> 
        public int Remove(Thing newthing, int amount, bool destroyItemsIfZero = true)
        {
            for (int i = 0; i < m_slots.Count; ++i)
            {
                if (amount <= 0)
                    break;

                if (!m_slots[i].IsEmpty() && m_slots[i].Item.Thing == newthing)
                {
                    if (m_slots[i].Amount > amount)
                    {
                        m_slots[i].Amount -= amount;
                        return 0;
                    }
                    else
                    {
                        amount -= m_slots[i].Amount;
                        if (destroyItemsIfZero)
                            m_slots[i].Item.DestroyItem();
                        else
                            m_slots[i].Clear();
                    }
                }
            }

            return amount;
        }

        public bool CanAddToSlot(int index, Thing thing, int amount)
        {
            if (amount <= 0)
                return false;

            if (index < 0 || index >= m_slots.Count)
            {
                Debug.LogError($"Can't add to slot with index '{index}'. Index must be non-negative and less than the size of inventory ({m_slots.Count})");
                return false;
            }

            if (!IsPreferable(thing))
                return false;

            if (m_slots[index].IsEmpty())
                return amount <= thing.MaxStackAmount;
            else if (m_slots[index].Item.Thing == thing)
                return amount <= thing.MaxStackAmount - m_slots[index].Amount;

            return false;
        }

        /// <summary> Returns the amount of things that can't be added </summary> 
        public int AddToSlot(int index, Item newItem, int amount)
        {
            if (amount <= 0)
                return 0;

            if (newItem == null)
                return amount;

            if (index < 0 || index >= m_slots.Count)
            {
                Debug.LogError($"Can't add to slot with index '{index}'. Index must be non-negative and less than the size of inventory ({m_slots.Count})");
                return amount;
            }

            if (!IsPreferable(newItem.Thing))
                return amount;

            if (m_slots[index].IsEmpty())
            {
                if (newItem.Thing.MaxStackAmount >= amount)
                {
                    m_slots[index].SetItem(newItem, amount);
                    return 0;
                }
                else
                {
                    m_slots[index].SetItem(newItem.Clone(), newItem.Thing.MaxStackAmount);
                    return amount - newItem.Thing.MaxStackAmount;
                }
            }
            else if (m_slots[index].Item.Thing == newItem.Thing)
            {
                if (newItem.Thing.MaxStackAmount - m_slots[index].Amount >= amount)
                {
                    newItem.MergeInto(m_slots[index].Item, m_slots[index].Amount, amount);
                    newItem.DestroyItem();
                    m_slots[index].Amount += amount;
                    return 0;
                }
                else
                {
                    int addedAmount = newItem.Thing.MaxStackAmount - m_slots[index].Amount;
                    newItem.MergeInto(m_slots[index].Item, m_slots[index].Amount, addedAmount);
                    m_slots[index].Amount += addedAmount;
                    return amount - addedAmount;
                }
            }

            return amount;
        }

        /// <summary> Returns the amount of things that can't be removed because there is no such amount of things in slot </summary> 
        public int RemoveFromSlot(int index, int amount, bool destroyItemIfZero = true)
        {
            if (index < 0 || index >= m_slots.Count)
            {
                Debug.LogError($"Can't remove from slot with index '{index}'. Index must be non-negative and less than the size of inventory ({m_slots.Count})");
                return amount;
            }

            if (m_slots[index].IsEmpty())
                return amount;

            if(m_slots[index].Amount > amount)
            {
                m_slots[index].Amount -= amount;
                return 0;
            }

            int prevAmount = m_slots[index].Amount;
            if (destroyItemIfZero)
                m_slots[index].Item.DestroyItem();
            else
                m_slots[index].Clear();
            return amount - prevAmount;            
        }

        public bool Contains(Thing thing, int amount)
        {
            for(int i = 0; i < m_slots.Count;i++)
            {
                if (!m_slots[i].IsEmpty() && m_slots[i].Item.Thing == thing)
                {
                    if (m_slots[i].Amount >= amount)
                        return true;
                    else
                        amount -= m_slots[i].Amount;
                }
            }

            return false;
        }

        public void ClearAll(bool destroyItems = true)
        {
            for(int i = 0; i < m_slots.Count;i++)
            {
                m_slots[i].OnContainerChanged.RemoveListener(OnSlotChanged);

                if (destroyItems && !m_slots[i].IsEmpty())
                    m_slots[i].Item.DestroyItem();
                else
                    m_slots[i].Clear();

                m_slots[i].OnContainerChanged.AddListener(OnSlotChanged);
            }

            OnContentChanged?.Invoke(this);
        }

        public Slot GetFirstEmptySlot()
        {
            for(int i = 0; i < m_slots.Count;i++)
            {
                if (m_slots[i].IsEmpty())
                    return m_slots[i];
            }

            return null;
        }

        public int GetAmountOf(Thing thing)
        {
            int amount = 0;

            for (int i = 0; i < m_slots.Count; i++)
            {
                if (!m_slots[i].IsEmpty())
                {
                    if (m_slots[i].Item.Thing == thing)
                    {
                        amount += m_slots[i].Amount;
                    }
                }
            }

            return amount;
        }

        public object GetSerializedData()
        {
            object[] data = new object[Size];

            for(int i = 0; i < Size;i++)
            {
                Dictionary<string, object> slotData = new Dictionary<string, object>();
                if (!m_slots[i].IsEmpty())
                    slotData.Add(c_itemSaveID, m_slots[i].Item.GetSerializedData());
                else
                    slotData.Add(c_itemSaveID, null);
                slotData.Add(c_amountSaveID, m_slots[i].Amount);
                data[i] = slotData;
            }

            return data;
        }

        public void SetSerializedData(object serializedData)
        {
            if (serializedData == null)
                return;

            ClearAll();

            for (int i = 0; i < Size; i++)
                m_slots[m_slots.Count - 1].OnContainerChanged.RemoveListener(OnSlotChanged);
            m_slots.Clear();

            object[] data = (object[])serializedData;

            for(int i = 0; i < data.Length;i++)
            {
                Dictionary<string, object> slotData = (Dictionary<string, object>)data[i];
                AddSlots(1);
                if (slotData[c_itemSaveID] != null)
                    m_slots[m_slots.Count - 1].SetItem(Item.Create(slotData[c_itemSaveID]), (int)slotData[c_amountSaveID]);
            }

            OnContentChanged?.Invoke(this);
        }

        private void OnSlotChanged(IItemContainer slot)
        {
            OnContentChanged?.Invoke(this);
        }
    }
}

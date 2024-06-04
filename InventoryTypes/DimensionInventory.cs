using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InventoryEngine
{
    public class DimensionInventory<T> : IDimensionInventory where T : ThingProperty
    {
        private const string c_itemSaveID = "item";
        private const string c_amountSaveID = "amount";
        private const string c_positionSaveID = "pos";

        private const int c_serializedItemsDataIndexOffset = 2;

        public EventCaller<IInventory> OnContentChanged { get; private set; } = new EventCaller<IInventory>();
        public EventCaller<IDimensionInventory> OnSizeChanged { get; private set; } = new EventCaller<IDimensionInventory>();
        public EventCaller<IDimensionInventory, DimensionItemInfo> OnItemPositionChanged { get; private set; } = new EventCaller<IDimensionInventory, DimensionItemInfo>();
        public EventCaller<IDimensionInventory, DimensionItemContainer> OnItemContainerAdded { get; private set; } = new EventCaller<IDimensionInventory, DimensionItemContainer>();
        public EventCaller<IDimensionInventory, DimensionItemContainer> OnItemContainerRemoved { get; private set; } = new EventCaller<IDimensionInventory, DimensionItemContainer>();

        private int m_width;
        private int m_height;

        private DimensionItemContainer[,] m_itemsMap;
        private List<DimensionItemContainer> m_items;


        public int Size => m_width * m_height;

        public int ItemsCount => m_items.Count;

        public int Width => m_width;
        public int Height => m_height;


        public DimensionInventory(int width, int height)
        {
            if (width < 1)
                throw new System.ArgumentOutOfRangeException($"[DimensionInventory.ctor] Inventory width must be positive number! (passed {width})");
            if (height < 1)
                throw new System.ArgumentOutOfRangeException($"[DimensionInventory.ctor] Inventory height must be positive number! (passed {height})");

            m_width = width;
            m_height = height;

            m_items = new List<DimensionItemContainer>();
            m_itemsMap = new DimensionItemContainer[m_width, m_height];
        }

        public bool IsPreferable(Thing thing)
        {
            return typeof(T) == typeof(ThingProperty) || thing.GetPropertyOfType<T>() != null;
        }

        private void AddItemToMap(DimensionItemContainer item)
        {
            var size = item.GetItemSize();
            for (int x = 0; x < size.x; ++x)
            {
                for (int y = 0; y < size.y; ++y)
                {
                    if (m_itemsMap[item.Position.x + x, item.Position.y + y] != null)
                    {
                        Debug.LogError($"IntersectionOfItemsException: item 1 ({item.Item.Thing.name} x{item.Amount}) intersects item 2 ({m_itemsMap[item.Position.x + x, item.Position.y + y].Item.Thing.name} x{m_itemsMap[item.Position.x + x, item.Position.y + y].Amount}) at pos [{item.Position.x + x}, {item.Position.y + y}]");
                        return;
                    }
                    m_itemsMap[item.Position.x + x, item.Position.y + y] = item;
                }
            }
        }

        private void RemoveItemFromMap(DimensionItemContainer item)
        {
            var size = item.GetItemSize();
            for(int x = 0;x < size.x;++x)
            {
                for (int y = 0; y < size.y; ++y)
                {
                    m_itemsMap[item.Position.x + x, item.Position.y + y] = null;
                }
            }
        }

        private void MoveItemToPosInMap(DimensionItemContainer item, Vector2Int newPosition)
        {
            RemoveItemFromMap(item);
            item.Position = newPosition;
            AddItemToMap(item);
        }

        private void RebuildItemMap()
        {
            m_itemsMap = new DimensionItemContainer[m_width, m_height];

            for (int i = 0; i < m_items.Count; ++i)
            {
                AddItemToMap(m_items[i]);
            }
        }
        

        private void CreateAndSetupNewContainer(Vector2Int position, Item item, int amount)
        {
            var itemContainer = new DimensionItemContainer(this, position, item, amount);
            itemContainer.OnContainerChanged.AddListener(OnContainerChanged);
            AddItemToMap(itemContainer);
            m_items.Add(itemContainer);

            OnItemContainerAdded?.Invoke(this, itemContainer);
        }

        private void ClearAndRemoveContainer(DimensionItemContainer itemContainer)
        {
            itemContainer.OnContainerChanged.RemoveListener(OnContainerChanged);
            RemoveItemFromMap(itemContainer);
            m_items.Remove(itemContainer);

            OnItemContainerRemoved?.Invoke(this, itemContainer);
        }

        private int GetItemIndex(DimensionItemContainer item)
        {
            for(int i = 0; i < m_items.Count;++i)
            {
                if (m_items[i] ==  item)
                    return i;
            }

            return -1;
        }

        public bool IsEmpty(int x, int y)
        {
            return m_itemsMap[x, y] == null;
        }

        public bool IsPosValid(int x, int y)
        {
            return x >= 0 && y >= 0 && x < m_width && y < m_height;
        }

        public bool CanMoveItem(int xFrom, int yFrom, int xTo, int yTo, bool useFromPosAsPivot = false)
        {
            if(xFrom == xTo && yFrom == yTo) 
                return true;

            if (IsEmpty(xFrom, yFrom))
                return false;

            var itemInfo = m_itemsMap[xFrom, yFrom];
            var newItemPos = new Vector2Int(xTo, yTo);
            var pivotPos = itemInfo.Position;
            if(useFromPosAsPivot)
            {
                var oldItemPos = itemInfo.Position;
                newItemPos.x -= xFrom - oldItemPos.x;
                newItemPos.y -= yFrom - oldItemPos.y;
                pivotPos.x = xFrom; 
                pivotPos.y = yFrom;
            }

            var itemSize = itemInfo.GetItemSize();

            if(newItemPos.x + itemSize.x > m_width)
                return false;
            if (newItemPos.y + itemSize.y > m_height)
                return false;

            if (IsEmpty(newItemPos.x, newItemPos.y) || m_itemsMap[newItemPos.x, newItemPos.y] == itemInfo)
            {
                for (int x = 0; x < itemSize.x; ++x)
                {
                    for (int y = 0; y < itemSize.y; ++y)
                    {
                        if (!IsEmpty(newItemPos.x + x, newItemPos.y + y) && m_itemsMap[newItemPos.x + x, newItemPos.y + y] != itemInfo)
                            return false;
                    }
                }

                return true;
            }

            bool sameTypeOfItems = itemInfo.Item.Thing == m_itemsMap[newItemPos.x, newItemPos.y].Item.Thing;
            bool destinationNotFull = m_itemsMap[newItemPos.x, newItemPos.y].Amount < m_itemsMap[newItemPos.x, newItemPos.y].Item.Thing.MaxStackAmount;
            return sameTypeOfItems && destinationNotFull;
        }

        public void MoveItem(int xFrom, int yFrom, int xTo, int yTo, bool useFromPosAsPivot = false)
        {
            if (xFrom == xTo && yFrom == yTo)
                return;

            var itemInfo = m_itemsMap[xFrom, yFrom];
            var newItemPos = new Vector2Int(xTo, yTo);
            var pivotPos = itemInfo.Position;
            if (useFromPosAsPivot)
            {
                var oldItemPos = itemInfo.Position;
                newItemPos.x -= xFrom - oldItemPos.x;
                newItemPos.y -= yFrom - oldItemPos.y;
                pivotPos.x = xFrom;
                pivotPos.y = yFrom;
            }

            if (IsEmpty(newItemPos.x, newItemPos.y) || m_itemsMap[newItemPos.x, newItemPos.y] == itemInfo)
            {
                MoveItemToPosInMap(itemInfo, newItemPos);
                var dimensionItemInfo = new DimensionItemInfo(itemInfo.Item, itemInfo.Amount, itemInfo.GetItemSize(), itemInfo.Position);
                OnItemPositionChanged?.Invoke(this, dimensionItemInfo);
            }
            else
            {
                var itemSize = itemInfo.GetItemSize();
                int notAdded = AddToPosInternal(newItemPos.x, newItemPos.y, itemSize.x, itemSize.y, itemInfo.Item, itemInfo.Amount);
                if(notAdded > 0)
                    itemInfo.Amount = notAdded;
            }
        }

        public DimensionItemContainer ItemContainerAt(int index)
        {
            return m_items[index];
        }

        public ItemInfo ItemInfoAt(int itemIndex)
        {
            return new ItemInfo(m_items[itemIndex].Item, m_items[itemIndex].Amount);
        }

        public DimensionItemInfo DimensionItemInfoAt(int itemIndex)
        {
            return new DimensionItemInfo(m_items[itemIndex].Item, m_items[itemIndex].Amount, m_items[itemIndex].GetItemSize(), m_items[itemIndex].Position);
        }

        public DimensionItemInfo DimensionItemInfoAtPos(int x, int y)
        {
            if (m_itemsMap[x, y] == null)
                return new DimensionItemInfo();

            return new DimensionItemInfo(m_itemsMap[x, y].Item, m_itemsMap[x, y].Amount, m_itemsMap[x, y].GetItemSize(), m_itemsMap[x, y].Position);
        }

        public void AddColumns(int count)
        {
            if (count == 0)
                return;

            if(count < 0)
                RemoveColumns(-count);

            m_width += count;
            RebuildItemMap();
            OnSizeChanged?.Invoke(this);
        }

        /// <summary> It will delete items if theirs part was in deleted columns </summary>
        public void RemoveColumns(int count)
        {
            if (count == 0)
                return;

            if (count < 0)
                AddColumns(-count);

            throw new System.NotImplementedException("RemoveColumns is not implemented.");
        }

        public void AddRows(int count)
        {
            if (count == 0)
                return;

            if (count < 0)
                RemoveRows(-count);

            m_height += count;
            RebuildItemMap();
            OnSizeChanged?.Invoke(this);
        }

        /// <summary> It will delete items if theirs part was in deleted rows </summary>
        public void RemoveRows(int count)
        {
            if (count == 0)
                return;

            if (count < 0)
                AddRows(-count);

            throw new System.NotImplementedException("RemoveRows is not implemented.");
        }

        public bool CanAdd(Thing thing, int amount)
        {
            if (!IsPreferable(thing))
                return false;

            var cache = new Caching.DimensionCache(m_itemsMap);

            return CanAddInternal(thing, amount, cache);
        }

        public bool CanAdd(Dictionary<Thing, int> things)
        {
            var cache = new Caching.DimensionCache(m_itemsMap);

            foreach (var thingWithAmount in things)
            {
                bool canAddThing = CanAddInternal(thingWithAmount.Key, thingWithAmount.Value, cache);
                if(!canAddThing)
                    return false;
            }

            return true;
        }

        private bool CanAddInternal(Thing thing, int amount, Caching.DimensionCache cache)
        {
            var itemSize = Vector2Int.one;
            var itemSizeProp = thing.GetPropertyOfType<DimensionItemSize>();
            if (itemSizeProp != null)
                itemSize = new Vector2Int(itemSizeProp.StartWidth, itemSizeProp.StartHeight);

            for (int x = 0; x < m_width; ++x)
            {
                if (m_width - x < itemSize.x)
                    break;

                for (int y = 0; y < m_height; ++y)
                {
                    if (m_height - y < itemSize.y)
                        break;

                    if (cache.IsEmpty(x, y))
                    {
                        bool hasEnoughSpace = true;
                        for (int xPos = 0; xPos < itemSize.x; ++xPos)
                        {
                            for (int yPos = 0; yPos < itemSize.y; ++yPos)
                            {
                                if (!cache.IsEmpty(x + xPos, y + yPos))
                                {
                                    hasEnoughSpace = false;
                                    break;
                                }
                            }

                            if (!hasEnoughSpace)
                                break;
                        }

                        if (hasEnoughSpace)
                        {
                            int addingAmount = Mathf.Min(amount, thing.MaxStackAmount);
                            amount -= addingAmount;
                            cache.AddContainerCache(new Caching.ContainerCache(thing, addingAmount), x, y, itemSize.x, itemSize.y);
                        }
                    }
                    else if (cache.CacheAt(x, y).Thing == thing)
                    {
                        if (cache.CacheAt(x, y).Amount + amount <= thing.MaxStackAmount)
                            return true;
                        else
                        {
                            amount -= thing.MaxStackAmount - cache.CacheAt(x, y).Amount;
                            cache.CacheAt(x, y).Amount = thing.MaxStackAmount;
                        }
                    }

                    if (amount <= 0)
                        return true;
                }
            }

            return amount <= 0;
        }

        public bool CanAddToPos(int x, int y, Thing thing, int amount)
        {
            if (!IsPreferable(thing))
                return false;

            var itemSize = Vector2Int.one;
            var itemSizeProp = thing.GetPropertyOfType<DimensionItemSize>();
            if (itemSizeProp != null)
                itemSize = new Vector2Int(itemSizeProp.StartWidth, itemSizeProp.StartHeight);

            if (!IsPosValid(x + itemSize.x - 1, y + itemSize.y - 1))
                return false;

            if (IsEmpty(x, y))
            {
                for (int xPos = 0; xPos < itemSize.x; ++xPos)
                {
                    for (int yPos = 0; yPos < itemSize.y; ++yPos)
                    {
                        if (!IsEmpty(x + xPos, y + yPos))
                            return false;
                    }
                }

                return amount <= thing.MaxStackAmount;
            }
            else
            {
                if (m_itemsMap[x, y].Item.Thing != thing)
                    return false;

                return m_itemsMap[x, y].Amount + amount <= thing.MaxStackAmount;
            }
        }

        /// <summary> Returns the amount of things that can't be added </summary> 
        public int Add(Item newItem, int amount)
        {
            if (amount <= 0)
                return 0;

            if (newItem == null)
                return amount;

            if (!IsPreferable(newItem.Thing))
                return amount;

            var itemSize = Vector2Int.one;
            var itemSizeProp = newItem.GetProperty<DimensionItemSize>();
            if (itemSizeProp != null)
                itemSize = itemSizeProp.Size;

            for (int x = 0; x < m_width; ++x)
            {
                if (m_width - x < itemSize.x)
                    return amount;

                for (int y = 0; y < m_height; ++y)
                {
                    if (m_height - y < itemSize.y)
                        break;

                    amount = AddToPosInternal(x, y, itemSize.x, itemSize.y, newItem, amount);

                    if (amount <= 0)
                        return 0;
                }
            }

            return amount;
        }

        /// <summary> Returns the amount of things that can't be added </summary> 
        public int AddToPos(int x, int y, Item newItem, int amount)
        {
            if (amount <= 0)
                return 0;

            if (newItem == null)
                return amount;

            if (!IsPreferable(newItem.Thing))
                return amount;

            var itemSize = Vector2Int.one;
            var itemSizeProp = newItem.GetProperty<DimensionItemSize>();
            if (itemSizeProp != null)
                itemSize = itemSizeProp.Size;

            if (!IsPosValid(x + itemSize.x - 1, y + itemSize.y - 1))
                return amount;

            return AddToPosInternal(x, y, itemSize.x, itemSize.y, newItem, amount);
        }

        private int AddToPosInternal(int x, int y, int width, int height, Item newItem, int amount)
        {
            if (IsEmpty(x, y))
            {
                bool hasEnoughSpace = true;
                for (int xPos = 0; xPos < width; ++xPos)
                {
                    for (int yPos = 0; yPos < height; ++yPos)
                    {
                        if (!IsEmpty(x + xPos, y + yPos))
                        {
                            hasEnoughSpace = false;
                            break;
                        }
                    }

                    if (!hasEnoughSpace)
                        break;
                }

                if (hasEnoughSpace)
                {
                    int addingAmount = Mathf.Min(amount, newItem.Thing.MaxStackAmount);
                    amount -= addingAmount;
                    if (amount == 0)
                        CreateAndSetupNewContainer(new Vector2Int(x, y), newItem, addingAmount);
                    else
                        CreateAndSetupNewContainer(new Vector2Int(x, y), newItem.Clone(), addingAmount);

                    return amount;
                }
            }
            else if (m_itemsMap[x, y].Item.Thing == newItem.Thing && m_itemsMap[x, y].Item != newItem)
            {
                if (m_itemsMap[x, y].Amount + amount <= newItem.Thing.MaxStackAmount)
                {
                    newItem.MergeInto(m_itemsMap[x, y].Item, m_itemsMap[x, y].Amount, amount);
                    newItem.DestroyItem();
                    m_itemsMap[x, y].Amount += amount;
                    return 0;
                }
                else
                {
                    int addingAmount = newItem.Thing.MaxStackAmount - m_itemsMap[x, y].Amount;
                    newItem.MergeInto(m_itemsMap[x, y].Item, m_itemsMap[x, y].Amount, addingAmount);
                    amount -= addingAmount;
                    m_itemsMap[x, y].Amount += addingAmount;
                }
            }

            return amount;
        }

        /// <summary> Returns the amount of things that can't be removed because there is not enough amount of things </summary> 
        public int Remove(Thing thing, int amount, bool destroyItems = true)
        {
            if (amount <= 0)
                return 0;

            for (int i = 0; i < m_items.Count; ++i) 
            {
                if (m_items[i].Item.Thing == thing)
                {
                    if (amount < m_items[i].Amount)
                    {
                        m_items[i].Amount -= amount;
                        amount = 0;
                    }
                    else
                    {
                        m_items[i].OnContainerChanged.RemoveListener(OnContainerChanged);
                        amount -= m_items[i].Amount;

                        if (destroyItems)
                            m_items[i].Item.DestroyItem();
                        else
                            m_items[i].Clear();

                        m_items.RemoveAt(i);
                        i--;
                    }
                }

                if (amount <= 0)
                    return 0;
            }

            OnContentChanged?.Invoke(this);

            return amount;
        }

        /// <summary> Returns the amount of things that can't be removed because there is not enough amount of things </summary> 
        public int RemoveFromPos(int x, int y, int amount, bool destroyItems = true)
        {
            if (IsEmpty(x, y))
                return amount;

            if (amount < m_itemsMap[x, y].Amount)
            {
                m_itemsMap[x, y].Amount -= amount;
                OnContentChanged?.Invoke(this);
                return 0;
            }
            else
            {
                int prev = m_itemsMap[x, y].Amount;
                if (destroyItems)
                    m_itemsMap[x, y].Item.DestroyItem();
                else
                    m_itemsMap[x, y].Clear();

                return amount - prev;
            }
        }

        public void ClearAll(bool destroyItems = true)
        {
            for (int i = 0; i < m_items.Count; ++i)
            {
                m_items[i].OnContainerChanged.RemoveListener(OnContainerChanged);
                if (destroyItems)
                    m_items[i].Item.DestroyItem();
                else
                    m_items[i].Clear();
            }

            m_items.Clear();
            RebuildItemMap();
            OnContentChanged?.Invoke(this);
        }

        public bool Contains(Thing thing, int amount)
        {
            int found = 0;
            for(int i = 0; i < m_items.Count;i++)
            {
                if (m_items[i].Item.Thing == thing)
                    found += m_items[i].Amount;

                if(found >= amount)
                    return true;
            }

            return false;
        }

        public int GetAmountOf(Thing thing)
        {
            int found = 0;
            for (int i = 0; i < m_items.Count; i++)
            {
                if (m_items[i].Item.Thing == thing)
                    found += m_items[i].Amount;
            }

            return found;
        }

        /// <summary> Returns Vector2Int(-1, -1) if there is no empty pos</summary> 
        public Vector2Int GetFirstEmptyPos()
        {
            for (int y = 0; y < m_height; ++y)
            {
                for (int x = 0; x < m_width; ++x)
                {
                    if (m_itemsMap[x, y] == null)
                        return new Vector2Int(x, y);
                }
            }

            return new Vector2Int(-1, -1);
        }

        public object GetSerializedData()
        {
            object[] data = new object[c_serializedItemsDataIndexOffset + m_items.Count];
            data[0] = m_width;
            data[1] = m_height;

            for (int i = 0; i < m_items.Count; ++i)
            {
                Dictionary<string, object> slotData = new Dictionary<string, object>();
                slotData.Add(c_itemSaveID, m_items[i].Item.GetSerializedData());
                slotData.Add(c_amountSaveID, m_items[i].Amount);
                slotData.Add(c_positionSaveID, new int[] { m_items[i].Position.x, m_items[i].Position.y });
                data[c_serializedItemsDataIndexOffset + i] = slotData;
            }

            return data;
        }

        public void SetSerializedData(object serializedData)
        {
            if (serializedData == null)
                return;

            ClearAll();

            object[] data = (object[])serializedData;

            m_width = (int)data[0];
            m_height = (int)data[1];

            RebuildItemMap();

            for (int i = c_serializedItemsDataIndexOffset; i < data.Length; i++)
            {
                Dictionary<string, object> slotData = (Dictionary<string, object>)data[i];
                var pos = (int[])slotData[c_positionSaveID];
                var item = Item.Create(slotData[c_itemSaveID]);
                if (item != null)  // could be null if we realeased a new game version where thing was deleted
                    CreateAndSetupNewContainer(new Vector2Int(pos[0], pos[1]), item, (int)slotData[c_amountSaveID]);
            }

            OnContentChanged?.Invoke(this);
        }

        private void OnContainerChanged(IItemContainer container)
        {
            if (container.Item == null)
                ClearAndRemoveContainer((DimensionItemContainer)container);

            OnContentChanged?.Invoke(this);
        }
    }
}

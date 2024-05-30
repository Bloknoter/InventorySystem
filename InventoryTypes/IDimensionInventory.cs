using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InventoryEngine
{
    public interface IDimensionInventory : IInventory
    {
        int Width { get; }
        int Height { get; }

        EventCaller<IDimensionInventory> OnSizeChanged { get; }
        EventCaller<IDimensionInventory, DimensionItemContainer> OnItemContainerAdded { get; }
        EventCaller<IDimensionInventory, DimensionItemContainer> OnItemContainerRemoved { get; }
        EventCaller<IDimensionInventory, DimensionItemInfo> OnItemPositionChanged { get; }

        DimensionItemContainer ItemContainerAt(int index);
        DimensionItemInfo DimensionItemInfoAt(int itemIndex);
        DimensionItemInfo DimensionItemInfoAtPos(int x, int y);

        bool IsEmpty(int x, int y);
        bool IsPosValid(int x, int y);

        bool CanAddToPos(int x, int y, Thing thing, int amount);

        /// <summary> Returns the amount of things that can't be added </summary> 
        int AddToPos(int x, int y, Item newItem, int amount);

        /// <summary> Returns the amount of things that can't be removed because there is not enough amount of things </summary> 
        int RemoveFromPos(int x, int y, int amount, bool destroyItems = true);

        public bool CanMoveItem(int xFrom, int yFrom, int xTo, int yTo, bool useFromPosAsPivot = false);
        public void MoveItem(int xFrom,  int yFrom, int xTo, int yTo, bool useFromPosAsPivot = false);

        void AddColumns(int count);
        void AddRows(int count);

        void RemoveColumns(int count);
        void RemoveRows(int count);

        /// <summary> Returns Vector2Int(-1, -1) if there is no empty pos</summary> 
        Vector2Int GetFirstEmptyPos();
    }

    public struct DimensionItemInfo
    {
        public Item Item;
        public int Amount;
        public Vector2Int Size;
        public Vector2Int Position;

        public DimensionItemInfo(Item item = null, int amount = 0, Vector2Int size = new Vector2Int(), Vector2Int position = new Vector2Int())
        {
            Item = item;
            Amount = amount;
            Size = size;
            Position = position;
        }
    }

    
}

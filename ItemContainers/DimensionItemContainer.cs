using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InventoryEngine
{
    public class DimensionItemContainer : ItemContainerBase
    {
        public EventCaller<DimensionItemContainer> OnPositionChanged { get; private set; } = new EventCaller<DimensionItemContainer>();

        public static readonly Vector2Int DefaultSize = Vector2Int.one;

        private DimensionItemSize m_itemSizeProp;
        private Vector2Int m_position = Vector2Int.zero;

        public DimensionItemSize ItemSizeProp => m_itemSizeProp;
        public Vector2Int Position
        {
            get => m_position;
            set
            {
                var prev = m_position;
                m_position = value;
                if (prev != m_position)
                    OnPositionChanged?.Invoke(this);
            }
        }
        
        public DimensionItemContainer(IDimensionInventory dimensionInventory, Vector2Int position, Item item, int amount) : base(dimensionInventory, item, amount)
        {
            m_itemSizeProp = Item.GetProperty<DimensionItemSize>();
            Position = position;
        }

        public Vector2Int GetItemSize()
        {
            if (ItemSizeProp != null)
                return ItemSizeProp.Size;
            return DefaultSize;
        }

        public override string ToString()
        {
            return $"Item data info\n   Item: {Item.Thing.Name} \n   Amount: {Amount} \n  Position: {Position} \n  Size: {GetItemSize()}";
        }
    }
}

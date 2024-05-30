using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InventoryEngine
{
    [CreateAssetMenu(fileName = "DimensionItemSize", menuName = InventoryCreateAssetMenuPathsGeneral.INVENTORY_SPECIFIC_PROPERTIES_MENU + "/Dimension Item Size", order = 1)]
    public class DimensionItemSize : ThingProperty
    {
        public delegate void SizeChangedListener(Vector2Int newSize);

        public event SizeChangedListener OnSizeChanged;

        [Min(1)]
        [SerializeField]
        private int m_startWidth = 1;

        [Min(1)]
        [SerializeField]
        private int m_startHeight = 1;

        private Vector2Int m_size;

        public int StartWidth => m_startWidth;
        public int StartHeight => m_startHeight;

        public Vector2Int Size
        {
            get => m_size;
            set
            {
                var prev = m_size;
                m_size.x = Mathf.Max(value.x, 1);
                m_size.y = Mathf.Max(value.y, 1);
                if (prev != m_size)
                    OnSizeChanged?.Invoke(m_size);
            }
        }

        public int Width
        {
            get => m_size.x;
            set
            {
                var prev = m_size.x;
                m_size.x = Mathf.Max(value, 1);
                if (prev != m_size.x)
                    OnSizeChanged?.Invoke(m_size);
            }
        }

        public int Height
        {
            get => m_size.y;
            set
            {
                var prev = m_size.y;
                m_size.y = Mathf.Max(value, 1);
                if (prev != m_size.y)
                    OnSizeChanged?.Invoke(m_size);
            }
        }

        protected override string GetUniquePropID()
        {
            return "DimItemSize";
        }

        public override void LoadFromSave(object data)
        {
            var deserializedSize = data as int[];

            if (deserializedSize == null)
                m_size = new Vector2Int(m_startWidth, m_startHeight);
            else
                m_size = new Vector2Int(deserializedSize[0], deserializedSize[1]);
        }

        public override ThingProperty Clone()
        {
            var prop = Instantiate(this);
            prop.m_size = m_size;
            return prop;
        }

        public override void Initialize()
        {
            base.Initialize();

            if (IsCreatedAsNew)
                m_size = new Vector2Int(m_startWidth, m_startHeight);
        }

        public override object CreateDataForSave()
        {
            return new int[] { m_size.x, m_size.y };
        }
    }
}

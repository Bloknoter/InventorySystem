using System.Collections;
using System.Collections.Generic;

using InventoryEngine.Transitions;

using UnityEngine;

namespace InventoryEngine.GUI
{
    public class DimensionInventoryDisplay : MonoBehaviour
    {
        public enum ItemsPivots
        {
            LeftDown, RightDown, LeftTop, RightTop,
        }

        [SerializeField]
        private ItemContainerDisplay m_itemPrefab;

        [SerializeField]
        private RectTransform m_contentArea;

        [SerializeField]
        private ItemsPivots m_itemsPivot;

        private IDimensionInventory m_inventory;

        private Pooling.GameObjectWithComponentPool<ItemContainerDisplay> m_itemsPool;

        private Vector2 m_cellSize;
        private Vector2 m_itemsPivotPos;
        private int m_horAxisDirection = 1;
        private int m_verAxisDirection = 1;

        private List<IDimensionInvAdditionalContent> m_invAdditionalContents;

        public ItemsPivots ItemsPivot => m_itemsPivot;
        public Vector2 CellSize => m_cellSize;
        public Vector2 ItemsPivotPos => m_itemsPivotPos;
        public int HorItemsAxisDirection => m_horAxisDirection;
        public int VerItemsAxisDirection => m_verAxisDirection;

        public void Initialize(IDimensionInventory dimensionInventory) 
        {
            if (m_invAdditionalContents == null)
            {
                m_invAdditionalContents = new List<IDimensionInvAdditionalContent>();
                GetComponents(m_invAdditionalContents);
            }

            if (m_itemsPool == null)
                m_itemsPool = new Pooling.GameObjectWithComponentPool<ItemContainerDisplay>(m_itemPrefab, m_contentArea);

            m_inventory = dimensionInventory;
            m_inventory.OnItemContainerAdded.AddListener(OnItemContainerAdded);
            m_inventory.OnItemContainerRemoved.AddListener(OnItemContainerRemoved);
            m_inventory.OnItemPositionChanged.AddListener(OnItemPositionChanged);

            m_cellSize = new Vector2(m_contentArea.rect.width / m_inventory.Width, m_contentArea.rect.height / m_inventory.Height);
            m_itemsPivotPos = GetPivot(m_itemsPivot);

            CalculateAxesDirections();

            for (int i = 0; i < m_invAdditionalContents.Count; ++i)
                m_invAdditionalContents[i].Initialize(this, m_inventory);

            for (int i = 0; i < m_inventory.ItemsCount; ++i)
            {
                CreateAndSetupItemContainerDisplay(m_inventory.ItemContainerAt(i));
            }
        }

        private void CalculateAxesDirections()
        {
            if (m_itemsPivot == ItemsPivots.LeftDown || m_itemsPivot == ItemsPivots.LeftTop)
                m_horAxisDirection = 1;
            else
                m_horAxisDirection = -1;

            if (m_itemsPivot == ItemsPivots.LeftDown || m_itemsPivot == ItemsPivots.RightDown)
                m_verAxisDirection = 1;
            else
                m_verAxisDirection = -1;
        }

        private Vector2 GetPivot(ItemsPivots pivot)
        {
            switch (pivot)
            {
                case ItemsPivots.LeftDown:
                    return new Vector2(0, 0);

                case ItemsPivots.RightDown:
                    return new Vector2(1, 0);

                case ItemsPivots.LeftTop:
                    return new Vector2(0, 1);

                case ItemsPivots.RightTop:
                    return new Vector2(1, 1);

                default:
                    Debug.LogError($"Unhadled type of pivot '{pivot}'. Default pivot returned ({ItemsPivots.LeftDown})");
                    return new Vector2(0, 0);
            }
        }

        private void CreateAndSetupItemContainerDisplay(DimensionItemContainer itemContainer)
        {
            var itemDisplay = m_itemsPool.GetObject();

            var rectTransform = itemDisplay.GetComponent<RectTransform>();
            rectTransform.pivot = m_itemsPivotPos;
            rectTransform.anchorMin = m_itemsPivotPos;
            rectTransform.anchorMax = m_itemsPivotPos;

            UpdateItemDisplayPos(rectTransform, itemContainer.Position);

            var itemSize = itemContainer.GetItemSize();
            rectTransform.sizeDelta = new Vector2(itemSize.x * m_cellSize.x, itemSize.y * m_cellSize.y);

            itemDisplay.Initialize(itemContainer);

            for (int i = 0; i < m_invAdditionalContents.Count; ++i)
                m_invAdditionalContents[i].OnItemContainerAdded(itemDisplay);
        }

        private void UpdateItemDisplayPos(RectTransform containerTranform, Vector2Int inventoryPos)
        {
            var localPos = new Vector2(inventoryPos.x * m_cellSize.x * m_horAxisDirection, inventoryPos.y * m_cellSize.y * m_verAxisDirection);
            containerTranform.anchoredPosition = localPos;
        }

        private void OnItemContainerAdded(IDimensionInventory dimensionInventory, DimensionItemContainer itemContainer)
        {
            CreateAndSetupItemContainerDisplay(itemContainer);
        }

        private void OnItemContainerRemoved(IDimensionInventory dimensionInventory, DimensionItemContainer itemContainer)
        {
            for(int i = 0; i < m_itemsPool.ActiveObjects.Count; ++i)
            {
                var containerDisplay = m_itemsPool.ActiveObjects[i];

                if (containerDisplay.ItemContainer == itemContainer)
                {
                    containerDisplay.Clear();

                    for (int c = 0; c < m_invAdditionalContents.Count; ++c)
                        m_invAdditionalContents[c].OnItemContainerRemoved(containerDisplay);

                    m_itemsPool.ReturnObjectToPool(containerDisplay.gameObject);
                    return;
                }
            }
        }

        private void OnItemPositionChanged(IDimensionInventory inventory, DimensionItemInfo dimensionItemInfo)
        {
            foreach(var containerDisplay in m_itemsPool.ActiveObjects)
            {
                if(containerDisplay.ItemContainer.Item == dimensionItemInfo.Item)
                {
                    UpdateItemDisplayPos(containerDisplay.GetComponent<RectTransform>(), dimensionItemInfo.Position);
                    break;
                }
            }
        }

        private void ClearItemSlots()
        {
            for (int i = 0; i < m_itemsPool.ActiveObjects.Count; ++i)
            {
                m_itemsPool.ActiveObjects[i].Clear();

                for (int c = 0; c < m_invAdditionalContents.Count; ++c)
                    m_invAdditionalContents[c].OnItemContainerRemoved(m_itemsPool.ActiveObjects[i]);
            }

            m_itemsPool.ReturnAllObjectsToPool();
        }

        public void Clear()
        {
            ClearItemSlots();

            m_inventory.OnItemContainerAdded.RemoveListener(OnItemContainerAdded);
            m_inventory.OnItemContainerRemoved.RemoveListener(OnItemContainerRemoved);
            m_inventory.OnItemPositionChanged.RemoveListener(OnItemPositionChanged);

            for (int i = 0; i < m_invAdditionalContents.Count; ++i)
                m_invAdditionalContents[i].Clear();
        }
    }
}
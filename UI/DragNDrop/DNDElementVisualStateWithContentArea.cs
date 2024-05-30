using System.Collections;
using System.Collections.Generic;

using InventoryEngine.Transitions;

using UnityEngine;


namespace InventoryEngine.GUI
{
    public class DNDElementVisualStateWithContentArea : DNDElementVisualStateControllerBase
    {
        [SerializeField]
        private DimensionInventoryDisplay m_inventoryDisplay;

        [SerializeField]
        private RectTransform m_contentArea;

        [SerializeField]
        private RectTransform m_canDrop;

        [SerializeField]
        private RectTransform m_cannotDrop;

        public override void ShowDragNDropState(bool canDrop, object locationData, ItemToTransitData itemToTransitData)
        {
            m_canDrop.gameObject.SetActive(canDrop);
            m_cannotDrop.gameObject.SetActive(!canDrop);

            RectTransform visualization = null;
            if (canDrop)
                visualization = m_canDrop;
            else
                visualization = m_cannotDrop;

            visualization.pivot = m_inventoryDisplay.ItemsPivotPos;
            visualization.anchorMin = m_inventoryDisplay.ItemsPivotPos;
            visualization.anchorMax = m_inventoryDisplay.ItemsPivotPos;

            var dimensionLocationData = (DimensionItemLocationData)locationData;
            

            var localPosX = dimensionLocationData.Position.x * m_inventoryDisplay.CellSize.x * m_inventoryDisplay.HorItemsAxisDirection;
            var localPosY = dimensionLocationData.Position.y * m_inventoryDisplay.CellSize.y * m_inventoryDisplay.VerItemsAxisDirection;

            var deltaFromCenter = new Vector3(m_contentArea.rect.width / 2 * -m_inventoryDisplay.HorItemsAxisDirection,
                m_contentArea.rect.height / 2 * -m_inventoryDisplay.VerItemsAxisDirection, 0);
            var worldCornerPos = m_contentArea.position + deltaFromCenter;

            visualization.position = worldCornerPos + new Vector3(localPosX, localPosY, 0);

            var itemSizeProp = itemToTransitData.Item.GetProperty<DimensionItemSize>();
            var itemSize = DimensionItemContainer.DefaultSize;
            if (itemSizeProp != null)
                itemSize = itemSizeProp.Size;

            visualization.sizeDelta = new Vector2(itemSize.x * m_inventoryDisplay.CellSize.x, itemSize.y * m_inventoryDisplay.CellSize.y);
        }

        public override void HideDragNDropState()
        {
            m_canDrop.gameObject.SetActive(false);
            m_cannotDrop.gameObject.SetActive(false);
        }

        public override void HideContent()
        {
            
        }

        public override void ShowContent()
        {
            
        }
    }
}

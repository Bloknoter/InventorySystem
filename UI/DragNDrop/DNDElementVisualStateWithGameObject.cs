using System.Collections;
using System.Collections.Generic;

using InventoryEngine.Transitions;

using UnityEngine;


namespace InventoryEngine.GUI
{
    public class DNDElementVisualStateWithGameObject : DNDElementVisualStateControllerBase
    {
        [SerializeField]
        private CanvasGroup m_contentRoot;

        [SerializeField]
        private GameObject m_canDropVis;

        [SerializeField]
        private GameObject m_cannotDropVis;

        public override void ShowDragNDropState(bool canDrop, object locationData, ItemToTransitData itemToTransitData)
        {
            m_canDropVis.SetActive(canDrop);
            m_cannotDropVis.SetActive(!canDrop);
        }

        public override void HideDragNDropState()
        {
            m_canDropVis.SetActive(false);
            m_cannotDropVis.SetActive(false);
        }

        public override void HideContent()
        {
            m_contentRoot.alpha = 0.0f;
        }

        public override void ShowContent()
        {
            m_contentRoot.alpha = 1.0f;
        }
    }
}

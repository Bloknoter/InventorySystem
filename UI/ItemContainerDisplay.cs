using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;


namespace InventoryEngine.GUI
{
    public class ItemContainerDisplay : MonoBehaviour
    {
        public delegate void DisplayPointerEventListener(ItemContainerDisplay display, PointerEventData eventData);

        public event DisplayPointerEventListener PointerEnter;

        [SerializeField]
        private Image m_icon;

        [SerializeField]
        private bool m_enabeAutoScale;

        [ShowWhen("m_enabeAutoScale", true)]
        [SerializeField]
        private Vector2 m_padding;

        [ShowWhen("m_enabeAutoScale", true)]
        [SerializeField]
        private Vector2 m_maxIconSize;

        [Space(10)]
        [SerializeField]
        private TextMeshProUGUI m_amount;

        [SerializeField]
        private bool m_showAmountForStackableIfOne = true;

        [SerializeField]
        private bool m_showAmountForNonStackable = false;

        [Space(10)]
        [SerializeField]
        private GameObject EmptyVis;

        private RectTransform m_myRectTransform;
        private RectTransform m_iconRectTransform;

        private IItemContainer m_container;
        private Item m_currentItem;

        private List<ItemPropDisplayBase> m_propsDisplayers = new List<ItemPropDisplayBase>(2);

        public IItemContainer ItemContainer => m_container;

        public Vector2 VisualItemSize => m_iconRectTransform.rect.size;

        public void Initialize(IItemContainer itemContainer)
        {
            if (m_myRectTransform == null)
                m_myRectTransform = GetComponent<RectTransform>();
            if (m_iconRectTransform == null)
                m_iconRectTransform = m_icon.GetComponent<RectTransform>();

            m_container = itemContainer;

            m_container.OnContainerChanged.AddListener(OnContainerChanged);
            OnContainerChanged(itemContainer);
        }

        private void OnContainerChanged(IItemContainer itemContainer)
        {
            if (m_container.Item != null)
            {
                m_icon.gameObject.SetActive(true);
                SetupIcon(m_container.Item.Thing.Icon);

                var amount = itemContainer.Amount;
                var isStackable = itemContainer.Item.Thing.IsStackable;

                m_amount.gameObject.SetActive(isStackable && (amount > 1 || m_showAmountForStackableIfOne) || !isStackable && m_showAmountForNonStackable);
                m_amount.SetText(amount.ToString());

                if (m_currentItem == null)
                {
                    LoadAndSetupPropDisplayers();
                }
                else if (m_currentItem != m_container.Item)
                {
                    ClearPropDisplayers();
                    LoadAndSetupPropDisplayers();
                }

                m_currentItem = m_container.Item;

                if (EmptyVis != null)
                    EmptyVis.gameObject.SetActive(false);
            }
            else
            {
                m_icon.gameObject.SetActive(false);
                m_amount.gameObject.SetActive(false);

                ClearPropDisplayers();
                m_currentItem = null;

                if (EmptyVis != null)
                    EmptyVis.gameObject.SetActive(true);
            }
        }

        private void SetupIcon(Sprite sprite)
        {
            m_icon.sprite = sprite;

            if (!m_enabeAutoScale)
                return;

            var displaySize = m_myRectTransform.rect.size;
            var maxPossibleSize = new Vector2(displaySize.x - m_padding.x * 2, displaySize.y - m_padding.y * 2);
            if (maxPossibleSize.x <= 0 || maxPossibleSize.y <= 0)
            {
                Debug.LogError($"Somehow max possible size of icon has negative or zero components -> [{maxPossibleSize.x}, {maxPossibleSize.y}]");
                return;
            }

            var iconMaxSize = new Vector2(Mathf.Min(m_maxIconSize.x, maxPossibleSize.x), Mathf.Min(m_maxIconSize.y, maxPossibleSize.y));
            if (iconMaxSize.x <= 0 || iconMaxSize.y <= 0)
            {
                Debug.LogError($"Somehow calculated icon max size has negative or zero components -> [{iconMaxSize.x}, {iconMaxSize.y}]");
                return;
            }

            var spriteSizeInPix = sprite.rect.size;
            var scaleFactor = Vector2.one;
            if (iconMaxSize.x / iconMaxSize.y > spriteSizeInPix.x / spriteSizeInPix.y)
            {
                scaleFactor.y = iconMaxSize.y / spriteSizeInPix.y;
                scaleFactor.x = scaleFactor.y;
            }
            else
            {
                scaleFactor.x = iconMaxSize.x / spriteSizeInPix.x;
                scaleFactor.y = scaleFactor.x;
            }

            m_iconRectTransform.sizeDelta = spriteSizeInPix * scaleFactor;
        }

        private void LoadAndSetupPropDisplayers()
        {
            var displayersListProp = m_container.Item.GetProperty<ItemPropDisplayers>();
            if (displayersListProp == null)
                return;

            for (int i = 0; i < displayersListProp.DisplayersPaths.Count; ++i)
            {
                var path = displayersListProp.DisplayersPaths[i];
                var propDisplayObject = Instantiate(Resources.Load<GameObject>(path), m_myRectTransform);
                var rectTransform = propDisplayObject.GetComponent<RectTransform>();

                rectTransform.anchorMin = Vector3.zero;
                rectTransform.anchorMax = Vector3.one;
                rectTransform.offsetMin = Vector3.one;
                rectTransform.offsetMax = Vector3.one;

                var propDisplay = propDisplayObject.GetComponent<ItemPropDisplayBase>();
                propDisplay.Initialize(m_container);
            }
        }

        private void ClearPropDisplayers()
        {
            for (int i = 0; i < m_propsDisplayers.Count; ++i)
            {
                m_propsDisplayers[i].Clear();
            }

            m_propsDisplayers.Clear();
        }

        public void Clear()
        {
            if (m_container != null)
            {
                m_container.OnContainerChanged.RemoveListener(OnContainerChanged);
                ClearPropDisplayers();
                m_container = null;
                m_currentItem = null;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using InventoryEngine.Transitions;


namespace InventoryEngine.GUI
{
    public class SlotInventoryDisplay : MonoBehaviour
    {
        [SerializeField]
        private List<ItemContainerDisplay> m_readyToUseSlots;

        [SerializeField]
        private ItemContainerDisplay m_itemPrefab;

        [SerializeField]
        private RectTransform m_slotsParent;

        private ISlotInventory m_inventory;

        private Pooling.GameObjectWithComponentPool<ItemContainerDisplay> m_itemsPool;

        private List<ISlotInvAdditionalContent> m_invAdditionalContents;

        public void Initialize(ISlotInventory slotInventory)
        {
            if (m_itemsPool == null && m_itemPrefab != null)
                m_itemsPool = new Pooling.GameObjectWithComponentPool<ItemContainerDisplay>(m_itemPrefab, m_slotsParent);

            if(m_invAdditionalContents == null)
            {
                m_invAdditionalContents = new List<ISlotInvAdditionalContent>();
                GetComponents(m_invAdditionalContents);
            }    

            m_inventory = slotInventory;
            m_inventory.OnSlotAdded.AddListener(OnSlotAdded);
            m_inventory.OnSlotRemoved.AddListener(OnSlotRemoved);

            for (int c = 0; c < m_invAdditionalContents.Count; ++c)
                m_invAdditionalContents[c].Initialize(this, m_inventory);

            SetupInventoryVisualization(m_inventory);
        }

        private void SetupInventoryVisualization(ISlotInventory slotInventory) 
        {
            for (int i = 0; i < m_readyToUseSlots.Count; ++i)
            {
                if (i < slotInventory.Size)
                {
                    m_readyToUseSlots[i].gameObject.SetActive(true);
                    m_readyToUseSlots[i].Initialize(slotInventory.SlotAt(i));

                    for (int c = 0; c < m_invAdditionalContents.Count; ++c)
                        m_invAdditionalContents[c].OnSlotAdded(m_readyToUseSlots[i]);
                }
                else
                    m_readyToUseSlots[i].gameObject.SetActive(false);
            }

            if(m_readyToUseSlots.Count < slotInventory.Size)
            {
                for (int i = 0; i < slotInventory.Size - m_readyToUseSlots.Count; ++i)
                {
                    SetupNewItemContainerDisplay(slotInventory.SlotAt(m_readyToUseSlots.Count + i));
                }
            }
        }

        private void SetupNewItemContainerDisplay(Slot slot)
        {
            var itemDisplay = m_itemsPool.GetObject();
            itemDisplay.Initialize(slot);

            for (int c = 0; c < m_invAdditionalContents.Count; ++c)
                m_invAdditionalContents[c].OnSlotAdded(itemDisplay);
        }

        private void OnSlotAdded(ISlotInventory slotInventory, Slot slot)
        {
            SetupNewItemContainerDisplay(slot);
        }

        private void OnSlotRemoved(ISlotInventory slotInventory, Slot slot)
        {
            for (int i = 0; i < m_readyToUseSlots.Count; ++i)
            {
                if (m_readyToUseSlots[i].ItemContainer == slot)
                {
                    m_readyToUseSlots[i].Clear();
                    m_readyToUseSlots[i].gameObject.SetActive(false);

                    for (int c = 0; c < m_invAdditionalContents.Count; ++c)
                        m_invAdditionalContents[c].OnSlotRemoved(m_readyToUseSlots[i]);

                    return;
                }
            }

            if (m_itemsPool == null)
                return;

            for (int i = 0; i < m_itemsPool.ActiveObjects.Count; ++i)
            {
                if (m_readyToUseSlots[i].ItemContainer == slot)
                {
                    m_itemsPool.ActiveObjects[i].Clear();
                    m_itemsPool.ReturnObjectToPool(m_itemsPool.ActiveObjects[i].gameObject);

                    for (int c = 0; c < m_invAdditionalContents.Count; ++c)
                        m_invAdditionalContents[c].OnSlotRemoved(m_itemsPool.ActiveObjects[i]);

                    return;
                }
            }
        }

        private void ClearItemSlots()
        {
            for(int i = 0; i < m_readyToUseSlots.Count; ++i)
            {
                m_readyToUseSlots[i].Clear();
                m_readyToUseSlots[i].gameObject.SetActive(false);

                for (int c = 0; c < m_invAdditionalContents.Count; ++c)
                    m_invAdditionalContents[c].OnSlotRemoved(m_readyToUseSlots[i]);
            }

            if (m_itemsPool == null)
                return;

            for (int i = 0; i < m_itemsPool.ActiveObjects.Count; ++i)
            {
                m_itemsPool.ActiveObjects[i].Clear();

                for (int c = 0; c < m_invAdditionalContents.Count; ++c)
                    m_invAdditionalContents[c].OnSlotRemoved(m_itemsPool.ActiveObjects[i]);
            }

            m_itemsPool.ReturnAllObjectsToPool();
        }

        public void Clear()
        {
            if (m_inventory != null)
            {
                m_inventory.OnSlotAdded.RemoveListener(OnSlotAdded);
                m_inventory.OnSlotRemoved.RemoveListener(OnSlotRemoved);

                ClearItemSlots();
                m_inventory = null;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using InventoryEngine;
using InventoryEngine.Transitions;



namespace InventoryEngine.GUI
{
    public class DragNDropDraggableElementVis : MonoBehaviour
    {
        [SerializeField]
        private Image m_draggableItemIcon;

        private DragNDropSystem m_dragNDropSystem;

        public void SetupDragNDrop(DragNDropSystem dragNDropSystem)
        {
            m_dragNDropSystem = dragNDropSystem;

            m_dragNDropSystem.Started += OnDragNDropStarted;
            m_dragNDropSystem.Finished += OnDragNDropFinished;
        }

        private void OnDragNDropStarted()
        {
            GlobalScene.Instance.TickManager.AddObjectToTick(UpdateTick, TickManager.TickTypes.Update);

            m_draggableItemIcon.sprite = m_dragNDropSystem.DraggingData.Item.Thing.Icon;

            UpdateItemIconRect();
            m_draggableItemIcon.gameObject.SetActive(true);
        }

        private void UpdateTick(float delta)
        {
            UpdateItemIconRect();
        }

        private void UpdateItemIconRect()
        {
            m_draggableItemIcon.rectTransform.position = m_dragNDropSystem.DraggingData.VisualPosition;
            m_draggableItemIcon.rectTransform.sizeDelta = m_dragNDropSystem.DraggingData.VisualSize;
        }

        private void OnDragNDropFinished(bool succeed) 
        {
            GlobalScene.Instance.TickManager.RemoveObjectToTick(UpdateTick);

            m_draggableItemIcon.gameObject.SetActive(false);
        }

        public void Clear()
        {
            m_dragNDropSystem.Started -= OnDragNDropStarted;
            m_dragNDropSystem.Finished -= OnDragNDropFinished;

            m_dragNDropSystem = null;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InventoryEngine
{
    [CreateAssetMenu(fileName = "Durability", menuName = InventoryCreateAssetMenuPathsGeneral.GENERAL_PROPERTIES_MENU + "/Durability", order = 1)]
    public class Durability : ThingProperty
    {
        public delegate void DurabilityChangedListener(float newvalue);

        public event DurabilityChangedListener OnDurabilityChanged;

        [SerializeField]
        private int m_startDurability;

        [SerializeField]
        private int m_maxDurability;

        [SerializeField]
        private bool m_destroyItemOnZeroDurability = true;

        private float m_durability;

        protected override string GetUniquePropID()
        {
            return "Durability";
        }

        public float Value
        {
            get => m_durability;
            set
            {
                float prevStrength = m_durability;
                m_durability = Mathf.Clamp(value, 0, m_maxDurability);

                if (prevStrength != m_durability)
                    OnDurabilityChanged?.Invoke(m_durability);

                if (m_durability <= 0 && !Item.IsDestroyed && m_destroyItemOnZeroDurability)
                    DestroyItem();
            }
        }

        public override object CreateDataForSave()
        {
            return m_durability;
        }

        public override void LoadFromSave(object data)
        {
            if(data is float durability)
                m_durability = durability;
            else
                m_durability = 0;
        }

        public override void Initialize()
        {
            base.Initialize();

            if (IsCreatedAsNew)
            {
                m_durability = m_startDurability;
            }
        }

        public override void MergeInto(ThingProperty thingProperty, int baseAmount, int mergingAmount)
        {
            var other = (Durability)thingProperty;
            var totalDurability = baseAmount * m_durability + mergingAmount * other.m_durability;
            other.Value = totalDurability / (baseAmount + mergingAmount);
        }

        private void DestroyItem()
        {
           Item.DestroyItem();
        }
    }
}

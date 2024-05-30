using System.Collections.Generic;
using UnityEngine;

namespace InventoryEngine
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "NewThing", menuName = InventoryCreateAssetMenuPathsGeneral.INVENTORY_MENU + "/Thing", order = 0)]
    public class Thing : ScriptableObject
    {
        [SerializeField]
        private string m_uniqueID;

        [SerializeField]
        private string m_thingName;

        [SerializeField]
        private string m_thingDescription;

        [SerializeField]
        private Sprite m_icon;

        [SerializeField]
        [Min(1)]
        private int m_maxAmountInSlote = 20;

        [SerializeField]
        private ThingProperty[] m_thingProperties;

        public string UniqueID => m_uniqueID;

        public string Name => m_thingName;
        public string Desc => m_thingDescription;

        public Sprite Icon => m_icon;

        public int MaxStackAmount => m_maxAmountInSlote;

        public ThingProperty[] Properties => m_thingProperties;

        public bool IsStackable => m_maxAmountInSlote > 1;

        public T GetPropertyOfType<T>() where T : ThingProperty
        {
            for (int i = 0; i < m_thingProperties.Length; i++)
            {
                if (m_thingProperties[i] is T)
                    return (T)m_thingProperties[i];
            }
            return null;
        }
    }
}

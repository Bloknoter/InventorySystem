using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InventoryEngine
{
    [CreateAssetMenu(fileName = "WorldCollectable", menuName = InventoryCreateAssetMenuPathsGeneral.GENERAL_PROPERTIES_MENU + "/World Collectable", order = 1)]
    public class WorldCollectable : ThingProperty
    {
        [SerializeField]
        private bool m_isSpecificPrefab;

        [SerializeField]
        private GameObject m_prefab;

        public GameObject Prefab => m_prefab;

        public bool IsSpecificPrefab => m_isSpecificPrefab;

        protected override string GetUniquePropID()
        {
            return "worldColl";
        }
    }
}

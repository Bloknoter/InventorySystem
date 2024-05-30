using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using UnityEngine;


namespace InventoryEngine
{
    [CreateAssetMenu(fileName = "ItemPropDisplayers", menuName = InventoryCreateAssetMenuPathsGeneral.GENERAL_PROPERTIES_MENU + "/Item Prop Displayers", order = 1)]
    public class ItemPropDisplayers : ThingProperty
    {
        [SerializeField]
        private List<string> m_displayersPaths;

        public ReadOnlyCollection<string> DisplayersPaths;

        protected override string GetUniquePropID()
        {
            return "ItemPropDis";
        }

        public override void Initialize()
        {
            base.Initialize();

            DisplayersPaths = new ReadOnlyCollection<string>(m_displayersPaths);
        }
    }
}

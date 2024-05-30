using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InventoryEngine
{
    [CreateAssetMenu(fileName = "Usable", menuName = InventoryCreateAssetMenuPathsGeneral.GENERAL_PROPERTIES_MENU + "/Usable", order = 1)]
    public class Usable : ThingProperty
    {
        protected override string GetUniquePropID()
        {
            return "Usable";
        }
    }
}

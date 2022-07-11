using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ThingEngine
{
    [CreateAssetMenu(fileName = "Fuel", menuName = "Properties/Other/New fuel property", order = 0)]
    [System.Serializable]
    public class Fuel : ThingProperty
    {
        [SerializeField]
        [Min(0)]
        private int fuelvalue;
        
        public int FuelValue { get { return fuelvalue; } }

        public override ThingProperty GetCopy()
        {
            Fuel prop = CreateInstance<Fuel>();
            prop.fuelvalue = fuelvalue;
            return prop;
        }
    }
}

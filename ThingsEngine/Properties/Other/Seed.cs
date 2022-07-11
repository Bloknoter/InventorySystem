using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ThingEngine
{
    [CreateAssetMenu(fileName = "Seed", menuName = "Properties/Other/New seed property", order = 0)]
    public class Seed : ThingProperty
    {
        [SerializeField]
        private Thing product;

        public Thing Product { get { return product; } }

        public override ThingProperty GetCopy()
        {
            Seed prop = CreateInstance<Seed>();
            prop.product = product;
            return prop;
        }
    }
}

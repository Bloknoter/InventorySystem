using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ThingEngine
{
    [CreateAssetMenu(fileName = "Bullet", menuName = "Properties/Bullets/New bullet property")]
    public class Bullet : ThingProperty
    {
        public override ThingProperty GetCopy()
        {
            Bullet property = CreateInstance<Bullet>();
            return property;
        }
    }
}

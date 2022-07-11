using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ThingEngine
{
    [CreateAssetMenu(fileName = "LiquidsContainer", menuName = "Properties/Other/New liquidscontainer property", order = 1)]
    public class LiquidsContainer : ThingProperty
    {
        public override ThingProperty GetCopy()
        {
            LiquidsContainer prop = CreateInstance<LiquidsContainer>();
            return prop;
        }
    }
}

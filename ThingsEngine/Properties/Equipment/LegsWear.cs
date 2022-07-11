using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ThingEngine
{
    [CreateAssetMenu(fileName = "LegsWear", menuName = "Properties/Equipment/New legs wear", order = 2)]
    [System.Serializable]
    public class LegsWear : Equipment
    {
        [SerializeField]
        private Sprite legsImage;

        public Sprite LegsImage { get { return legsImage; } }

        protected override Equipment GetEquipmentCopy()
        {
            LegsWear property = CreateInstance<LegsWear>();
            property.legsImage = legsImage;
            return property;
        }
    }
}

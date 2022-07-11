using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ThingEngine
{
    [CreateAssetMenu(fileName = "HeadWear", menuName = "Properties/Equipment/New head wear", order = 0)]
    [System.Serializable]
    public class HeadWear : Equipment
    {
        [SerializeField]
        private Sprite headImage;

        public Sprite HeadImage { get { return headImage; } }

        protected override Equipment GetEquipmentCopy()
        {
            HeadWear property = CreateInstance<HeadWear>();
            property.headImage = headImage;
            return property;
        }
    }
}

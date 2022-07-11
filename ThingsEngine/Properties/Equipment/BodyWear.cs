using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ThingEngine
{
    [CreateAssetMenu(fileName = "BodyWear", menuName = "Properties/Equipment/New body wear", order = 1)]
    [System.Serializable]
    public class BodyWear : Equipment
    {
        [SerializeField]
        private Sprite bodyImage;

        public Sprite BodyImage { get { return bodyImage; } }

        protected override Equipment GetEquipmentCopy()
        {
            BodyWear property = CreateInstance<BodyWear>();
            property.bodyImage = bodyImage;
            return property;
        }
    }
}

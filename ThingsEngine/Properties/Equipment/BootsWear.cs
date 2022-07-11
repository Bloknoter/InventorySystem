using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ThingEngine
{
    [CreateAssetMenu(fileName = "BootsWear", menuName = "Properties/Equipment/New boots wear", order = 3)]
    [System.Serializable]
    public class BootsWear : Equipment
    {
        [SerializeField]
        private Sprite bootsImage;

        public Sprite BootsImage { get { return bootsImage; } }

        protected override Equipment GetEquipmentCopy()
        {
            BootsWear property = CreateInstance<BootsWear>();
            property.bootsImage = bootsImage;
            return property;
        }
    }
}

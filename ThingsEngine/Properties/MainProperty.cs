using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using InventoryEngine;

namespace ThingEngine
{
    [CreateAssetMenu(fileName ="Main Property", menuName ="Properties/New main property", order = 1)]
    [System.Serializable]
    public class MainProperty : ThingProperty
    {
        public delegate void OnStrengthChangedDelegate(float newvalue);

        public event OnStrengthChangedDelegate OnStrengthChanged;

        public const float MAX_STRENGTH = 100;

        [Range(0, MAX_STRENGTH)]
        public float StartStrength = MAX_STRENGTH;

        [SerializeField]
        private bool isstatic = true;
        
        /// <summary>
        /// Is strength changing during the game?
        /// </summary>
        public bool IsStatic { get { return isstatic; } }

        [HideInInspector]
        public float Strength
        {
            get { return strength; }
            set
            {
                if (!isstatic)
                {
                    float prevStrength = strength;
                    strength = Mathf.Clamp(value, 0, MAX_STRENGTH);
                    if (prevStrength != strength)
                        OnStrengthChanged?.Invoke(strength);
                    if (strength == 0)
                    {
                        if (prevStrength != 0)
                            DestroyItem();
                    }
                }
            }
        }

        private float strength = 100;

        private Item item;

        public Item Item
        {
            get { return item; }
            set
            {
                if(item == null)
                {
                    item = value;
                }
            }
        }

        public override ThingProperty GetCopy()
        {
            MainProperty property = CreateInstance<MainProperty>();
            property.Strength = StartStrength;
            property.isstatic = isstatic;
            return property;
        }

        private void DestroyItem()
        {
            item.DestroyItem();
        }

        /*public override bool MustBeCreated()
        {
            return true;
        }*/
    }
}

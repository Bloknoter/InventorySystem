using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventoryEngine
{
    [CreateAssetMenu(fileName = "MainProperty", menuName = "Inventory/Properties/Main property", order = 1)]
    [System.Serializable]
    public class MainProperty : ThingProperty
    {
        public const float MAX_STRENGTH = 100;

        public delegate void OnStrengthChangedDelegate(float newvalue);

        public event OnStrengthChangedDelegate OnStrengthChanged;

        [Range(0, MAX_STRENGTH)]
        [SerializeField]
        private float _startStrength = MAX_STRENGTH;

        [SerializeField]
        private bool _isStatic = true;

        private float _strength = 100;

        private Item _item;


        public float Strength
        {
            get { return _strength; }
            set
            {
                if (!_isStatic)
                {
                    float prevStrength = _strength;
                    _strength = Mathf.Clamp(value, 0, MAX_STRENGTH);
                    if (prevStrength != _strength)
                        OnStrengthChanged?.Invoke(_strength);
                    if (_strength == 0)
                    {
                        if (prevStrength != 0)
                            DestroyItem();
                    }
                }
            }
        }

        public Item Item
        {
            get => _item;
            set
            {
                if (_item == null)
                {
                    _item = value;
                }
            }
        }

        /// <summary>
        /// Is strength changing during the game?
        /// </summary>
        public bool IsStrengthStatic => _isStatic;

        private void DestroyItem()
        {
            _item.DestroyItem();
        }
    }
}

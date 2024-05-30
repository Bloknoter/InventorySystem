using System.Collections.Generic;
using UnityEngine;

namespace InventoryEngine
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "NewThing", menuName = "Inventory/Thing", order = 0)]
    public class Thing : ScriptableObject
    {
        [SerializeField]
        private string _thingName;

        [SerializeField]
        private Sprite _image;

        [SerializeField]
        [Min(1)]
        private int _maxAmountInSlote = 20;

        [SerializeField]
        private MainProperty _mainProperty;

        [SerializeField]
        private ThingProperty[] _thingProperties;

        public string Name => _thingName;

        public Sprite Image => _image;

        public int MaxAmountInSlote => _maxAmountInSlote;

        public MainProperty MainProperty => _mainProperty;

        public ThingProperty[] Properties => _thingProperties;

        public string SavingID => name;

        public bool IsStacking => _maxAmountInSlote > 1;

        public T GetPropertyofType<T>() where T : ThingProperty
        {
            for (int i = 0; i < _thingProperties.Length; i++)
            {
                if (_thingProperties[i] is T)
                    return (T)_thingProperties[i];
            }
            return null;
        }
    }
}

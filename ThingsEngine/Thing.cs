using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThingEngine
{
    [System.Serializable]
    [CreateAssetMenu(fileName ="New thing", menuName ="New thing", order = 0)]
    public class Thing : ScriptableObject
    {
        [SerializeField]
        private string thingName;

        public string Name
        {
            get
            {
                return thingName;
            }
        }

        [SerializeField]
        private Sprite image;

        public Sprite Image
        {
            get
            {
                return image;
            }
        }


        [SerializeField]
        private Vector2 worldItemSize = new Vector2(1f, 1f);

        public Vector2 WorldItemSize
        {
            get
            {
                return worldItemSize;
            }
        }


        [SerializeField]
        [Min(1)]
        private int maxAmountInSlote = 20;

        public int MaxAmountInSlote
        {
            get
            {
                return maxAmountInSlote;
            }
        }

        [SerializeField]
        private MainProperty mainProperty;

        public MainProperty MainProperty
        {
            get { return mainProperty; }
        }

        [SerializeField]
        private ThingProperty[] thingProperties;

        public ThingProperty[] Properties
        {
            get 
            {
                List<ThingProperty> props = new List<ThingProperty>();
                //props.Add(mainProperty);
                props.AddRange(thingProperties);
                return props.ToArray(); 
            }
        }

        public string SavingID { get { return name; } }

        public T GetPropertyofType<T>() where T : ThingProperty
        {
            for(int i = 0; i < thingProperties.Length;i++)
            {
                if (thingProperties[i] is T)
                    return (T)thingProperties[i];
            }
            return null;
        }

        public bool IsStacking { get { return MaxAmountInSlote > 1; } }

    }
}

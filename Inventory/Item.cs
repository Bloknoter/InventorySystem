using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventoryEngine.Things;

namespace InventoryEngine
{
    public class Item
    {
        public delegate void OnDestroyItemDelegate();

        public event OnDestroyItemDelegate OnDestroyItemEvent;

        public static Item Create(Thing thing)
        {
            if (thing == null) return null;
            return new Item(thing);
        }

        public static Item Create(object SerializedData)
        {
            Dictionary<string, object> data = (Dictionary<string, object>)SerializedData;
            Thing thing = ThingCollection.Find((string)data["thingid"]);

            if (thing == null) return null;

            return new Item(thing, SerializedData); ;
        }

        private Item(Thing thing)
        {
            _thing = thing;
            if (Thing.MainProperty != null)
                _mainProperty = Object.Instantiate(Thing.MainProperty);
            else
                _mainProperty = ScriptableObject.CreateInstance<MainProperty>();
            _mainProperty.Item = this;
            _properties = new ThingProperty[Thing.Properties.Length];
            for (int i = 0; i < Thing.Properties.Length; i++)
            {
                _properties[i] = Object.Instantiate(Thing.Properties[i]);
                _properties[i].MainProperty = _mainProperty;
            }
        }

        private Item(Thing thing, object SerializedData)
        {
            Dictionary<string, object> data = (Dictionary<string, object>)SerializedData;
            _thing = thing;
            if (Thing.MainProperty != null)
                _mainProperty = Object.Instantiate(Thing.MainProperty);
            else
                _mainProperty = ScriptableObject.CreateInstance<MainProperty>();
            _mainProperty.Item = this;
            _mainProperty.Strength = (float)data["strength"];
            _properties = new ThingProperty[Thing.Properties.Length];
            for (int i = 0; i < Thing.Properties.Length; i++)
            {
                _properties[i] = Object.Instantiate(Thing.Properties[i]);
                _properties[i].MainProperty = _mainProperty;
            }
        }



        private Thing _thing;

        private MainProperty _mainProperty;

        private ThingProperty[] _properties;

        public Thing Thing => _thing;

        public MainProperty MainProperty => _mainProperty;

        public T GetProperty<T>() where T : ThingProperty
        {
            for (int i = 0; i < _properties.Length; i++)
            {
                if (_properties[i] is T)
                    return (T)_properties[i];
            }
            return null;
        }

        public T[] GetProperties<T>() where T : ThingProperty
        {
            List<T> props = new List<T>();
            for (int i = 0; i < _properties.Length; i++)
            {
                if (_properties[i] is T)
                    props.Add((T)_properties[i]);
            }
            return props.ToArray();
        }

        public void DestroyItem()
        {
            for(int i = 0; i < _properties.Length;i++)
            {
                if(_properties[i] is IDestroyItemListener)
                {
                    ((IDestroyItemListener)_properties[i]).OnDestroyItem();
                }
            }
            OnDestroyItemEvent?.Invoke();
        }

        public object GetSerializedData()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("thingid", Thing.name);
            data.Add("strength", _mainProperty.Strength);
            return data;
        }

    }

}

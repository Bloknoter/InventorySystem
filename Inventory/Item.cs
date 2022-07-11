using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ThingEngine;

namespace InventoryEngine
{
    public class Item
    {
        public Thing thing { get; private set; }

        private MainProperty mainProperty;

        public MainProperty MainProperty
        {
            get { return mainProperty; }
        }

        private ThingProperty[] properties;

        public static Item Create(Thing newthing)
        {
            if(newthing != null)
            {
                return new Item(newthing);
            }
            return null;
        }

        public static Item Create(object SerializedData)
        {
            Dictionary<string, object> data = (Dictionary<string, object>)SerializedData;
            Thing thing = ThingCollection.Find((string)data["thingid"]);
            if(thing != null)
            {
                Item item = new Item(thing, SerializedData);
                return item;
            }
            return null;
        }

        private Item(Thing newthing)
        {
            thing = newthing;
            if (thing.MainProperty != null)
                mainProperty = (MainProperty)thing.MainProperty.GetCopy();
            else
                mainProperty = ScriptableObject.CreateInstance<MainProperty>();
            mainProperty.Item = this;
            properties = new ThingProperty[thing.Properties.Length];
            for (int i = 0; i < thing.Properties.Length; i++)
            {
                properties[i] = thing.Properties[i].GetCopy();
                properties[i].SetMainProperty(mainProperty);
            }
        }

        private Item(Thing _thing, object SerializedData)
        {
            Dictionary<string, object> data = (Dictionary<string, object>)SerializedData;
            thing = _thing;
            if (thing.MainProperty != null)
                mainProperty = (MainProperty)thing.MainProperty.GetCopy();
            else
                mainProperty = ScriptableObject.CreateInstance<MainProperty>();
            mainProperty.Item = this;
            mainProperty.Strength = (float)data["strength"];
            properties = new ThingProperty[thing.Properties.Length];
            for (int i = 0; i < thing.Properties.Length; i++)
            {
                properties[i] = thing.Properties[i].GetCopy();
                properties[i].SetMainProperty(mainProperty);
            }
        }

        public delegate void OnDestroyItemDelegate();

        public event OnDestroyItemDelegate OnDestroyItemEvent;

        public T GetPropertyorNull<T>() where T : ThingProperty
        {
            for (int i = 0; i < properties.Length; i++)
            {
                if (properties[i] is T)
                    return (T)properties[i];
            }
            return null;
        }

        public T[] GetPropertiesorNull<T>() where T : ThingProperty
        {
            List<T> props = new List<T>();
            for (int i = 0; i < properties.Length; i++)
            {
                if (properties[i] is T)
                    props.Add((T)properties[i]);
            }
            return props.ToArray();
        }

        public void DestroyItem()
        {
            for(int i = 0; i < properties.Length;i++)
            {
                if(properties[i] is IDestroyItemListener)
                {
                    ((IDestroyItemListener)properties[i]).OnDestroyItem();
                }
            }
            OnDestroyItemEvent?.Invoke();
        }

        public object GetSerializedData()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("thingid", thing.name);
            data.Add("strength", mainProperty.Strength);
            return data;
        }

    }

}

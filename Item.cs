using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace InventoryEngine
{
    public delegate void DestroyItemListener();

    public class Item
    {
        private const string c_saveThingID = "thing_id";

        public event DestroyItemListener OnPreDestroy;
        public event DestroyItemListener OnPostDestroy;

        public IItemContainer ItemContainer;

        private Thing m_thing;
        private ReadOnlyCollection<ThingProperty> m_properties;
        private bool m_isDestroyed;

        public Thing Thing => m_thing;
        public bool IsDestroyed => m_isDestroyed;

        public static Item Create(Thing thing)
        {
            if (thing == null)
            {
                Debug.LogError("Can't create item from null thing");
                return null;
            }
            
            return new Item(thing);
        }

        public static Item Create(object serializedData)
        {
            Dictionary<string, object> data = (Dictionary<string, object>)serializedData;
            Thing thing = ThingCollection.Find((string)data[c_saveThingID]);

            if (thing == null) 
                return null;

            return new Item(thing, serializedData);
        }

        private Item()
        {

        }

        private Item(Thing thing)
        {
            m_thing = thing;

            var props = new ThingProperty[Thing.Properties.Length];

            for (int i = 0; i < props.Length; i++)
            {
                props[i] = Object.Instantiate(Thing.Properties[i]);
                props[i].InitializeFromItem(this, true);
                props[i].Initialize();
            }

            m_properties = new ReadOnlyCollection<ThingProperty>(props);

            for (int i = 0; i < props.Length; i++)
            {
                props[i].PostInitialize();
            }
        }

        private Item(Thing thing, object serializedData)
        {
            Dictionary<string, object> data = (Dictionary<string, object>)serializedData;

            m_thing = thing;

            var props = new ThingProperty[Thing.Properties.Length];

            for (int i = 0; i < props.Length; i++)
            {
                props[i] = Object.Instantiate(Thing.Properties[i]);

                props[i].InitializeFromItem(this, false);
                if (data.TryGetValue(props[i].UniqueID, out var propData) && propData != null)
                    props[i].LoadFromSave(propData);
                props[i].Initialize();
            }

            m_properties = new ReadOnlyCollection<ThingProperty>(props);

            for (int i = 0; i < props.Length; i++)
            {
                props[i].PostInitialize();
            } 
        }

        public T GetProperty<T>() where T : ThingProperty
        {
            for (int i = 0; i < m_properties.Count; i++)
            {
                if (m_properties[i] is T)
                    return (T)m_properties[i];
            }
            return null;
        }

        public List<T> GetProperties<T>() where T : ThingProperty
        {
            List<T> props = new List<T>();
            for (int i = 0; i < m_properties.Count; i++)
            {
                if (m_properties[i] is T)
                    props.Add((T)m_properties[i]);
            }
            return props;
        }

        public void MergeInto(Item other, int baseAmount, int mergingAmount)
        {
            var notMergedPropsIndexes = new HashSet<int>(other.m_properties.Count);
            for(int i = 0;i < other.m_properties.Count;++i)
                notMergedPropsIndexes.Add(i);

            for (int i = 0; i < m_properties.Count; ++i)
            {
                int foundIndex = -1;
                foreach (var otherIndex in notMergedPropsIndexes)
                {
                    if (m_properties[i].UniqueID == other.m_properties[otherIndex].UniqueID)
                    {
                        m_properties[i].MergeInto(other.m_properties[otherIndex], baseAmount, mergingAmount);
                        foundIndex = otherIndex;
                        break;
                    }
                }
                notMergedPropsIndexes.Remove(foundIndex);
            }
        }

        public Item Clone()
        {
            if (m_isDestroyed)
            {
                Debug.LogError($"Trying to clone item that is already destroyed (thing: {m_thing.Name})");
                return null;
            }

            var cloneItem = new Item();
            cloneItem.m_thing = m_thing;

            var clonedProps = new ThingProperty[m_properties.Count];
            for(int i = 0; i < m_properties.Count;++i)
            {
                clonedProps[i] = m_properties[i].Clone();
                clonedProps[i].InitializeFromItem(cloneItem, false);
                clonedProps[i].Initialize();
            }

            cloneItem.m_properties = new ReadOnlyCollection<ThingProperty>(clonedProps);

            for (int i = 0; i < cloneItem.m_properties.Count; i++)
            {
                cloneItem.m_properties[i].PostInitialize();
            }

            return cloneItem;
        }

        public void DestroyItem()
        {
            if(m_isDestroyed)
            {
                Debug.LogError($"Trying to destroy item that is already destroyed (thing: {m_thing.Name})");
                return;
            }

            OnPreDestroy?.Invoke();

            m_isDestroyed = true;
            for(int i = 0; i < m_properties.Count;i++)
                m_properties[i].OnDestroyItem();

            OnPostDestroy?.Invoke();
        }

        public void RequestRemoveItem()
        {
            if (m_isDestroyed)
            {
                Debug.LogError($"Trying to request remove item that is already destroyed (thing: {m_thing.Name})");
                return;
            }

            if(ItemContainer == null)
            {
                Debug.LogError($"Trying to request remove item that is not handled by any contanier (ItemContainer is null; thing: {m_thing.Name})");
                return;
            }

            ItemContainer.RemoveOne();
        }

        public object GetSerializedData()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add(c_saveThingID, Thing.UniqueID);
            for(int i = 0; i < m_properties.Count;++i)
            {
                if (m_properties[i].Savable)
                    data.Add(m_properties[i].UniqueID, m_properties[i].CreateDataForSave());
            }
            return data;
        }

    }

}

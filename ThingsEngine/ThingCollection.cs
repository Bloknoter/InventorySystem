using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InventoryEngine
{
    public class ThingCollection
    {
        private static Dictionary<string, Thing> m_things;

        public static Thing Find(string thingID)
        {
            if (m_things == null)
                InitializeThingLibrary();

            if (m_things.ContainsKey(thingID))
                return m_things[thingID];
            return null;
        }

        private static void InitializeThingLibrary()
        {
            m_things = new Dictionary<string, Thing>();
            Thing[] loadedThings = Resources.LoadAll<Thing>("Things");
            for (int i = 0; i < loadedThings.Length; i++)
            {
                m_things.Add(loadedThings[i].UniqueID, loadedThings[i]);
            }
        }
    }
}

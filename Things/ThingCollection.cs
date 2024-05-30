using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InventoryEngine
{
    public class ThingCollection
    {
        private static Dictionary<string, Thing> things;

        public static Thing Find(string Name)
        {
            if (things == null)
                InitializeThingLibrary();

            string nameNormalized = Name.ToLowerInvariant().Replace(" ", "");

            if (things.ContainsKey(nameNormalized))
                return things[nameNormalized];
            return null;
        }

        private static void InitializeThingLibrary()
        {
            things = new Dictionary<string, Thing>();
            Thing[] loadedthings = Resources.LoadAll<Thing>("Things");
            for (int i = 0; i < loadedthings.Length; i++)
            {
                things.Add(loadedthings[i].name.ToLowerInvariant().Replace(" ", ""), loadedthings[i]);
            }
        }
    }
}

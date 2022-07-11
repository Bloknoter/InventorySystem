using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ThingEngine
{
    public class ThingCollection : MonoBehaviour
    {
        /*#region Singletone
        private static ThingCollection instance;
        public static ThingCollection Get()
        {
            return instance;
        }

        private void OnEnable()
        {
            instance = this;
            Thing[] loadedthings = Resources.LoadAll<Thing>("Things");
            for(int i = 0; i < loadedthings.Length;i++)
            {
                things.Add(loadedthings[i].Name, loadedthings[i]);
            }
        }

        #endregion
        */
        private static Dictionary<string, Thing> things;

        public static Thing Find(string Name)
        {
            if (things == null)
            {
                things = new Dictionary<string, Thing>();
                Thing[] loadedthings = Resources.LoadAll<Thing>("Things");
                for (int i = 0; i < loadedthings.Length; i++)
                {
                    things.Add(loadedthings[i].name.ToLowerInvariant().Replace(" ", ""), loadedthings[i]);
                }
            }
            if (things.ContainsKey(Name.ToLowerInvariant().Replace(" ", "")))
                return things[Name.ToLowerInvariant().Replace(" ", "")];
            return null;
        }
    }
}

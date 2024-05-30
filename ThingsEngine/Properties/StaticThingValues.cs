using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InventoryEngine
{
    [CreateAssetMenu(fileName = "StaticThingValues", menuName = InventoryCreateAssetMenuPathsGeneral.GENERAL_PROPERTIES_MENU + "/StaticThingValue")]
    public class StaticThingValues : ThingProperty
    {
        [System.Serializable]
        public class StaticValue
        {
            public string ID;

            public string Value;
        }

        [Header("Supports string, int, float, bool types")]
        [SerializeField]
        private List<StaticValue> m_staticValues;

        protected override string GetUniquePropID()
        {
            return "StaticThingValues";
        }

        public string GetValueAsString(string id)
        {
            var staticValue = GetStaticValue(id);

            return staticValue.Value;
        }

        public int GetValueAsInt(string id)
        {
            var staticValue = GetStaticValue(id);

            return int.Parse(staticValue.Value);
        }

        public float GetValueAsFloat(string id)
        {
            var staticValue = GetStaticValue(id);

            return float.Parse(staticValue.Value);
        }

        public bool GetValueAsBool(string id)
        {
            var staticValue = GetStaticValue(id);

            return bool.Parse(staticValue.Value);
        }

        public bool TryGetValueAsString(string id, out string value)
        {
            var staticValue = GetStaticValue(id);

            if(staticValue == null)
            {
                value = string.Empty;
                return false;
            }

            value = staticValue.Value;
            return true;
        }

        public bool TryGetValueAsInt(string id, out int value)
        {
            var staticValue = GetStaticValue(id);

            if (staticValue == null)
            {
                value = 0;
                return false;
            }

            value = int.Parse(staticValue.Value);
            return true;
        }

        public bool TryGetValueAsFloat(string id, out float value)
        {
            var staticValue = GetStaticValue(id);

            if (staticValue == null)
            {
                value = 0;
                return false;
            }

            value = float.Parse(staticValue.Value);
            return true;
        }

        public bool TryGetValueAsBool(string id, out bool value)
        {
            var staticValue = GetStaticValue(id);

            if (staticValue == null)
            {
                value = false;
                return false;
            }

            value = bool.Parse(staticValue.Value);
            return true;
        }

        private StaticValue GetStaticValue(string id)
        {
            for(int i = 0; i < m_staticValues.Count;i++)
            {
                if (m_staticValues[i].ID == id)
                    return m_staticValues[i];
            }

            return null;
        }
    }
}

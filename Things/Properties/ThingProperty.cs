using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventoryEngine
{
    public abstract class ThingProperty : ScriptableObject
    {
        public virtual void Initialize()
        {

        }

        public virtual void OnDestroyItem()
        {

        }
    }
}

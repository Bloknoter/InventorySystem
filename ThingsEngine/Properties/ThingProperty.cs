using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThingEngine
{
    public abstract class ThingProperty : ScriptableObject
    {

        public abstract ThingProperty GetCopy();

        public MainProperty mainProperty { get; protected set; }

        //public abstract bool MustBeCreated();

        public void SetMainProperty(MainProperty newmainProperty)
        {
            mainProperty = newmainProperty;
        }
    }

    public interface IDestroyItemListener
    {
        void OnDestroyItem();
    }

    public interface IDeathListener
    {
        void OnDeath();
    }
}

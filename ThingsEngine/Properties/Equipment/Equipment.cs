using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PlayerSystems;
using VitalStatsEngine;

namespace ThingEngine
{
    public abstract class Equipment : ThingProperty
    {
        [SerializeField]
        [Min(0)]
        protected int armor;

        public override ThingProperty GetCopy()
        {
            Equipment equipment = GetEquipmentCopy();
            equipment.armor = armor;
            return equipment;
        }

        protected abstract Equipment GetEquipmentCopy();

    }
}

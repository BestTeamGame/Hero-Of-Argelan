using System;
using UnityEngine;

namespace Project.Scripts.Stats
{
    // На будущее: можно добавить тип модификатора StatModifierType (число, проценты, множитель...)
    [Serializable]
    public class StatModifier
    {
        [SerializeField] private StatType statType;
        [SerializeField] private float value;
        
        public float Value => value;
        public StatType StatType => statType;
    }
}

using System;

namespace Project.Scripts.Stats
{
    // На будущее: можно добавить тип модификатора StatModifierType (число, проценты, множитель...)
    [Serializable]
    public class StatModifier
    {
        private StatType statType;
        private float value;
        
        public float Value => value;
        public StatType StatType => statType;
    }
}

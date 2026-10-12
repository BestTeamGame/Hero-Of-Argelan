using System;

namespace Project.Scripts.Stats
{
    [Serializable]
    public class StatModifier
    {
        private StatType statType;
        private float value;
        
        public float Value => value;
        public StatType StatType => statType;
    }
}

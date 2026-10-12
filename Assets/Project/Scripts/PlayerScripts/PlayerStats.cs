using System.Collections.Generic;
using Project.Scripts.Stats;
using UnityEngine;

namespace Project.Scripts.PlayerScripts
{
    public class PlayerStats : MonoBehaviour
    {
        [Header("Resources")]
        [SerializeField, Min(1f)] private float baseMaxHealth = 100f;
        [SerializeField, Min(0f)] private float baseMaxMana = 50f;

        [Header("Defense")]
        [SerializeField, Min(0f)] private float baseDefense;

        [Header("Damage")]
        [SerializeField, Min(0f)] private float baseMeleeDamage = 1f;
        [SerializeField, Min(0f)] private float baseMagicDamage = 1f;
        [SerializeField, Min(0f)] private float baseRangedDamage = 1f;

        [Header("Speed")]
        [SerializeField, Min(0f)] private float baseAttackSpeed = 1f;
        [SerializeField, Min(0f)] private float baseMovementSpeed = 1f;

        [Header("Critical")]
        [SerializeField, Range(0f, 1f)] private float baseCritChance = 0.05f;
        [SerializeField, Min(1f)] private float baseCritMultiplier = 1.5f;

        private readonly List<StatModifier> modifiers = new();

        public void AddModifier(StatModifier modifier)
        {
            if (modifier == null)
                return;

            modifiers.Add(modifier);
        }

        public bool RemoveModifier(StatModifier modifier)
        {
            if (modifier == null)
                return false;

            return modifiers.Remove(modifier);
        }

        public float GetStat(StatType stat)
        {
            float value = GetBaseStat(stat);

            foreach (StatModifier modifier in modifiers)
            {
                if (modifier.StatType != stat)
                    continue;

                value += modifier.Value;
            }

            return ClampStat(stat, value);
        }

        private float GetBaseStat(StatType stat)
        {
            return stat switch
            {
                StatType.MaxHealth => baseMaxHealth,
                StatType.MaxMana => baseMaxMana,
                StatType.Defense => baseDefense,

                StatType.MeleeDamage => baseMeleeDamage,
                StatType.MagicDamage => baseMagicDamage,
                StatType.RangedDamage => baseRangedDamage,

                StatType.AttackSpeed => baseAttackSpeed,
                StatType.MovementSpeed => baseMovementSpeed,

                StatType.CritChance => baseCritChance,
                StatType.CritMultiplier => baseCritMultiplier,

                _ => 0f
            };
        }

        private static float ClampStat(StatType stat, float value)
        {
            return stat switch
            {
                StatType.MaxHealth => Mathf.Max(1f, value),
                StatType.MaxMana => Mathf.Max(0f, value),
                StatType.Defense => Mathf.Max(0f, value),

                StatType.MeleeDamage => Mathf.Max(0f, value),
                StatType.MagicDamage => Mathf.Max(0f, value),
                StatType.RangedDamage => Mathf.Max(0f, value),

                StatType.AttackSpeed => Mathf.Max(0f, value),
                StatType.MovementSpeed => Mathf.Max(0f, value),

                StatType.CritChance => Mathf.Clamp01(value),
                StatType.CritMultiplier => Mathf.Max(1f, value),

                _ => value
            };
        }
    }
}
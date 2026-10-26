using Project.Scripts.Combat;
using Project.Scripts.PlayerScripts;
using Project.Scripts.Stats;
using UnityEngine;

namespace Project.Scripts.Inventory.Items
{
    public abstract class WeaponData : EquippableItemData
    {
        [Header("Combat")]
        [SerializeField, Min(0f)]
        private float baseDamage;

        [SerializeField, Min(0.01f)]
        private float attackCooldown;

        public float BaseDamage => baseDamage;

        public float AttackCooldown => attackCooldown;

        public abstract StatType DamageStat { get; }

        public virtual bool CanUse(PlayerContext context)
        {
            return true;
        }

        public virtual bool TryConsumeResources(
            PlayerContext context)
        {
            return true;
        }

        public abstract void Attack(
            WeaponAttackContext context);
    }
}
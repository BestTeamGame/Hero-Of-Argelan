using Project.Scripts.Combat;
using Project.Scripts.PlayerScripts;
using Project.Scripts.Stats;
using UnityEngine;

namespace Project.Scripts.Inventory.Items
{
    public abstract class MagicWeaponData : WeaponData
    {
        [SerializeField, Min(0f)]
        private float manaCost;

        public float ManaCost => manaCost;

        public override StatType DamageStat =>
            StatType.MagicDamage;

        public override bool CanUse(
            PlayerContext context)
        {
            return context.Mana.CurrentMana >= manaCost;
        }

        public override bool TryConsumeResources(
            PlayerContext context)
        {
            return context.Mana.Spend(manaCost);
        }
        
        public override void Attack(WeaponAttackContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}

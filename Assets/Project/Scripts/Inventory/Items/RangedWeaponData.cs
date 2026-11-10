using Project.Scripts.Combat;
using Project.Scripts.PlayerScripts;
using Project.Scripts.Stats;
using UnityEngine;

namespace Project.Scripts.Inventory.Items
{
    public abstract class RangedWeaponData : WeaponData
    {
        [SerializeField, Min(1)]
        private int arrowCost = 1;

        public int ArrowCost => arrowCost;

        public override StatType DamageStat =>
            StatType.RangedDamage;

        public override bool CanUse(
            PlayerContext context)
        {
            return context.Inventory.Arrows >=
                   arrowCost;
        }

        public override bool TryConsumeResources(
            PlayerContext context)
        {
            return context.Inventory.UseArrows(
                arrowCost
            );
        }
        
        public override void Attack(WeaponAttackContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}

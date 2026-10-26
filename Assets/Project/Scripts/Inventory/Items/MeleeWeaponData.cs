using Project.Scripts.Combat;
using Project.Scripts.PlayerScripts;
using Project.Scripts.Stats;
using UnityEngine;

namespace Project.Scripts.Inventory.Items
{
    public class MeleeWeaponData : WeaponData
    {
        public override StatType DamageStat =>
            StatType.MeleeDamage;

        public override bool CanUse(
            PlayerContext context)
        {
            return true;
        }

        public override bool TryConsumeResources(
            PlayerContext context)
        {
            return true;
        }

        public override void Attack(WeaponAttackContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}

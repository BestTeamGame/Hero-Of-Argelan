using UnityEngine;

namespace Project.Scripts.Inventory.Items
{
    public class MeleeWeaponData: WeaponData
    {
        [SerializeField] private float attackRange;

        public float AttackRange => attackRange;
    }
}

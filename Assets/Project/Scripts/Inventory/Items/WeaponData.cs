using UnityEngine;

namespace Project.Scripts.Inventory.Items
{
    public class WeaponData: EquippableItemData
    {
        [SerializeField] private int damage;
        [SerializeField] private float attackSpeed;
        
        public int Damage => damage;
        public float AttackSpeed => attackSpeed;
    }
}

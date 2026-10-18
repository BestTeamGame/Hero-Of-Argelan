using UnityEngine;

namespace Project.Scripts.Inventory.Items
{
    public class MagicWeaponData: WeaponData
    {
        [SerializeField] private int manaCost;
        
        public int ManaCost => manaCost;
    }
}

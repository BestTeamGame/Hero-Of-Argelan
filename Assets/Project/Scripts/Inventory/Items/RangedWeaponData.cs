using UnityEngine;

namespace Project.Scripts.Inventory.Items
{
    public class RangedWeaponData: WeaponData
    {
        [SerializeField] private int arrowsPerShot = 1;
        
        public int ArrowsPerShot => arrowsPerShot;
    }
}

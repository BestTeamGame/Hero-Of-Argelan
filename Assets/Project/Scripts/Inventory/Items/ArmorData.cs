using UnityEngine;

namespace Project.Scripts.Inventory.Items
{
    public class ArmorData: EquippableItemData
    {
        [SerializeField] private ArmorType armorType;
        
        public ArmorType ArmorType => armorType;
    }
}

using UnityEngine;

namespace Project.Scripts.Inventory.Items
{
    [CreateAssetMenu(
        menuName = "Project/Armors/Default Armor",
        fileName = "DefaultArmor"
    )]
    public class ArmorData: EquippableItemData
    {
        [SerializeField] private ArmorType armorType;
        
        public ArmorType ArmorType => armorType;
    }
}

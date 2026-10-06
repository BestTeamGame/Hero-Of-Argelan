using UnityEngine;

namespace Project.Scripts.Inventory.Items
{
    public class ActiveSkillData: EquippableItemData
    {
        [SerializeField] private float cooldown;
        
        public float Cooldown => cooldown;
    }
}

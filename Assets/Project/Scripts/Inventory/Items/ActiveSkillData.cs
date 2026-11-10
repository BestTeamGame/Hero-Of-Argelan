using Project.Scripts.Combat;
using Project.Scripts.PlayerScripts;
using UnityEngine;

namespace Project.Scripts.Inventory.Items
{
    public abstract class ActiveSkillData : EquippableItemData
    {
        [Header("Skill")]
        [SerializeField, Min(0f)]
        private float cooldown = 5f;

        public float Cooldown => cooldown;

        public virtual bool CanUse(
            PlayerContext context)
        {
            return true;
        }

        public virtual bool TryConsumeResources(
            PlayerContext context)
        {
            return true;
        }

        public abstract void Use(
            SkillUseContext context);
    }
}

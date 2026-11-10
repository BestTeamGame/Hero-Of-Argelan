using Project.Scripts.Combat;
using Project.Scripts.Inventory.Items;
using Project.Scripts.PlayerScripts;
using UnityEngine;

namespace Project.Scripts.Items.Skills
{
    [CreateAssetMenu(
        menuName = "Project/Skills/Test Healing Skill",
        fileName = "TestHealingSkill"
    )]
    public sealed class TestHealingSkillData : ActiveSkillData
    {
        [Header("Healing")]
        [SerializeField, Min(0f)]
        private float healAmount = 25f;

        public override bool CanUse(PlayerContext context)
        {
            return context != null &&
                   context.Health.CurrentHealth < context.Health.MaxHealth;
        }

        public override void Use(SkillUseContext context)
        {
            if (context.Player == null)
                return;

            context.Player.Health.Heal(healAmount);
        }
    }
}

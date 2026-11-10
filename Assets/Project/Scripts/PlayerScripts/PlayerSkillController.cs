using System;
using Project.Scripts.Combat;
using Project.Scripts.Inventory.Items;
using UnityEngine;

namespace Project.Scripts.PlayerScripts
{
    [RequireComponent(typeof(PlayerContext))]
    [RequireComponent(typeof(PlayerInventory))]
    [RequireComponent(typeof(PlayerHealth))]
    public class PlayerSkillController : MonoBehaviour
    {
        [Header("Skill")]
        [SerializeField]
        private Transform skillOrigin;

        private PlayerContext context;
        private PlayerInventory inventory;
        private PlayerHealth health;

        private float nextUseTime;

        public ActiveSkillData CurrentSkill =>
            inventory.ActiveSkill;

        public bool IsOnCooldown =>
            Time.time < nextUseTime;

        public float CooldownRemaining =>
            Mathf.Max(
                0f,
                nextUseTime - Time.time
            );

        public float CooldownNormalized
        {
            get
            {
                ActiveSkillData skill = CurrentSkill;

                if (!skill || skill.Cooldown <= 0f)
                    return 0f;

                return Mathf.Clamp01(
                    CooldownRemaining /
                    skill.Cooldown
                );
            }
        }

        public event Action SkillUsed;
        public event Action CooldownStarted;

        private void Awake()
        {
            context = GetComponent<PlayerContext>();
            inventory = GetComponent<PlayerInventory>();
            health = GetComponent<PlayerHealth>();

            if (!skillOrigin)
                skillOrigin = transform;
        }

        public bool TryUseSkill()
        {
            if (health.IsDead)
                return false;

            ActiveSkillData skill =
                CurrentSkill;

            if (!skill)
                return false;

            if (IsOnCooldown)
                return false;

            if (!skill.CanUse(context))
                return false;

            if (!skill.TryConsumeResources(context))
                return false;

            SkillUseContext useContext =
                new(
                    context,
                    skillOrigin,
                    GetUseDirection()
                );

            skill.Use(useContext);

            nextUseTime =
                GetNextUseTime(skill);

            SkillUsed?.Invoke();
            CooldownStarted?.Invoke();

            return true;
        }

        private float GetNextUseTime(
            ActiveSkillData skill)
        {
            return Time.time +
                   skill.Cooldown;
        }

        private Vector2 GetUseDirection()
        {
            return context.Movement.IsFacingRight
                ? Vector2.right
                : Vector2.left;
        }
    }
}
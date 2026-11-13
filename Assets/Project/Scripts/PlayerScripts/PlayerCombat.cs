using System.Linq;
using Project.Scripts.Combat;
using Project.Scripts.Inventory.Items;
using Project.Scripts.Stats;
using UnityEngine;

namespace Project.Scripts.PlayerScripts
{
    [RequireComponent(typeof(PlayerContext))]
    [RequireComponent(typeof(PlayerInventory))]
    [RequireComponent(typeof(PlayerStats))]
    [RequireComponent(typeof(PlayerHealth))]
    [RequireComponent(typeof(PlayerCombatEvents))]
    [RequireComponent(typeof(Animator))]
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Attack")]
        [SerializeField] private Transform attackOrigin;

        private PlayerContext context;

        private PlayerInventory inventory;
        private PlayerStats stats;
        private PlayerHealth health;
        private PlayerCombatEvents combatEvents;
        private Animator animator;

        private WeaponData pendingWeapon;
        private WeaponAttackContext pendingAttackContext;
        private bool hasPendingAttack;

        private float weapon1NextAttackTime;
        private float weapon2NextAttackTime;

        public WeaponData Weapon1 =>
            inventory.WeaponSlot1;

        public WeaponData Weapon2 =>
            inventory.WeaponSlot2;

        public float Weapon1CooldownRemaining =>
            Mathf.Max(
                0f,
                weapon1NextAttackTime - Time.time
            );

        public float Weapon2CooldownRemaining =>
            Mathf.Max(
                0f,
                weapon2NextAttackTime - Time.time
            );

        private void Awake()
        {
            context = GetComponent<PlayerContext>();

            inventory = GetComponent<PlayerInventory>();
            stats = GetComponent<PlayerStats>();
            health = GetComponent<PlayerHealth>();

            combatEvents = GetComponent<PlayerCombatEvents>();
            
            animator = GetComponent<Animator>();

            if (!attackOrigin)
                attackOrigin = transform;
        }

        public bool TryAttackWeapon1()
        {
            return TryAttack(
                Weapon1,
                ref weapon1NextAttackTime
            );
        }

        public bool TryAttackWeapon2()
        {
            return TryAttack(
                Weapon2,
                ref weapon2NextAttackTime
            );
        }

        private bool TryAttack(
            WeaponData weapon,
            ref float nextAttackTime)
        {
            if (hasPendingAttack)
                return false;

            if (health.IsDead)
                return false;

            if (!weapon)
                return false;

            if (Time.time < nextAttackTime)
                return false;

            if (!weapon.CanUse(context))
                return false;

            if (!weapon.TryConsumeResources(context))
                return false;

            float damage =
                CalculateDamage(weapon);

            Vector2 direction =
                GetAttackDirection();

            WeaponAttackContext attackContext =
                new(
                    context,
                    attackOrigin,
                    direction,
                    damage
                );

            if (HasAttackAnimation(weapon.AttackAnimationTrigger))
            {
                pendingWeapon = weapon;
                pendingAttackContext = attackContext;
                hasPendingAttack = true;
                animator.SetTrigger(weapon.AttackAnimationTrigger);
            }
            else
            {
                weapon.Attack(attackContext);
                combatEvents.RaiseAttackPerformed();
            }

            nextAttackTime =
                GetNextAttackTime(weapon);

            return true;
        }
        
        public void DealSwordDamage()
        {
            PerformPendingAttack();
        }
        
        public void ShootArrow()
        {
            PerformPendingAttack();
        }

        private void PerformPendingAttack()
        {
            if (!hasPendingAttack)
                return;

            WeaponData weapon = pendingWeapon;
            WeaponAttackContext attackContext = pendingAttackContext;

            hasPendingAttack = false;
            pendingWeapon = null;

            if (health.IsDead || !weapon)
                return;

            weapon.Attack(attackContext);
            combatEvents.RaiseAttackPerformed();
        }

        private bool HasAttackAnimation(string triggerName)
        {
            if (!animator || string.IsNullOrEmpty(triggerName))
                return false;

            return animator.parameters.Any(parameter =>
                parameter.type == AnimatorControllerParameterType.Trigger
                && parameter.name == triggerName);
        }
        
        private float GetNextAttackTime(
            WeaponData weapon)
        {
            float attackSpeed =
                stats.GetStat(
                    StatType.AttackSpeed
                );

            attackSpeed =
                Mathf.Max(
                    0.01f,
                    attackSpeed
                );

            float cooldown =
                weapon.AttackCooldown /
                attackSpeed;

            return Time.time + cooldown;
        }

        private float CalculateDamage(
            WeaponData weapon)
        {
            float multiplier =
                stats.GetStat(weapon.DamageStat);

            return weapon.BaseDamage *
                   multiplier;
        }

        private Vector2 GetAttackDirection()
        {
            return context.Movement.IsFacingRight
                ? Vector2.right
                : Vector2.left;
        }
    }
}

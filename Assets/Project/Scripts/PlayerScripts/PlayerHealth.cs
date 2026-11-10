using System;
using Project.Scripts.Stats;
using UnityEngine;

namespace Project.Scripts.PlayerScripts
{
    [RequireComponent(typeof(PlayerStats))]
    [RequireComponent(typeof(PlayerCombatEvents))]
    public class PlayerHealth : MonoBehaviour
    {
        private static readonly int Hurt =
            Animator.StringToHash("Hurt");

        private static readonly int Death =
            Animator.StringToHash("Death");

        private PlayerStats stats;
        private PlayerCombatEvents combatEvents;

        private Animator animator;

        [SerializeField]
        private float currentHealth;

        private bool isDead;

        public float CurrentHealth => currentHealth;

        public float MaxHealth =>
            stats.GetStat(StatType.MaxHealth);

        public bool IsDead => isDead;

        public event Action<float, float> HealthChanged;
        public event Action Died;

        private void Awake()
        {
            stats = GetComponent<PlayerStats>();
            combatEvents = GetComponent<PlayerCombatEvents>();

            animator = GetComponent<Animator>();

            currentHealth = MaxHealth;
        }

        private void OnEnable()
        {
            stats.StatChanged += OnStatChanged;
        }

        private void OnDisable()
        {
            stats.StatChanged -= OnStatChanged;
        }

        public bool TakeDamage(float damage)
        {
            if (isDead || damage <= 0f)
                return false;

            float previousHealth = currentHealth;

            currentHealth = Mathf.Max(
                0f,
                currentHealth - damage
            );

            float actualDamage =
                previousHealth - currentHealth;

            if (actualDamage <= 0f)
                return false;

            combatEvents.RaiseDamageTaken(actualDamage);

            HealthChanged?.Invoke(
                currentHealth,
                MaxHealth
            );

            if (animator)
                animator.SetTrigger(Hurt);

            if (currentHealth <= 0f)
                Die();

            return true;
        }

        public bool Heal(float amount)
        {
            if (isDead || amount <= 0f)
                return false;

            float previousHealth = currentHealth;

            currentHealth = Mathf.Min(
                currentHealth + amount,
                MaxHealth
            );

            if (Mathf.Approximately(
                    previousHealth,
                    currentHealth))
            {
                return false;
            }

            HealthChanged?.Invoke(
                currentHealth,
                MaxHealth
            );

            return true;
        }

        private void Die()
        {
            if (isDead)
                return;

            isDead = true;

            currentHealth = 0f;

            if (animator)
                animator.SetTrigger(Death);

            HealthChanged?.Invoke(
                currentHealth,
                MaxHealth
            );

            Died?.Invoke();
        }

        private void OnStatChanged(StatType stat)
        {
            if (stat != StatType.MaxHealth)
                return;

            currentHealth = Mathf.Min(
                currentHealth,
                MaxHealth
            );

            HealthChanged?.Invoke(
                currentHealth,
                MaxHealth
            );
        }
    }
}
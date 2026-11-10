using UnityEngine;

namespace Project.Scripts.PlayerScripts
{
    [RequireComponent(
        typeof(PlayerStats),
        typeof(PlayerCombatEvents),
        typeof(PlayerHealth)
    )]
    [RequireComponent(
        typeof(PlayerMana),
        typeof(PlayerMovement),
        typeof(PlayerInventory)
    )]
    [RequireComponent(
        typeof(PlayerEffects),
        typeof(PlayerCombat),
        typeof(PlayerSkillController))]
    [RequireComponent(typeof(PlayerConsumables))]
    [DisallowMultipleComponent]
    public sealed class PlayerContext : MonoBehaviour
    {
        private bool initialized;

        private PlayerMovement movement;
        private PlayerHealth health;
        private PlayerMana mana;
        private PlayerStats stats;
        private PlayerInventory inventory;
        private PlayerEffects effects;
        private PlayerCombatEvents combatEvents;
        private PlayerCombat combat;
        private PlayerSkillController skillController;
        private PlayerConsumables consumables;

        public PlayerMovement Movement
        {
            get
            {
                EnsureInitialized();
                return movement;
            }
        }

        public PlayerHealth Health
        {
            get
            {
                EnsureInitialized();
                return health;
            }
        }

        public PlayerMana Mana
        {
            get
            {
                EnsureInitialized();
                return mana;
            }
        }

        public PlayerStats Stats
        {
            get
            {
                EnsureInitialized();
                return stats;
            }
        }

        public PlayerInventory Inventory
        {
            get
            {
                EnsureInitialized();
                return inventory;
            }
        }

        public PlayerEffects Effects
        {
            get
            {
                EnsureInitialized();
                return effects;
            }
        }

        public PlayerCombatEvents CombatEvents
        {
            get
            {
                EnsureInitialized();
                return combatEvents;
            }
        }

        public PlayerCombat Combat
        {
            get
            {
                EnsureInitialized();
                return combat;
            }
        }

        public PlayerSkillController SkillController
        {
            get
            {
                EnsureInitialized();
                return skillController;
            }
        }

        public PlayerConsumables Consumables
        {
            get
            {
                EnsureInitialized();
                return consumables;
            }
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (initialized)
                return;

            movement = GetRequiredComponent<PlayerMovement>();
            health = GetRequiredComponent<PlayerHealth>();
            mana = GetRequiredComponent<PlayerMana>();
            stats = GetRequiredComponent<PlayerStats>();
            inventory = GetRequiredComponent<PlayerInventory>();
            effects = GetRequiredComponent<PlayerEffects>();
            combatEvents = GetRequiredComponent<PlayerCombatEvents>();
            combat = GetRequiredComponent<PlayerCombat>();
            skillController = GetRequiredComponent<PlayerSkillController>();
            consumables = GetRequiredComponent<PlayerConsumables>();

            initialized = true;
        }

        private T GetRequiredComponent<T>() where T : Component
        {
            if (TryGetComponent(out T component))
                return component;

            Debug.LogError(
                $"{nameof(PlayerContext)} на объекте '{name}': " +
                $"не найден обязательный компонент {typeof(T).Name}.",
                this
            );

            return null;
        }
    }
}
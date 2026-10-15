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
    [RequireComponent(typeof(PlayerEffects))]
    public class PlayerContext : MonoBehaviour
    {
        public PlayerMovement Movement { get; private set; }
        public PlayerHealth Health { get; private set; }
        public PlayerMana Mana { get; private set; }

        public PlayerStats Stats { get; private set; }

        public PlayerInventory Inventory { get; private set; }
        public PlayerEffects Effects { get; private set; }

        public PlayerCombatEvents CombatEvents { get; private set; }

        private void Awake()
        {
            Movement = GetComponent<PlayerMovement>();
            Health = GetComponent<PlayerHealth>();
            Mana = GetComponent<PlayerMana>();

            Stats = GetComponent<PlayerStats>();

            Inventory = GetComponent<PlayerInventory>();
            Effects = GetComponent<PlayerEffects>();

            CombatEvents = GetComponent<PlayerCombatEvents>();
        }
    }
}
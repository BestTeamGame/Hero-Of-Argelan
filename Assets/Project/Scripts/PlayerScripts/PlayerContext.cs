using UnityEngine;

namespace Project.Scripts.PlayerScripts
{
    [RequireComponent(typeof(Player))]
    [RequireComponent(typeof(PlayerInventory))]
    [RequireComponent(typeof(PlayerStats))]
    [RequireComponent(typeof(PlayerCombatEvents))]
    public class PlayerContext : MonoBehaviour
    {
        public Player Player { get; private set; }
        public PlayerInventory Inventory { get; private set; }
        public PlayerStats Stats { get; private set; }
        public PlayerEffects Effects { get; private set; }
        public PlayerCombatEvents CombatEvents { get; private set; }

        private void Awake()
        {
            Player = GetComponent<Player>();
            Inventory = GetComponent<PlayerInventory>();
            Stats = GetComponent<PlayerStats>();
            Effects = GetComponent<PlayerEffects>();
            CombatEvents = GetComponent<PlayerCombatEvents>();
        }
    }
}
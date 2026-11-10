using System;
using UnityEngine;

namespace Project.Scripts.PlayerScripts
{
    [RequireComponent(typeof(PlayerInventory))]
    [RequireComponent(typeof(PlayerHealth))]
    [RequireComponent(typeof(PlayerMana))]
    public class PlayerConsumables : MonoBehaviour
    {
        [Header("Health Potion")]
        [SerializeField, Min(0f)]
        private float healthRestoreAmount = 40f;

        [Header("Mana Potion")]
        [SerializeField, Min(0f)]
        private float manaRestoreAmount = 30f;

        private PlayerInventory inventory;
        private PlayerHealth health;
        private PlayerMana mana;

        public float HealthRestoreAmount =>
            healthRestoreAmount;

        public float ManaRestoreAmount =>
            manaRestoreAmount;

        public bool CanUseHealthPotion =>
            !health.IsDead &&
            inventory.HealthPotions > 0 &&
            health.CurrentHealth < health.MaxHealth;

        public bool CanUseManaPotion =>
            !health.IsDead &&
            inventory.ManaPotions > 0 &&
            mana.CurrentMana < mana.MaxMana;

        public event Action HealthPotionUsed;
        public event Action ManaPotionUsed;

        private void Awake()
        {
            inventory = GetComponent<PlayerInventory>();
            health = GetComponent<PlayerHealth>();
            mana = GetComponent<PlayerMana>();
        }

        public bool UseHealthPotion()
        {
            if (!CanUseHealthPotion)
                return false;

            if (!inventory.UseHealthPotion())
                return false;

            health.Heal(healthRestoreAmount);

            HealthPotionUsed?.Invoke();

            return true;
        }

        public bool UseManaPotion()
        {
            if (!CanUseManaPotion)
                return false;

            if (!inventory.UseManaPotion())
                return false;

            mana.Restore(manaRestoreAmount);

            ManaPotionUsed?.Invoke();

            return true;
        }
    }
}
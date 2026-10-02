using System.Collections.Generic;
using Project.Scripts.Inventory.Items;
using Project.Scripts.Stats;
using UnityEngine;

namespace Project.Scripts.PlayerScripts
{
    public class PlayerInventory : MonoBehaviour
    {
        [Header("General")]
        // Или получать в Awake, пока не уверен
        [SerializeField] private PlayerContext playerContext;
        
        [Header("Weapons")]
        [SerializeField] private WeaponData weaponSlot1;
        [SerializeField] private WeaponData weaponSlot2;

        [Header("Skill")]
        [SerializeField] private ActiveSkillData activeSkill;

        [Header("Armor")]
        [SerializeField] private ArmorData headArmor;
        [SerializeField] private ArmorData bodyArmor;

        [Header("Artifacts")]
        [SerializeField] private ArtifactData[] artifacts = new ArtifactData[3];

        [Header("Consumables")]
        [SerializeField, Min(0)] private int healthPotions;
        [SerializeField, Min(0)] private int manaPotions;

        [Header("Resources")]
        [SerializeField, Min(0)] private int coins;
        [SerializeField, Min(0)] private int arrows;

        [Header("Key Items")]
        [SerializeField] private List<KeyItemData> keyItems = new();
        
        [Header("Limits")]
        [SerializeField, Min(1)] private int maxHealthPotions = 5;
        [SerializeField, Min(1)] private int maxManaPotions = 5;
        [SerializeField, Min(1)] private int maxArrows = 999;
        
        public int HealthPotions => healthPotions;
        public int ManaPotions => manaPotions;
        
        public int Coins => coins;
        public int Arrows => arrows;

        // HealthPotions
        
        private void SetHealthPotions(int amount)
        {
            healthPotions = Mathf.Clamp(amount, 0, maxHealthPotions);
        }
        
        public bool AddHealthPotions(int amount)
        {
            if (amount <= 0 || healthPotions >= maxHealthPotions)
                return false;
            
            SetHealthPotions(healthPotions + amount);
            return true;
        }

        public bool UseHealthPotion()
        {
            if (healthPotions <= 0)
                return false;

            SetHealthPotions(healthPotions - 1);
            return true;
        }
        
        // ManaPotions
        
        private void SetManaPotions(int amount)
        {
            manaPotions = Mathf.Clamp(amount, 0, maxManaPotions);
        }
        
        public bool AddManaPotions(int amount)
        {
            if (amount <= 0 || manaPotions >= maxManaPotions)
                return false;
            
            SetManaPotions(manaPotions + amount);
            return true;
        }

        public bool UseManaPotion()
        {
            if (manaPotions <= 0)
                return false;
            
            SetManaPotions(manaPotions - 1);
            return true;
        }
        
        // Coins
        
        private void SetCoins(int amount)
        {
            coins = Mathf.Max(0, amount);
        }

        public bool AddCoins(int amount)
        {
            if (amount <= 0)
                return false;
            
            SetCoins(coins + amount);
            return true;
        }
        
        public bool SpendCoins(int amount)
        {
            if (amount <= 0 || coins < amount)
                return false;

            SetCoins(coins - amount);
            return true;
        }

        // Arrows
        
        private void SetArrows(int amount)
        {
            arrows = Mathf.Clamp(amount, 0, maxArrows);
        }

        public bool AddArrows(int amount)
        {
            if (amount <= 0 || arrows >= maxArrows)
                return false;
            
            SetArrows(arrows + amount);
            return true;
        }

        public bool UseArrows(int amount = 1)
        {
            if (amount <= 0 || arrows < amount)
                return false;
            
            SetArrows(arrows - amount);
            return true;
        }
        
        // Экипировка и снятие
        
        private void ApplyItem(EquippableItemData item)
        {
            if (!item)
                return;

            foreach (StatModifier modifier in item.StatModifiers)
            {
                // playerContext.Stats.AddModifier(modifier);
            }

            foreach (ItemEffect effect in item.ItemEffects)
            {
                // playerContext.Effects.AddEffect(effect);
            }
        }
        
        private void RemoveItem(EquippableItemData item)
        {
            if (!item)
                return;

            foreach (StatModifier modifier in item.StatModifiers)
            {
                // playerContext.Stats.RemoveModifier(modifier);
            }

            foreach (ItemEffect effect in item.ItemEffects)
            {
                // playerContext.Effects.RemoveEffect(effect);
            }
        }
        
        public WeaponData EquipWeapon(WeaponData weapon, int slot)
        {
            WeaponData oldWeapon;

            switch (slot)
            {
                case 0:
                    oldWeapon = weaponSlot1;
                    RemoveItem(oldWeapon);

                    weaponSlot1 = weapon;
                    ApplyItem(weaponSlot1);
                    break;

                case 1:
                    oldWeapon = weaponSlot2;
                    RemoveItem(oldWeapon);

                    weaponSlot2 = weapon;
                    ApplyItem(weaponSlot2);
                    break;

                default:
                    return null;
            }

            return oldWeapon;
        }
    }
}

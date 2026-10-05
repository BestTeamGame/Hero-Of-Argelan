using System;
using System.Collections.Generic;
using Project.Scripts.Inventory.Items;
using Project.Scripts.Stats;
using UnityEngine;

namespace Project.Scripts.PlayerScripts
{
    [RequireComponent(typeof(PlayerContext))]
    public class PlayerInventory : MonoBehaviour
    {
        // Приватные поля
        
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
        
        // Публичные поля
        
        public WeaponData WeaponSlot1 => weaponSlot1;
        public WeaponData WeaponSlot2 => weaponSlot2;

        public ActiveSkillData ActiveSkill => activeSkill;

        public ArmorData HeadArmor => headArmor;
        public ArmorData BodyArmor => bodyArmor;

        public IReadOnlyList<ArtifactData> Artifacts => artifacts;
        
        public int HealthPotions => healthPotions;
        public int ManaPotions => manaPotions;
        
        public int Coins => coins;
        public int Arrows => arrows;
        
        public IReadOnlyList<KeyItemData> KeyItems => keyItems;
        
        public int MaxHealthPotions => maxHealthPotions;
        public int MaxManaPotions => maxManaPotions;
        public int MaxArrows => maxArrows;
        
        // События
        
        public event Action EquipmentChanged;
        public event Action ResourcesChanged;
        public event Action KeyItemsChanged;

        // HealthPotions
        
        private void SetHealthPotions(int amount)
        {
            healthPotions = Mathf.Clamp(amount, 0, maxHealthPotions);
            ResourcesChanged?.Invoke();
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
            ResourcesChanged?.Invoke();
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
            ResourcesChanged?.Invoke();
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
            ResourcesChanged?.Invoke();
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
        
        // KeyItems
        
        /// <summary>
        /// Проверяет есть ли в инвентаре ключевой предмет
        /// </summary>
        /// <param name="keyItem">Ключевой предмет</param>
        /// <returns>True, если ключевой предмет не null и он есть в инвентаре,
        /// иначе - false</returns>
        public bool HasKeyItem(KeyItemData keyItem)
        {
            return keyItem && keyItems.Contains(keyItem);
        }
        
        /// <summary>
        /// Добавляет ключевой предмет
        /// </summary>
        /// <param name="keyItem">Ключевой предмет</param>
        /// <returns>Успешна ли операция</returns>
        public bool AddKeyItem(KeyItemData keyItem)
        {
            if (!keyItem || HasKeyItem(keyItem))
                return false;
            
            keyItems.Add(keyItem);
            KeyItemsChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Удаляет ключевой предмет
        /// </summary>
        /// <param name="keyItem">Ключевой предмет</param>
        /// <returns>Успешна ли операция</returns>
        public bool RemoveKeyItem(KeyItemData keyItem)
        {
            if (!keyItem || !keyItems.Remove(keyItem))
                return false;
            
            KeyItemsChanged?.Invoke();
            return true;
        }
        
        // Экипировка и снятие
        
        /// <summary>
        /// Применяет эффекты предмета
        /// </summary>
        /// <param name="item">Экипируемый предмет</param>
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
        
        /// <summary>
        /// Отменяет эффекты предмета
        /// </summary>
        /// <param name="item">Экипируемый предмет</param>
        private void UnapplyItem(EquippableItemData item)
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
        
        /// <summary>
        /// Экипирует оружие
        /// </summary>
        /// <param name="weapon">Оружие</param>
        /// <param name="slot">Слот для экипировки (начало с 0)</param>
        /// <returns>Прежде экипированное оружие или null</returns>
        public WeaponData EquipWeapon(WeaponData weapon, int slot)
        {
            WeaponData oldWeapon;

            if (!weapon)
            {
                return null;
            }
            
            switch (slot)
            {
                case 0:
                    oldWeapon = weaponSlot1;
                    UnapplyItem(oldWeapon);

                    weaponSlot1 = weapon;
                    ApplyItem(weaponSlot1);
                    break;

                case 1:
                    oldWeapon = weaponSlot2;
                    UnapplyItem(oldWeapon);

                    weaponSlot2 = weapon;
                    ApplyItem(weaponSlot2);
                    break;

                default:
                    return null;
            }
            EquipmentChanged?.Invoke();
            return oldWeapon;
        }
        
        /// <summary>
        /// Снимает оружие
        /// </summary>
        /// <param name="slot">Слот для снятия (начало с 0)</param>
        /// <returns>Снятое оружие</returns>
        public WeaponData UnequipWeapon(int slot)
        {
            if (slot < 0 || slot > 1)
                return null;

            WeaponData weapon = slot == 0
                ? weaponSlot1
                : weaponSlot2;

            if (!weapon)
                return null;

            UnapplyItem(weapon);

            if (slot == 0)
                weaponSlot1 = null;
            else
                weaponSlot2 = null;

            EquipmentChanged?.Invoke();
            return weapon;
        }
        
        /// <summary>
        /// Экипирует броню (автоматически в нужный слот по типу)
        /// </summary>
        /// <param name="armor">Броня</param>
        /// <returns>Прежде экипированная броня или null</returns>
        public ArmorData EquipArmor(ArmorData armor)
        {
            if (!armor)
                return null;

            ArmorData oldArmor;

            switch (armor.armorType)
            {
                case ArmorType.Head:
                    oldArmor = headArmor;

                    UnapplyItem(oldArmor);

                    headArmor = armor;

                    ApplyItem(headArmor);
                    break;

                case ArmorType.Body:
                    oldArmor = bodyArmor;

                    UnapplyItem(oldArmor);

                    bodyArmor = armor;

                    ApplyItem(bodyArmor);
                    break;

                default:
                    return null;
            }
            EquipmentChanged?.Invoke();
            return oldArmor;
        }
        
        /// <summary>
        /// Снимает броню
        /// </summary>
        /// <param name="type">Тип слота брони для снятия</param>
        /// <returns>Снятая броня</returns>
        public ArmorData UnequipArmor(ArmorType type)
        {
            if (type != ArmorType.Head && type != ArmorType.Body)
                return null;

            ArmorData armor = type == ArmorType.Head
                ? headArmor
                : bodyArmor;

            if (!armor)
                return null;

            UnapplyItem(armor);

            if (type == ArmorType.Head)
                headArmor = null;
            else
                bodyArmor = null;

            EquipmentChanged?.Invoke();
            return armor;
        }
        
        /// <summary>
        /// Экипирует артефакт
        /// </summary>
        /// <param name="artifact">Артефакт</param>
        /// <param name="slot">Слот для экипировки (начало с 0)</param>
        /// <returns>Прежде экипированный артефакт или null</returns>
        public ArtifactData EquipArtifact(ArtifactData artifact, int slot)
        {
            if (!artifact)
                return null;

            if (slot < 0 || slot >= artifacts.Length)
                return null;

            ArtifactData oldArtifact = artifacts[slot];

            UnapplyItem(oldArtifact);

            artifacts[slot] = artifact;

            ApplyItem(artifact);
            
            EquipmentChanged?.Invoke();
            return oldArtifact;
        }
        
        /// <summary>
        /// Снимает артефакт
        /// </summary>
        /// <param name="slot">Слот для снятия (начало с 0)</param>
        /// <returns>Снятый артефакт</returns>
        public ArtifactData UnequipArtifact(int slot)
        {
            if (slot < 0 || slot >= artifacts.Length)
                return null;
            
            ArtifactData artifact = artifacts[slot];
            
            if (!artifact)
                return null;

            UnapplyItem(artifact);
            
            artifacts[slot] = null;
            
            EquipmentChanged?.Invoke();
            return artifact;
        }
        
        /// <summary>
        /// Экипирует скилл
        /// </summary>
        /// <param name="skill">Скилл</param>
        /// <returns>Прежде экипированный скилл или null</returns>
        public ActiveSkillData EquipActiveSkill(ActiveSkillData skill)
        {
            if (!skill)
                return null;
            
            ActiveSkillData oldSkill = activeSkill;

            UnapplyItem(oldSkill);

            activeSkill = skill;

            ApplyItem(activeSkill);
            
            EquipmentChanged?.Invoke();
            return oldSkill;
        }

        /// <summary>
        /// Снимает скилл
        /// </summary>
        /// <returns>Снятый скилл</returns>
        public ActiveSkillData UnequipActiveSkill()
        {
            ActiveSkillData skill = activeSkill;
            
            if (!skill)
                return null;

            UnapplyItem(skill);
            
            activeSkill = null;
            
            EquipmentChanged?.Invoke();
            return skill;
        }
        
        // Инициализация
        
        private void Start()
        {
            ApplyItem(weaponSlot1);
            ApplyItem(weaponSlot2);

            ApplyItem(activeSkill);

            ApplyItem(headArmor);
            ApplyItem(bodyArmor);

            foreach (ArtifactData artifact in artifacts)
            {
                ApplyItem(artifact);
            }
        }

        private void Awake()
        {
            playerContext = GetComponent<PlayerContext>();
        }
    }
}

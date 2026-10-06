using Project.Scripts.Inventory.Items;
using Project.Scripts.PlayerScripts;
using UnityEngine;

namespace Project.Scripts.Inventory.UI
{
    [RequireComponent(typeof(InventoryExampleView))]
    public class InventoryUIController : MonoBehaviour
    {
        private const int WeaponSlot1Index = 0;
        private const int WeaponSlot2Index = 1;
        private const int ActiveSkillSlotIndex = 2;
        private const int HeadArmorSlotIndex = 3;
        private const int BodyArmorSlotIndex = 4;
        private const int FirstArtifactSlotIndex = 5;
        private const int ArtifactSlotsCount = 3;

        [Header("View")]
        [SerializeField] private InventoryExampleView view;

        private PlayerInventory inventory;
        private bool isSubscribed;

        private void Awake()
        {
            if (!view)
                view = GetComponent<InventoryExampleView>();
        }

        private void OnEnable()
        {
            Subscribe();
            RefreshAll();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        /// <summary>
        /// Связывает UI инвентаря с конкретным PlayerInventory.
        /// </summary>
        /// <param name="playerInventory">Инвентарь игрока.</param>
        public void Initialize(PlayerInventory playerInventory)
        {
            if (!playerInventory)
            {
                Debug.LogWarning(
                    "InventoryUIController: передан пустой PlayerInventory.",
                    this
                );

                return;
            }
            
            Unsubscribe();

            inventory = playerInventory;

            if (!isActiveAndEnabled)
                return;

            Subscribe();
            RefreshAll();
        }

        private void Subscribe()
        {
            if (!inventory || isSubscribed)
                return;

            inventory.EquipmentChanged += RefreshEquipment;
            inventory.ResourcesChanged += RefreshResources;
            inventory.KeyItemsChanged += RefreshKeyItems;

            isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!inventory || !isSubscribed)
                return;

            inventory.EquipmentChanged -= RefreshEquipment;
            inventory.ResourcesChanged -= RefreshResources;
            inventory.KeyItemsChanged -= RefreshKeyItems;

            isSubscribed = false;
        }

        private void RefreshAll()
        {
            if (!inventory || !view)
                return;

            RefreshEquipment();
            RefreshResources();
            RefreshKeyItems();
        }

        private void RefreshEquipment()
        {
            if (!inventory || !view)
                return;

            view.SetSlot(
                WeaponSlot1Index,
                ConvertItem(inventory.WeaponSlot1)
            );

            view.SetSlot(
                WeaponSlot2Index,
                ConvertItem(inventory.WeaponSlot2)
            );

            view.SetSlot(
                ActiveSkillSlotIndex,
                ConvertItem(inventory.ActiveSkill)
            );

            view.SetSlot(
                HeadArmorSlotIndex,
                ConvertItem(inventory.HeadArmor)
            );

            view.SetSlot(
                BodyArmorSlotIndex,
                ConvertItem(inventory.BodyArmor)
            );

            for (int i = 0; i < ArtifactSlotsCount; i++)
            {
                ViewItem artifact = i < inventory.Artifacts.Count
                    ? ConvertItem(inventory.Artifacts[i])
                    : null;

                view.SetSlot(
                    FirstArtifactSlotIndex + i,
                    artifact
                );
            }
        }

        private void RefreshResources()
        {
            if (!inventory || !view)
                return;

            view.SetResources(
                inventory.HealthPotions,
                inventory.MaxHealthPotions,
                inventory.ManaPotions,
                inventory.MaxManaPotions,
                inventory.Coins,
                inventory.Arrows
            );
        }

        private void RefreshKeyItems()
        {
            if (!inventory || !view)
                return;

            ViewItem[] items =
                new ViewItem[inventory.KeyItems.Count];

            for (int i = 0; i < inventory.KeyItems.Count; i++)
            {
                items[i] = ConvertItem(inventory.KeyItems[i]);
            }

            view.SetKeyItems(items);
        }

        private static ViewItem ConvertItem(ItemData item)
        {
            if (!item)
                return null;

            return new ViewItem
            {
                source = item,
                icon = item.Icon,
                title = item.ItemName,
                category = GetCategory(item),
                description = item.Description,
                stats = GetStats(item),
                effects = GetEffects(item)
            };
        }

        private static string GetCategory(ItemData item)
        {
            return item switch
            {
                WeaponData => "Оружие",
                ActiveSkillData => "Активный навык",
                ArmorData => "Броня",
                ArtifactData => "Артефакт",
                KeyItemData => "Ключевой предмет",
                _ => "Предмет"
            };
        }

        private static string GetStats(ItemData item)
        {
            if (item is not EquippableItemData)
                return "";

            // TODO:
            // Здесь позже можно сформировать красивую строку
            // из equippable.StatModifiers.

            return "";
        }

        private static string GetEffects(ItemData item)
        {
            if (item is not EquippableItemData)
                return "";

            // TODO:
            // Здесь позже можно сформировать описание
            // equippable.ItemEffects.

            return "";
        }
    }
}
using System.Linq;
using Project.Scripts.Converters;
using Project.Scripts.Inventory.Items;
using Project.Scripts.PlayerScripts;
using UnityEngine;
using UnityEngine.InputSystem;

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
        
        [Header("Input")]
        [SerializeField] private InputActionReference inventoryAction;

        private PlayerInventory inventory;
        private bool isSubscribed;
        private bool isOpen;

        private void Awake()
        {
            if (!view)
                view = GetComponent<InventoryExampleView>();
            
            view.Close();
        }
        
        private void OnEnable()
        {
            Subscribe();
            
            if (view)
                view.CloseRequested += Close;
            
            if (inventoryAction)
            {
                inventoryAction.action.performed += OnInventoryPressed;
                inventoryAction.action.Enable();
            }
            
            RefreshAll();
        }

        private void OnDisable()
        {
            Unsubscribe();
            
            if (view)
                view.CloseRequested -= Close;

            if (!inventoryAction) return;
            
            inventoryAction.action.performed -= OnInventoryPressed;
            inventoryAction.action.Disable();
        }
        
        /// <summary>
        /// Связывает UI инвентаря с конкретным <see cref="PlayerInventory"/>.
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
        
        /// <summary>
        /// Событие при нажатии инвентаря.
        /// </summary>
        /// <param name="context">Контекст.</param>
        private void OnInventoryPressed(InputAction.CallbackContext context)
        {
            Toggle();
        }
        
        /// <summary>
        /// Закрывает UI, если открыт. Иначе - открывает UI.
        /// </summary>
        public void Toggle()
        {
            if (isOpen)
                Close();
            else
                Open();
        }
        
        /// <summary>
        /// Открывает UI представление инвентаря и ставит игру на паузу.
        /// </summary>
        public void Open()
        {
            if (isOpen)
                return;

            isOpen = true;

            RefreshAll();
            view.Open();
            
            Time.timeScale = 0f;
        }

        /// <summary>
        /// Закрывает UI представление инвентаря и убирает игру с паузы.
        /// </summary>
        public void Close()
        {
            if (!isOpen)
                return;

            isOpen = false;
            
            view.Close();
            
            Time.timeScale = 1f;
        }

        /// <summary>
        /// Подписывает контроллер на обновления инвентаря <see cref="PlayerInventory"/>.
        /// </summary>
        private void Subscribe()
        {
            if (!inventory || isSubscribed)
                return;

            inventory.EquipmentChanged += RefreshEquipment;
            inventory.ResourcesChanged += RefreshResources;
            inventory.KeyItemsChanged += RefreshKeyItems;

            isSubscribed = true;
        }

        /// <summary>
        /// Отписывает контроллер на обновления инвентаря <see cref="PlayerInventory"/>.
        /// </summary>
        private void Unsubscribe()
        {
            if (!inventory || !isSubscribed)
                return;

            inventory.EquipmentChanged -= RefreshEquipment;
            inventory.ResourcesChanged -= RefreshResources;
            inventory.KeyItemsChanged -= RefreshKeyItems;

            isSubscribed = false;
        }

        /// <summary>
        /// Обновляет все элементы инвентаря на UI представлении.
        /// </summary>
        private void RefreshAll()
        {
            if (!inventory || !view)
                return;

            RefreshEquipment();
            RefreshResources();
            RefreshKeyItems();
        }

        /// <summary>
        /// Обновляет экипировку на UI представлении.
        /// </summary>
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

        /// <summary>
        /// Обновляет ресурсы на UI представлении.
        /// </summary>
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

        /// <summary>
        /// Обновляет ключевые предметы на UI представлении.
        /// </summary>
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
        
        /// <summary>
        /// Конвертирует предмет из типа <see cref="ItemData"/> в тип <see cref="ViewItem"/>.
        /// </summary>
        /// <param name="item">Предмет типа <see cref="ItemData"/>.</param>
        /// <returns>Предмет типа <see cref="ViewItem"/>.</returns>
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

        /// <summary>
        /// Получает строковое название категории предмета.
        /// </summary>
        /// <param name="item">Предмет.</param>
        /// <returns>Название категории предмета.</returns>
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

        /// <summary>
        /// Получает строковые модификаторы статистики предмета.
        /// </summary>
        /// <param name="item">Предмет.</param>
        /// <returns>Модификаторы статистики предмета.</returns>
        private static string GetStats(ItemData item)
        {
            if (item is not EquippableItemData equippable)
                return "";

            if (equippable.StatModifiers.Count == 0)
                return "";

            return string.Join(
                "\n",
                equippable.StatModifiers.Select(StatTextFormatter.Format)
            );
        }

        /// <summary>
        /// Получает строковые эффекты предмета.
        /// </summary>
        /// <param name="item">Предмет.</param>
        /// <returns>Эффекты предмета.</returns>
        private static string GetEffects(ItemData item)
        {
            if (item is not EquippableItemData equippable)
                return "";

            if (equippable.ItemEffects.Count == 0)
                return "";

            return string.Join(
                "\n\n",
                equippable.ItemEffects.Select(
                    effect => $"{effect.EffectName}\n{effect.GetDescription()}"
                )
            );
        }
    }
}
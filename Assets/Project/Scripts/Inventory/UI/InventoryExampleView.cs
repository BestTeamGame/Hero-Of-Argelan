using System;
using System.Collections.Generic;
using Project.Scripts.Inventory.Items;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.Inventory.UI
{
    [Serializable]
    public class ViewItem
    {
        [NonSerialized] public ItemData source;
        
        public Sprite icon;
        public string title;
        public string category;
        [TextArea] public string description;
        [TextArea] public string stats;
        [TextArea] public string effects;
    }

    [Serializable]
    public class ViewSlot
    {
        public string label;
        public Button button;
        public Image icon;
        public GameObject selection;
        public GameObject emptyHint;
        
        [NonSerialized] public ViewItem item;
    }

    // Visual example only. Supply data from your inventory through the public methods.
    public sealed class InventoryExampleView : MonoBehaviour
    {
        public ViewSlot[] slots;
        public Image detailIcon;
        public Text detailTitle, detailCategory, detailDescription, detailStats, detailEffects, selectedLabel;
        public Text hpCount, manaCount, coinCount, arrowCount;
        public Image cooldownShade;
        public Text cooldownText;
        public Button closeButton;
        public RectTransform window, keyContent;
        public int selectedIndex;
        private float remaining, duration;
        public event Action<int> SelectionChanged;

        [Header("Key items grid")]
        [Min(8)] public int keyItemCellSize = 28;
        [Min(0)] public int keyItemSpacing = 4;
        public Color keyItemBackgroundColor = new Color32(10, 16, 28, 255);
        public Color keyItemSelectionColor = new Color32(71, 218, 239, 255);
        public event Action<int> KeyItemSelectionChanged; 
        private readonly List<ViewSlot> keySlots = new();
        private GridLayoutGroup keyGrid;
        private int selectedKeyItemIndex = -1;

        private void Awake()
        {
            if (slots != null)
                for (int i = 0; i < slots.Length; i++)
                {
                    int index = i;
                    if (slots[i].button)
                        slots[i].button.onClick.AddListener(() => SelectSlot(index));
                }
            if (closeButton)
                closeButton.onClick.AddListener(Close);
            if (keyContent) keyGrid = keyContent.GetComponent<GridLayoutGroup>();
            SelectSlot(selectedIndex);
        }
        
        private void OnRectTransformDimensionsChange()
        {
            if (keyGrid && keyContent)
                UpdateKeyGridLayout();
        }

        public void Open() { window.gameObject.SetActive(true); }
        public void Close() { window.gameObject.SetActive(false); }

        // Slot order: weapon I, weapon II, active skill, head, body, artifacts I/II/III.
        public void SetSlot(int index, ViewItem item)
        {
            if (slots == null || index < 0 || index >= slots.Length) return;
            slots[index].item = item;
            slots[index].icon.sprite = item?.icon;
            slots[index].icon.enabled = item != null && item.icon;
            if (slots[index].emptyHint) slots[index].emptyHint.SetActive(item == null);
            if (selectedIndex == index && selectedKeyItemIndex < 0) SelectSlot(index);
        }

        public void SelectSlot(int index)
        {
            if (slots == null || index < 0 || index >= slots.Length) return;
            ClearKeyItemSelection();
            selectedIndex = index;
            for (int i = 0; i < slots.Length; i++) slots[i].selection.SetActive(i == index);
            ViewItem item = slots[index].item;
            detailIcon.sprite = item?.icon;
            detailIcon.enabled = item != null && item.icon;
            detailTitle.text = item != null ? item.title : "Пустой слот";
            detailCategory.text = item != null ? item.category : slots[index].label;
            detailDescription.text = item != null ? item.description : "Выбери предмет в этой категории.";
            detailStats.text = item != null ? item.stats : "";
            detailEffects.text = item != null ? item.effects : "";
            selectedLabel.text = "Выбрано: " + slots[index].label;
            SelectionChanged?.Invoke(index);
        }

        public void SetResources(int hp, int hpLimit, int mana, int manaLimit, int coins, int arrows)
        {
            hpCount.text = hp + " / " + hpLimit;
            manaCount.text = mana + " / " + manaLimit;
            coinCount.text = coins.ToString();
            arrowCount.text = arrows.ToString();
        }

        // Pass actual remaining/total seconds; this example counts down in unscaled time.
        public void SetSkillCooldown(float secondsRemaining, float totalSeconds)
        {
            duration = Mathf.Max(0.001f, totalSeconds);
            remaining = Mathf.Clamp(secondsRemaining, 0f, duration);
            RefreshCooldown();
        }

        private void RefreshCooldown()
        {
            cooldownShade.gameObject.SetActive(remaining > 0f);
            cooldownShade.fillAmount = remaining / Mathf.Max(duration, 0.001f);
            cooldownText.text = remaining > 0f ? remaining.ToString("0.0") + " с" : "Готов";
        }

        // Unique items only; square icon buttons without labels or fixed capacity.
        public void SetKeyItems(ViewItem[] items)
        {
            if (!keyContent) return;

            bool hadKeySelection = selectedKeyItemIndex >= 0;
            ViewItem previousItem = selectedKeyItemIndex >= 0 && selectedKeyItemIndex < keySlots.Count
                ? keySlots[selectedKeyItemIndex].item
                : null;

            for (int i = keyContent.childCount - 1; i >= 0; i--)
            {
                GameObject child = keyContent.GetChild(i).gameObject;
                child.SetActive(false);
                if (Application.isPlaying)
                    Destroy(child);
                else
                    DestroyImmediate(child);
            }
            keySlots.Clear();
            selectedKeyItemIndex = -1;

            // The grid controls cell positions; the content height is calculated below.
            foreach (LayoutGroup layout in keyContent.GetComponents<LayoutGroup>())
                if (!(layout is GridLayoutGroup)) layout.enabled = false;
            ContentSizeFitter fitter = keyContent.GetComponent<ContentSizeFitter>();
            if (fitter) fitter.enabled = false;

            keyGrid = keyContent.GetComponent<GridLayoutGroup>();
            if (!keyGrid) keyGrid = keyContent.gameObject.AddComponent<GridLayoutGroup>();
            keyGrid.enabled = true;
            keyGrid.padding = new RectOffset();
            keyGrid.childAlignment = TextAnchor.UpperLeft;
            keyGrid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            keyGrid.startAxis = GridLayoutGroup.Axis.Horizontal;
            keyGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;

            keyContent.anchorMin = new Vector2(0, 1);
            keyContent.anchorMax = new Vector2(1, 1);
            keyContent.pivot = new Vector2(0, 1);
            keyContent.sizeDelta = new Vector2(0, keyContent.sizeDelta.y);
            keyContent.anchoredPosition = Vector2.zero;

            ScrollRect scroll = keyContent.GetComponentInParent<ScrollRect>();
            if (scroll && scroll.content == keyContent)
            {
                scroll.horizontal = false;
                scroll.vertical = true;
            }

            if (items != null)
                foreach (ViewItem item in items)
                {
                    if (item == null) continue;
                    int index = keySlots.Count;
                    keySlots.Add(CreateKeySlot(item, index));
                }
            
            UpdateKeyGridLayout();
            LayoutRebuilder.ForceRebuildLayoutImmediate(keyContent);

            // Preserve selection when the same item instance is still in the collection.
            int restoredIndex =
                previousItem?.source 
                    ? keySlots.FindIndex(
                        slot =>
                            slot.item != null &&
                            slot.item.source == previousItem.source)
                    : -1;
            if (restoredIndex >= 0)
                SelectKeyItem(restoredIndex);
            else if (hadKeySelection)
                SelectSlot(selectedIndex);
        }
        
        
        public void SelectKeyItem(int index)
        {
            if (index < 0 || index >= keySlots.Count) return;
            selectedKeyItemIndex = index;
            if (slots != null)
                foreach (ViewSlot slot in slots)
                    if (slot != null && slot.selection) slot.selection.SetActive(false);
            for (int i = 0; i < keySlots.Count; i++)
                keySlots[i].selection.SetActive(i == index);

            ViewItem item = keySlots[index].item;
            detailIcon.sprite = item.icon;
            detailIcon.enabled = item.icon;
            detailTitle.text = item.title;
            detailCategory.text = item.category;
            detailDescription.text = item.description;
            detailStats.text = item.stats;
            detailEffects.text = item.effects;
            selectedLabel.text = "Выбрано: " + item.title;
            KeyItemSelectionChanged?.Invoke(index);
        }

        private void ClearKeyItemSelection()
        {
            selectedKeyItemIndex = -1;
            foreach (ViewSlot slot in keySlots)
                if (slot.selection) slot.selection.SetActive(false);
        }

        private void UpdateKeyGridLayout()
        {
            float cellSize = Mathf.Max(8, keyItemCellSize);
            float spacing = Mathf.Max(0, keyItemSpacing);
            RectTransform viewport = keyContent.parent as RectTransform;
            float width = viewport ? viewport.rect.width : keyContent.rect.width;
            int columns = Mathf.Max(1, Mathf.FloorToInt((width + spacing) / (cellSize + spacing)));
            Vector2 size = new Vector2(cellSize, cellSize);
            Vector2 gap = new Vector2(spacing, spacing);
            if (keyGrid.cellSize != size) keyGrid.cellSize = size;
            if (keyGrid.spacing != gap) keyGrid.spacing = gap;
            if (keyGrid.constraintCount != columns) keyGrid.constraintCount = columns;

            int rows = (keySlots.Count + columns - 1) / columns;
            float height = rows > 0 ? rows * cellSize + (rows - 1) * spacing : 0;
            if (viewport) height = Mathf.Max(height, viewport.rect.height);
            if (!Mathf.Approximately(keyContent.rect.height, height))
                keyContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        private ViewSlot CreateKeySlot(ViewItem item, int index)
        {
            var obj = new GameObject("KeyItem_" + index, typeof(RectTransform), typeof(Image), typeof(Button));
            obj.transform.SetParent(keyContent, false);
            var background = obj.GetComponent<Image>();
            background.color = keyItemBackgroundColor;
            background.raycastTarget = true;
            var button = obj.GetComponent<Button>();
            button.targetGraphic = background;
            
            button.onClick.AddListener(
                () => SelectKeyItem(index)
            );

            var iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObject.transform.SetParent(obj.transform, false);
            var iconRect = (RectTransform)iconObject.transform;
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = new Vector2(4, 4);
            iconRect.offsetMax = new Vector2(-4, -4);
            var icon = iconObject.GetComponent<Image>();
            icon.sprite = item.icon;
            icon.enabled = item.icon;
            icon.preserveAspect = true;
            icon.raycastTarget = false;

            var selection = new GameObject("Selection", typeof(RectTransform));
            selection.transform.SetParent(obj.transform, false);
            var selectionRect = (RectTransform)selection.transform;
            selectionRect.anchorMin = Vector2.zero;
            selectionRect.anchorMax = Vector2.one;
            selectionRect.offsetMin = selectionRect.offsetMax = Vector2.zero;
            AddKeyBorder(selectionRect, "Top", new Vector2(0, 1), Vector2.one, new Vector2(0, -2), Vector2.zero);
            AddKeyBorder(selectionRect, "Bottom", Vector2.zero, new Vector2(1, 0), Vector2.zero, new Vector2(0, 2));
            AddKeyBorder(selectionRect, "Left", Vector2.zero, new Vector2(0, 1), Vector2.zero, new Vector2(2, 0));
            AddKeyBorder(selectionRect, "Right", new Vector2(1, 0), Vector2.one, new Vector2(-2, 0), Vector2.zero);
            selection.SetActive(false);

            return new ViewSlot { label = item.title, button = button, icon = icon, selection = selection, item = item };
        }

        private void AddKeyBorder(Transform parent, string keyItemName, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            var obj = new GameObject(keyItemName, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(parent, false);
            var rect = (RectTransform)obj.transform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            var image = obj.GetComponent<Image>();
            image.color = keyItemSelectionColor;
            image.raycastTarget = false;
        }
    }
}

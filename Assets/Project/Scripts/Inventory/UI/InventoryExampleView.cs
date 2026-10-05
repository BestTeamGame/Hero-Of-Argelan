using System;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.Inventory.UI
{
    [Serializable]
    public class ExampleItem
    {
        public Sprite icon;
        public string title;
        public string category;
        [TextArea] public string description;
        [TextArea] public string stats;
        [TextArea] public string effects;
    }

    [Serializable]
    public class ExampleSlot
    {
        public string label;
        public Button button;
        public Image icon;
        public GameObject selection;
        public GameObject emptyHint;
        public ExampleItem item;
    }

    // Visual example only. Supply data from your inventory through the public methods.
    public sealed class InventoryExampleView : MonoBehaviour
    {
        public ExampleSlot[] slots;
        public Image detailIcon;
        public Text detailTitle, detailCategory, detailDescription, detailStats, detailEffects, selectedLabel;
        public Text hpCount, manaCount, coinCount, arrowCount;
        public Image cooldownShade;
        public Text cooldownText;
        public Button closeButton;
        public RectTransform window, keyContent;
        public Font textFont;
        public Color textColor = new Color32(231, 224, 205, 255);
        [Tooltip("Starts a 4.2-second cooldown when Play begins. Disable when connecting game data.")]
        public bool demonstrateCooldown = true;
        public int selectedIndex;
        private float remaining, duration;
        public event Action<int> SelectionChanged;

        private void Awake()
        {
            if (slots != null)
                for (int i = 0; i < slots.Length; i++)
                {
                    int index = i;
                    if (slots[i].button != null)
                        slots[i].button.onClick.AddListener(() => SelectSlot(index));
                }
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            SelectSlot(selectedIndex);
        }

        private void Start()
        {
            if (demonstrateCooldown) SetSkillCooldown(4.2f, 6f);
        }

        private void Update()
        {
            if (remaining <= 0f) return;
            remaining = Mathf.Max(0f, remaining - Time.unscaledDeltaTime);
            RefreshCooldown();
        }

        public void Open() { window.gameObject.SetActive(true); }
        public void Close() { window.gameObject.SetActive(false); }

        // Slot order: weapon I, weapon II, active skill, head, body, artifacts I/II/III.
        public void SetSlot(int index, ExampleItem item)
        {
            if (slots == null || index < 0 || index >= slots.Length) return;
            slots[index].item = item;
            slots[index].icon.sprite = item != null ? item.icon : null;
            slots[index].icon.enabled = item != null && item.icon != null;
            if (slots[index].emptyHint != null) slots[index].emptyHint.SetActive(item == null);
            if (selectedIndex == index) SelectSlot(index);
        }

        public void SelectSlot(int index)
        {
            if (slots == null || index < 0 || index >= slots.Length) return;
            selectedIndex = index;
            for (int i = 0; i < slots.Length; i++) slots[i].selection.SetActive(i == index);
            ExampleItem item = slots[index].item;
            detailIcon.sprite = item != null ? item.icon : null;
            detailIcon.enabled = item != null && item.icon != null;
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

        // Unique items only; no fixed number of placeholders or stacks.
        public void SetKeyItems(ExampleItem[] items)
        {
            for (int i = keyContent.childCount - 1; i >= 0; i--)
            {
                GameObject child = keyContent.GetChild(i).gameObject;
                child.SetActive(false);
                if (Application.isPlaying) Destroy(child); else DestroyImmediate(child);
            }
            int count = items == null ? 0 : items.Length;
            float available = ((RectTransform)keyContent.parent).rect.width;
            keyContent.sizeDelta = new Vector2(Mathf.Max(available, count * 146f), 28f);
            keyContent.anchoredPosition = Vector2.zero;
            if (count == 0)
            {
                AddKeyLabel(keyContent, "Ключевых предметов пока нет", 0f, 320f);
                return;
            }
            for (int i = 0; i < count; i++)
            {
                if (items[i] == null) continue;
                var row = new GameObject("KeyItem_" + i, typeof(RectTransform));
                row.transform.SetParent(keyContent, false);
                var rect = (RectTransform)row.transform;
                rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
                rect.anchoredPosition = new Vector2(i * 146f, 0);
                rect.sizeDelta = new Vector2(146, 28);
                var iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconObject.transform.SetParent(rect, false);
                var iconRect = (RectTransform)iconObject.transform;
                iconRect.anchorMin = iconRect.anchorMax = iconRect.pivot = new Vector2(0, 1);
                iconRect.anchoredPosition = new Vector2(0, -2);
                iconRect.sizeDelta = new Vector2(24, 24);
                var icon = iconObject.GetComponent<Image>();
                icon.sprite = items[i].icon;
                icon.enabled = items[i].icon != null;
                icon.preserveAspect = true;
                icon.raycastTarget = false;
                AddKeyLabel(rect, items[i].title, 30, 110);
            }
        }

        private void AddKeyLabel(Transform parent, string value, float x, float width)
        {
            var obj = new GameObject("Name", typeof(RectTransform), typeof(Text));
            obj.transform.SetParent(parent, false);
            var rect = (RectTransform)obj.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, 0);
            rect.sizeDelta = new Vector2(width, 28);
            var text = obj.GetComponent<Text>();
            text.font = textFont;
            text.fontSize = 9;
            text.alignment = TextAnchor.MiddleLeft;
            text.color = textColor;
            text.supportRichText = false;
            text.raycastTarget = false;
            text.text = value;
        }
    }
}

using UnityEngine;

namespace Project.Scripts.Inventory.Items
{
    public abstract class ItemData : ScriptableObject
    {
        [Header("General")]
        public string itemName;
        [TextArea]
        public string description;
        public Sprite icon;
    }
}

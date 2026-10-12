using UnityEngine;

namespace Project.Scripts.Inventory.Items
{
    public abstract class ItemData : ScriptableObject
    {
        [Header("General")] 
        [SerializeField] private string itemName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private Sprite icon;
        
        public string ItemName => itemName;
        public string Description => description;
        public Sprite Icon => icon;
    }
}

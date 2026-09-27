using System.Collections.Generic;
using Project.Scripts.Stats;
using UnityEngine;

namespace Project.Scripts.Inventory.Items
{
    public class EquippableItemData: ItemData
    {
        [SerializeField] private List<StatModifier> statModifiers = new();
        [SerializeField] private List<ItemEffect> itemEffects = new();
        
        public IReadOnlyList<StatModifier> StatModifiers => statModifiers;
        public IReadOnlyList<ItemEffect> ItemEffects => itemEffects;
    }
}

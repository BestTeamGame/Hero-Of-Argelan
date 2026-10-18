using System.Collections.Generic;
using Project.Scripts.Stats;
using UnityEngine;

namespace Project.Scripts.Inventory.Items
{
    public class KeyItemData: ItemData
    {
        [SerializeField] private List<ItemEffect> itemEffects = new();
        
        public IReadOnlyList<ItemEffect> ItemEffects => itemEffects;
    }
}

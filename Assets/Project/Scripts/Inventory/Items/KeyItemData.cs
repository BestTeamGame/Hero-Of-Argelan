using System.Collections.Generic;
using Project.Scripts.Stats;
using UnityEngine;

namespace Project.Scripts.Inventory.Items
{
    [CreateAssetMenu(
        menuName = "Project/Key Items/Default Key Item",
        fileName = "DefaultKeyItem"
    )]
    public class KeyItemData: ItemData
    {
        [SerializeField] private List<ItemEffect> itemEffects = new();
        
        public IReadOnlyList<ItemEffect> ItemEffects => itemEffects;
    }
}

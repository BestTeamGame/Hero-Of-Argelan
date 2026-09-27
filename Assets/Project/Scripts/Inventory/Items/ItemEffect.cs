using UnityEngine;

namespace Project.Scripts.Inventory.Items
{
    public abstract class ItemEffect: ScriptableObject
    {
        public virtual void OnEquip(PlayerContext player) { }
        public virtual void OnUnequip(PlayerContext player) { }
    }
}

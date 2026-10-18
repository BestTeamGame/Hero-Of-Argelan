using Project.Scripts.PlayerScripts;
using UnityEngine;

namespace Project.Scripts.Inventory.Items
{
    public abstract class ItemEffect: ScriptableObject
    {
        [SerializeField] private string effectName;

        [TextArea]
        [SerializeField] private string description;

        public string EffectName => effectName;
        
        public virtual void OnEquip(PlayerContext player) { }
        public virtual void OnUnequip(PlayerContext player) { }

        /// <summary>
        /// Получает правильное описание эффекта для конкретного предмета.
        /// </summary>
        /// <returns>Строка с описанием эффекта.</returns>
        /// <remarks>
        /// Необходимо для подставления актуальных значений в строку, если они изменятся.
        /// </remarks>
        public virtual string GetDescription()
        {
            return description;
        }
    }
}

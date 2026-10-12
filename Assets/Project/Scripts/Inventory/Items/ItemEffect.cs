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
        
        public virtual void OnApply(PlayerContext context) { }

        public virtual void OnRemove(PlayerContext context) { }

        public virtual void OnEnemyKilled(PlayerContext context) { }
        
        public virtual void OnDamageTaken(
            PlayerContext context,
            float damage)
        {
        }

        public virtual void OnDamageDealt(
            PlayerContext context,
            float damage)
        {
        }

        public virtual void OnAttack(PlayerContext context) { }
    }
}

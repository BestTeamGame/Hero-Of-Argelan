using System.Collections.Generic;
using Project.Scripts.Inventory.Items;
using UnityEngine;

namespace Project.Scripts.PlayerScripts
{
    public class PlayerEffects : MonoBehaviour
    {
        private PlayerContext context;

        private readonly List<ItemEffect> activeEffects = new();

        private void Awake()
        {
            context = GetComponent<PlayerContext>();
        }

        private void OnEnable()
        {
            context.CombatEvents.EnemyKilled += OnEnemyKilled;
            context.CombatEvents.DamageTaken += OnDamageTaken;
            context.CombatEvents.DamageDealt += OnDamageDealt;
            context.CombatEvents.AttackPerformed += OnAttackPerformed;
        }

        private void OnDisable()
        {
            context.CombatEvents.EnemyKilled -= OnEnemyKilled;
            context.CombatEvents.DamageTaken -= OnDamageTaken;
            context.CombatEvents.DamageDealt -= OnDamageDealt;
            context.CombatEvents.AttackPerformed -= OnAttackPerformed;
        }

        public void AddEffect(ItemEffect effect)
        {
            if (!effect)
                return;

            activeEffects.Add(effect);
            effect.OnApply(context);
        }

        public bool RemoveEffect(ItemEffect effect)
        {
            if (!effect)
                return false;

            if (!activeEffects.Remove(effect))
                return false;

            effect.OnRemove(context);

            return true;
        }

        private void OnEnemyKilled()
        {
            foreach (ItemEffect effect in activeEffects)
            {
                effect.OnEnemyKilled(context);
            }
        }

        private void OnDamageTaken(float damage)
        {
            foreach (ItemEffect effect in activeEffects)
            {
                effect.OnDamageTaken(context, damage);
            }
        }

        private void OnDamageDealt(float damage)
        {
            foreach (ItemEffect effect in activeEffects)
            {
                effect.OnDamageDealt(context, damage);
            }
        }

        private void OnAttackPerformed()
        {
            foreach (ItemEffect effect in activeEffects)
            {
                effect.OnAttack(context);
            }
        }
    }
}
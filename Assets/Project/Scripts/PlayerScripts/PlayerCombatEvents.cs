using System;
using UnityEngine;

namespace Project.Scripts.PlayerScripts
{
    public class PlayerCombatEvents : MonoBehaviour
    {
        // События
        public event Action EnemyKilled;

        public event Action<float> DamageTaken;
        public event Action<float> DamageDealt;

        public event Action AttackPerformed;
        
        // Методы для вызова событий

        public void RaiseEnemyKilled()
        {
            EnemyKilled?.Invoke();
        }

        public void RaiseDamageTaken(float damage)
        {
            if (damage <= 0)
                return;

            DamageTaken?.Invoke(damage);
        }

        public void RaiseDamageDealt(float damage)
        {
            if (damage <= 0)
                return;

            DamageDealt?.Invoke(damage);
        }

        public void RaiseAttackPerformed()
        {
            AttackPerformed?.Invoke();
        }
    }
}
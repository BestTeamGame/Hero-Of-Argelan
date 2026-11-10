using Project.Scripts.PlayerScripts;
using UnityEngine;

namespace Project.Scripts.Combat
{
    public readonly struct WeaponAttackContext
    {
        public PlayerContext Player { get; }

        public Transform Origin { get; }

        public Vector2 Direction { get; }

        public float Damage { get; }

        public WeaponAttackContext(
            PlayerContext player,
            Transform origin,
            Vector2 direction,
            float damage)
        {
            Player = player;
            Origin = origin;
            Direction = direction;
            Damage = damage;
        }
    }
}
using Project.Scripts.PlayerScripts;
using UnityEngine;

namespace Project.Scripts.Combat
{
    public readonly struct SkillUseContext
    {
        public PlayerContext Player { get; }

        public Transform Origin { get; }

        public Vector2 Direction { get; }

        public SkillUseContext(
            PlayerContext player,
            Transform origin,
            Vector2 direction)
        {
            Player = player;
            Origin = origin;
            Direction = direction;
        }
    }
}
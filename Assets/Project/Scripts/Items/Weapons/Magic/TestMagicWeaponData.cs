using System.Collections.Generic;
using Project.Scripts.Combat;
using Project.Scripts.Inventory.Items;
using Project.Scripts.Stats;
using UnityEngine;

namespace Project.Scripts.Items.Weapons.Magic
{
    [CreateAssetMenu(
        menuName = "Project/Weapons/Test Magic Weapon",
        fileName = "TestMagicWeapon"
    )]
    public sealed class TestMagicWeaponData : MagicWeaponData
    {
        [SerializeField, Min(0f)]
        private float radius = 1.25f;

        [SerializeField, Min(0f)]
        private float forwardOffset = 0.8f;

        [SerializeField]
        private LayerMask targetLayers = 1 << 7;

        private readonly Collider2D[] hitBuffer = new Collider2D[16];
        private readonly HashSet<Enemy> damagedEnemies = new();

        public override StatType DamageStat => StatType.MagicDamage;

        public override void Attack(WeaponAttackContext context)
        {
            if (!context.Origin)
                return;

            Vector2 direction = context.Direction.normalized;
            Vector2 center =
                (Vector2)context.Origin.position + direction * forwardOffset;

            var filter = new ContactFilter2D();
            filter.SetLayerMask(targetLayers);
            filter.useTriggers = true;

            int hitCount = Physics2D.OverlapCircle(
                center,
                radius,
                filter,
                hitBuffer
            );

            damagedEnemies.Clear();
            int damage = Mathf.CeilToInt(context.Damage);

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hit = hitBuffer[i];
                hitBuffer[i] = null;

                if (!hit)
                    continue;

                Enemy enemy = hit.GetComponentInParent<Enemy>();
                if (!enemy || !damagedEnemies.Add(enemy))
                    continue;

                enemy.TakeDamage(damage);
                context.Player.CombatEvents.RaiseDamageDealt(damage);
            }
        }
    }
}

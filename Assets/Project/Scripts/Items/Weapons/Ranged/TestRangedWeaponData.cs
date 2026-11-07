using Project.Scripts.Combat;
using Project.Scripts.Inventory.Items;
using Project.Scripts.PlayerScripts;
using Project.Scripts.Stats;
using UnityEngine;

namespace Project.Scripts.Items.Weapons.Ranged
{
    [CreateAssetMenu(
        menuName = "Project/Weapons/Test Ranged Weapon",
        fileName = "TestRangedWeapon"
    )]
    public sealed class TestRangedWeaponData : RangedWeaponData
    {
        [SerializeField]
        private GameObject projectilePrefab;

        [SerializeField, Min(0f)]
        private float projectileSpeed = 12f;

        [SerializeField]
        private LayerMask targetLayers = 1 << 7;

        public override StatType DamageStat => StatType.RangedDamage;

        public override bool CanUse(PlayerContext context)
        {
            return projectilePrefab && base.CanUse(context);
        }

        public override void Attack(WeaponAttackContext context)
        {
            if (!projectilePrefab || !context.Origin)
                return;

            Vector2 direction = context.Direction.normalized;
            if (Mathf.Approximately(direction.x, 0f))
                direction = Vector2.right;

            Vector2 spawnPosition = context.Origin.position;

            GameObject projectile = Instantiate(
                projectilePrefab,
                spawnPosition,
                Quaternion.identity
            );

            Arrow arrow = projectile.GetComponent<Arrow>();
            if (!arrow)
            {
                Debug.LogError(
                    $"Projectile prefab '{projectilePrefab.name}' " +
                    $"does not have a {nameof(Arrow)} component."
                );
                Destroy(projectile);
                return;
            }

            arrow.Initialize(
                Mathf.Sign(direction.x),
                projectileSpeed,
                Mathf.CeilToInt(context.Damage),
                targetLayers
            );
        }
    }
}

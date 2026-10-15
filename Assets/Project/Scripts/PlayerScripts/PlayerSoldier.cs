using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Scripts
{
    public class PlayerSoldier : Player
    {
        private static readonly int Attack =
            Animator.StringToHash("Attack");

        private static readonly int BowAttack =
            Animator.StringToHash("BowAttack");

        [Header("Soldier Attack")]
        public Transform attackPoint;
        public float attackRange = 0.6f;
        public LayerMask enemyLayer;
        public float attackCooldown = 0.5f;
        public int attackDamage = 25;

        [Header("Bow Attack")]
        public Transform arrowPoint;
        public GameObject arrowPrefab;
        public float arrowSpeed = 12f;
        public int arrowDamage = 25;
        public float bowCooldown = 1f;

        private ContactFilter2D enemyFilter;

        private readonly Collider2D[] hitEnemies =
            new Collider2D[10];

        private float lastAttackTime = -Mathf.Infinity;
        private float lastBowAttackTime = -Mathf.Infinity;

        protected override void Awake()
        {
            base.Awake();

            enemyFilter = new ContactFilter2D();

            enemyFilter.SetLayerMask(enemyLayer);
            enemyFilter.useLayerMask = true;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (input == null)
                return;


            input.Player.Attack.performed += OnAttack;


            input.Player.BowAttack.performed += OnBowAttack;
        }

        protected override void OnDisable()
        {
            if (input != null)
            {
                input.Player.Attack.performed -= OnAttack;
                input.Player.BowAttack.performed -= OnBowAttack;
            }

            base.OnDisable();
        }

        private void OnAttack(
            InputAction.CallbackContext context)
        {
            if (isDead)
                return;

            if (Time.time <
                lastAttackTime + attackCooldown)
                return;

            lastAttackTime = Time.time;

            AttackEnemy();
        }

        private void AttackEnemy()
        {
            if (animator != null)
            {
                animator.SetTrigger(Attack);
            }

            if (attackPoint == null)
                return;

            int hitCount = Physics2D.OverlapCircle(
                attackPoint.position,
                attackRange,
                enemyFilter,
                hitEnemies
            );

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D enemyCollider =
                    hitEnemies[i];

                if (enemyCollider == null)
                    continue;

                Enemy enemy =
                    enemyCollider.GetComponent<Enemy>();

                if (enemy == null)
                {
                    enemy =
                        enemyCollider.GetComponentInParent<Enemy>();
                }

                if (enemy != null)
                {
                    Debug.Log(
                        "Ударили: " +
                        enemy.name
                    );

                    enemy.TakeDamage(attackDamage);
                }
            }
        }


        private void OnBowAttack(
            InputAction.CallbackContext context)
        {
            if (isDead)
                return;

            if (Time.time <
                lastBowAttackTime + bowCooldown)
                return;

            if (arrowPrefab == null)
            {
                Debug.LogWarning(
                    "Arrow Prefab не назначен!"
                );

                return;
            }

            if (arrowPoint == null)
            {
                Debug.LogWarning(
                    "Arrow Point не назначен!"
                );

                return;
            }

            lastBowAttackTime = Time.time;

            if (animator != null)
            {
                animator.SetTrigger(BowAttack);
            }
        }

        public void ShootArrow()
        {
            if (isDead)
                return;

            if (arrowPrefab == null || arrowPoint == null)
                return;

            GameObject arrowObject = Instantiate(
                arrowPrefab,
                arrowPoint.position,
                Quaternion.identity
            );

            Arrow arrow = arrowObject.GetComponent<Arrow>();

            if (arrow != null)
            {
                float direction = arrowPoint.position.x >= transform.position.x
                    ? 1f
                    : -1f;

                arrow.Initialize(
                    direction,
                    arrowSpeed,
                    arrowDamage,
                    enemyLayer
                );
            }
        }

        protected override void Flip()
        {
            base.Flip();

            if (attackPoint != null)
            {
                Vector3 position =
                    attackPoint.localPosition;

                position.x = -position.x;

                attackPoint.localPosition =
                    position;
            }

            if (arrowPoint != null)
            {
                Vector3 position =
                    arrowPoint.localPosition;

                position.x = -position.x;

                arrowPoint.localPosition =
                    position;
            }
        }


        private void OnDrawGizmosSelected()
        {
            if (attackPoint != null)
            {
                Gizmos.color = Color.yellow;

                Gizmos.DrawWireSphere(
                    attackPoint.position,
                    attackRange
                );
            }

            if (arrowPoint != null)
            {
                Gizmos.color = Color.red;

                Gizmos.DrawWireSphere(
                    arrowPoint.position,
                    0.08f
                );
            }
        }
    }
}
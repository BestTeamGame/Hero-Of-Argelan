
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Scripts
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int Attack1 = Animator.StringToHash("Attack");
        private static readonly int Hurt = Animator.StringToHash("Hurt");
        private static readonly int Death = Animator.StringToHash("Death");

        [Header("Movement")]
        public float moveSpeed = 5f;
        public float jumpForce = 12f;

        [Header("Ground Check")]
        public Transform groundCheck;
        public float groundCheckRadius = 0.15f;
        public LayerMask groundLayer;

        [Header("Attack")]
        public Transform attackPoint;
        public float attackRange = 0.6f;
        public LayerMask enemyLayer;
        public float attackCooldown = 0.5f;
        public int attackDamage = 25;

        [Header("Health")]
        public int maxHealth = 100;
        private int currentHealth;

        private Rigidbody2D rb;
        private SpriteRenderer sr;
        private Animator animator;
        private ContactFilter2D enemyFilter;

        private InputSystem_Actions input;

        private readonly Collider2D[] hitEnemies = new Collider2D[10];

        private bool jumpRequested;
        private bool isGrounded;
        private bool facingRight = true;
        private bool isDead;

        private float moveInput;
        private float lastAttackTime = -Mathf.Infinity;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            sr = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();

            input = new InputSystem_Actions();

            currentHealth = maxHealth;

            enemyFilter = new ContactFilter2D();
            enemyFilter.SetLayerMask(enemyLayer);
            enemyFilter.useLayerMask = true;
        }

        private void OnEnable()
        {
            input.Player.Enable();

            input.Player.Jump.performed += OnJump;
            input.Player.Attack.performed += OnAttack;
        }

        private void OnDisable()
        {
            input.Player.Jump.performed -= OnJump;
            input.Player.Attack.performed -= OnAttack;

            input.Player.Disable();
        }

        private void Update()
        {
            if (isDead)
            {
                moveInput = 0f;

                if (animator)
                    animator.SetFloat(Speed, 0f);

                return;
            }

            Vector2 move = input.Player.Move.ReadValue<Vector2>();
            moveInput = move.x;

            isGrounded = Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                groundLayer
            );

            if (moveInput > 0f && !facingRight ||
                moveInput < 0f && facingRight)
            {
                Flip();
            }

            if (animator)
            {
                animator.SetFloat(Speed, Mathf.Abs(moveInput));
            }
        }

        private void FixedUpdate()
        {
            if (isDead)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                return;
            }

            isGrounded = Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                groundLayer
            );

            rb.linearVelocity = new Vector2(
                moveInput * moveSpeed,
                rb.linearVelocity.y
            );

            if (jumpRequested && isGrounded)
            {
                rb.linearVelocity = new Vector2(
                    rb.linearVelocity.x,
                    jumpForce
                );
            }

            jumpRequested = false;
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            if (isDead)
                return;

            jumpRequested = true;
        }

        private void OnAttack(InputAction.CallbackContext context)
        {
            if (isDead)
                return;

            if (Time.time < lastAttackTime + attackCooldown)
                return;

            lastAttackTime = Time.time;

            Attack();
        }

        private void Flip()
        {
            facingRight = !facingRight;
            sr.flipX = !facingRight;

            if (!attackPoint)
                return;

            Vector3 pos = attackPoint.localPosition;
            pos.x = -pos.x;
            attackPoint.localPosition = pos;
        }

        private void Attack()
        {
            if (animator != null)
            {
                animator.SetTrigger(Attack1);
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
                Collider2D enemyCollider = hitEnemies[i];

                if (enemyCollider == null)
                    continue;

                Enemy enemy = enemyCollider.GetComponent<Enemy>();

                if (enemy == null)
                {
                    enemy = enemyCollider.GetComponentInParent<Enemy>();
                }

                if (enemy != null)
                {
                    Debug.Log("Ударили: " + enemy.name);

                    enemy.TakeDamage(attackDamage);
                }
            }
        }

        public void TakeDamage(int damage)
        {
            if (isDead)
                return;

            currentHealth -= damage;

            if (currentHealth < 0)
                currentHealth = 0;

            Debug.Log("Игрок получил урон: " + damage +
                      ". Здоровье: " + currentHealth + "/" + maxHealth);

            if (animator != null)
            {
                animator.SetTrigger(Hurt);
            }

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(int amount)
        {
            if (isDead)
                return;

            currentHealth += amount;

            if (currentHealth > maxHealth)
                currentHealth = maxHealth;

            Debug.Log("Игрок восстановил здоровье: " +
                      currentHealth + "/" + maxHealth);
        }

        private void Die()
        {
            if (isDead)
                return;

            isDead = true;

            moveInput = 0f;
            jumpRequested = false;

            rb.linearVelocity = Vector2.zero;

            if (animator != null)
            {
                animator.SetFloat(Speed, 0f);
                animator.SetTrigger(Death);
            }

            Debug.Log("Игрок умер");
        }

        public int GetCurrentHealth()
        {
            return currentHealth;
        }

        public int GetMaxHealth()
        {
            return maxHealth;
        }

        public bool IsDead()
        {
            return isDead;
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = Color.red;

                Gizmos.DrawWireSphere(
                    groundCheck.position,
                    groundCheckRadius
                );
            }

            if (attackPoint != null)
            {
                Gizmos.color = Color.yellow;

                Gizmos.DrawWireSphere(
                    attackPoint.position,
                    attackRange
                );
            }
        }
    }
}

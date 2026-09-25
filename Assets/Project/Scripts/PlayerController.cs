using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Scripts
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int Attack1 = Animator.StringToHash("Attack");

        [Header("Movement")]
        public float moveSpeed = 5f;
        public float jumpForce = 12f;

        [Header("Ground Check")]
        public Transform groundCheck;      // пустой объект у ног персонажа
        public float groundCheckRadius = 0.15f;
        public LayerMask groundLayer;      // сюда назначить слой terrain/земли

        [Header("Attack")]
        public Transform attackPoint;      // пустой объект перед персонажем
        public float attackRange = 0.6f;
        public LayerMask enemyLayer;       // сюда назначить слой врагов
        public float attackCooldown = 0.5f;
        

        private Rigidbody2D rb;
        private SpriteRenderer sr;
        private Animator animator;
        private ContactFilter2D enemyFilter;
        
        private InputSystem_Actions input;
        
        private readonly Collider2D[] hitEnemies = new Collider2D[10];
        
        private bool jumpRequested;
        private bool isGrounded;
        private bool facingRight = true;
        private float moveInput;
        private float lastAttackTime = -Mathf.Infinity;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            sr = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            
            input = new InputSystem_Actions();
            
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
            Vector2 move = input.Player.Move.ReadValue<Vector2>();
            moveInput = move.x;

            isGrounded = Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                groundLayer
            );

            if (moveInput > 0f && !facingRight || moveInput < 0f && facingRight)
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
            jumpRequested = true;
        }

        private void OnAttack(InputAction.CallbackContext context)
        {
            if (Time.time < lastAttackTime + attackCooldown)
                return;

            lastAttackTime = Time.time;
            Attack();
        }

        private void Flip()
        {
            facingRight = !facingRight;
            sr.flipX = !facingRight;

            if (!attackPoint) return;
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
                Collider2D enemy = hitEnemies[i];

                Debug.Log("Ударили: " + enemy.name);

                // Позже:
                // enemy.GetComponent<EnemyHealth>()?.TakeDamage(damage);
            }
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
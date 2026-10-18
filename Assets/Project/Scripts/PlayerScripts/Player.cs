using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Scripts
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Player : MonoBehaviour
    {
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int Hurt = Animator.StringToHash("Hurt");
        private static readonly int Death = Animator.StringToHash("Death");

        [Header("Movement")]
        public float moveSpeed = 5f;
        public float jumpForce = 12f;

        [Header("Ground Check")]
        public Transform groundCheck;
        public float groundCheckRadius = 0.15f;
        public LayerMask groundLayer;

        [Header("Health")]
        public int maxHealth = 100;

        protected int currentHealth;

        protected Rigidbody2D rb;
        protected SpriteRenderer sr;
        protected Animator animator;

        protected InputSystem_Actions input;

        protected bool jumpRequested;
        protected bool isGrounded;
        protected bool facingRight = true;
        protected bool isDead;

        protected float moveInput;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            sr = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();

            input = new InputSystem_Actions();

            currentHealth = maxHealth;
        }

        protected virtual void OnEnable()
        {
            if (input == null)
                return;

            input.Player.Enable();

            input.Player.Jump.performed += OnJump;
        }

        protected virtual void OnDisable()
        {
            if (input == null)
                return;

            input.Player.Jump.performed -= OnJump;

            input.Player.Disable();
        }

        protected virtual void Update()
        {
            if (isDead)
            {
                moveInput = 0f;

                if (animator != null)
                    animator.SetFloat(Speed, 0f);

                return;
            }

            Vector2 move = input.Player.Move.ReadValue<Vector2>();

            moveInput = move.x;

            if (groundCheck != null)
            {
                isGrounded = Physics2D.OverlapCircle(
                    groundCheck.position,
                    groundCheckRadius,
                    groundLayer
                );
            }

            if ((moveInput > 0f && !facingRight) ||
                (moveInput < 0f && facingRight))
            {
                Flip();
            }

            if (animator != null)
            {
                animator.SetFloat(
                    Speed,
                    Mathf.Abs(moveInput)
                );
            }
        }

        protected virtual void FixedUpdate()
        {
            if (isDead)
            {
                rb.linearVelocity = new Vector2(
                    0f,
                    rb.linearVelocity.y
                );

                return;
            }

            if (groundCheck != null)
            {
                isGrounded = Physics2D.OverlapCircle(
                    groundCheck.position,
                    groundCheckRadius,
                    groundLayer
                );
            }

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

        protected virtual void OnJump(
            InputAction.CallbackContext context)
        {
            if (isDead)
                return;

            jumpRequested = true;
        }

        protected virtual void Flip()
        {
            facingRight = !facingRight;

            if (sr != null)
                sr.flipX = !facingRight;
        }

        public virtual void TakeDamage(int damage)
        {
            if (isDead)
                return;

            currentHealth -= damage;

            if (currentHealth < 0)
                currentHealth = 0;

            Debug.Log(
                "Игрок получил урон: " +
                damage +
                ". Здоровье: " +
                currentHealth +
                "/" +
                maxHealth
            );

            if (animator != null)
            {
                animator.SetTrigger(Hurt);
            }

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public virtual void Heal(int amount)
        {
            if (isDead)
                return;

            currentHealth += amount;

            if (currentHealth > maxHealth)
                currentHealth = maxHealth;

            Debug.Log(
                "Игрок восстановил здоровье: " +
                currentHealth +
                "/" +
                maxHealth
            );
        }

        protected virtual void Die()
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

        protected bool IsFacingRight()
        {
            return facingRight;
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
        }
    }
}
using Project.Scripts.Stats;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Scripts.PlayerScripts
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerStats))]
    [RequireComponent(typeof(PlayerHealth))]
    public class PlayerMovement : MonoBehaviour
    {
        private static readonly int Speed =
            Animator.StringToHash("Speed");

        [Header("Movement")]
        [SerializeField] private float baseMoveSpeed = 5f;
        [SerializeField] private float jumpForce = 12f;

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.15f;
        [SerializeField] private LayerMask groundLayer;

        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private Animator animator;

        private PlayerHealth health;
        private PlayerStats stats;

        private InputSystem_Actions input;

        private float moveInput;

        private bool jumpRequested;
        private bool isGrounded;
        private bool facingRight = true;

        public bool IsGrounded => isGrounded;
        public bool IsFacingRight => facingRight;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();

            health = GetComponent<PlayerHealth>();
            stats = GetComponent<PlayerStats>();

            input = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            input.Player.Enable();

            input.Player.Jump.performed += OnJump;
        }

        private void OnDisable()
        {
            input.Player.Jump.performed -= OnJump;

            input.Player.Disable();
        }

        private void Update()
        {
            if (health.IsDead)
            {
                moveInput = 0f;

                if (animator)
                    animator.SetFloat(Speed, 0f);

                return;
            }

            Vector2 move = input.Player.Move.ReadValue<Vector2>();

            moveInput = move.x;

            if ((moveInput > 0f && !facingRight) ||
                (moveInput < 0f && facingRight))
            {
                Flip();
            }

            if (animator)
            {
                animator.SetFloat(
                    Speed,
                    Mathf.Abs(moveInput)
                );
            }
        }

        private void FixedUpdate()
        {
            UpdateGrounded();

            if (health.IsDead)
            {
                rb.linearVelocity = new Vector2(
                    0f,
                    rb.linearVelocity.y
                );

                return;
            }

            float movementMultiplier =
                stats.GetStat(StatType.MovementSpeed);

            float movementSpeed =
                baseMoveSpeed * movementMultiplier;

            rb.linearVelocity = new Vector2(
                moveInput * movementSpeed,
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

        private void OnJump(
            InputAction.CallbackContext context)
        {
            if (health.IsDead)
                return;

            jumpRequested = true;
        }

        private void UpdateGrounded()
        {
            if (!groundCheck)
            {
                isGrounded = false;
                return;
            }

            isGrounded = Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                groundLayer
            );
        }

        private void Flip()
        {
            facingRight = !facingRight;

            if (spriteRenderer)
                spriteRenderer.flipX = !facingRight;
        }

        private void OnDrawGizmosSelected()
        {
            if (!groundCheck)
                return;

            Gizmos.color = Color.red;

            Gizmos.DrawWireSphere(
                groundCheck.position,
                groundCheckRadius
            );
        }
    }
}
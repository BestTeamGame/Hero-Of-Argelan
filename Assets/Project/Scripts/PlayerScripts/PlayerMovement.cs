using Project.Scripts.Stats;
using UnityEngine;

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
        [SerializeField, Min(0f)]
        private float baseMoveSpeed = 5f;

        [SerializeField, Min(0f)]
        private float jumpForce = 12f;

        [Header("Ground Check")]
        [SerializeField]
        private Transform groundCheck;

        [SerializeField, Min(0f)]
        private float groundCheckRadius = 0.15f;

        [SerializeField]
        private LayerMask groundLayer;

        private Rigidbody2D rigidbody;
        private SpriteRenderer spriteRenderer;
        private Animator animator;

        private PlayerStats stats;
        private PlayerHealth health;

        private float moveInput;

        private bool jumpRequested;
        private bool isGrounded;
        private bool facingRight = true;

        public bool IsGrounded => isGrounded;
        public bool IsFacingRight => facingRight;

        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();

            stats = GetComponent<PlayerStats>();
            health = GetComponent<PlayerHealth>();
        }

        private void Update()
        {
            if (health.IsDead)
            {
                moveInput = 0f;

                UpdateMovementAnimation();

                return;
            }

            UpdateFacingDirection();
            UpdateMovementAnimation();
        }

        private void FixedUpdate()
        {
            UpdateGrounded();

            if (health.IsDead)
            {
                StopHorizontalMovement();
                jumpRequested = false;

                return;
            }

            Move();
            TryJump();

            jumpRequested = false;
        }

        public void SetMoveInput(Vector2 input)
        {
            if (health.IsDead)
            {
                moveInput = 0f;
                return;
            }

            moveInput = Mathf.Clamp(
                input.x,
                -1f,
                1f
            );
        }

        public void RequestJump()
        {
            if (health.IsDead)
                return;

            jumpRequested = true;
        }

        public void Stop()
        {
            moveInput = 0f;
            jumpRequested = false;

            StopHorizontalMovement();
            UpdateMovementAnimation();
        }

        private void Move()
        {
            float movementMultiplier =
                stats.GetStat(
                    StatType.MovementSpeed
                );

            float movementSpeed =
                baseMoveSpeed *
                movementMultiplier;

            rigidbody.linearVelocity = new Vector2(
                moveInput * movementSpeed,
                rigidbody.linearVelocity.y
            );
        }

        private void TryJump()
        {
            if (!jumpRequested || !isGrounded)
                return;

            rigidbody.linearVelocity = new Vector2(
                rigidbody.linearVelocity.x,
                jumpForce
            );

            isGrounded = false;
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

        private void UpdateFacingDirection()
        {
            if (moveInput > 0f && !facingRight || moveInput < 0f && facingRight)
            {
                Flip();
            }
        }

        private void Flip()
        {
            facingRight = !facingRight;

            if (spriteRenderer)
            {
                spriteRenderer.flipX =
                    !facingRight;
            }
        }

        private void UpdateMovementAnimation()
        {
            if (!animator)
                return;

            animator.SetFloat(
                Speed,
                Mathf.Abs(moveInput)
            );
        }

        private void StopHorizontalMovement()
        {
            rigidbody.linearVelocity = new Vector2(
                0f,
                rigidbody.linearVelocity.y
            );
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
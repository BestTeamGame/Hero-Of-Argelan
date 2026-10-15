
using Project.Scripts;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class OrcEnemy : Enemy
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float detectionDistance = 7f;
    public float attackDistance = 1.5f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.3f;
    public float edgeCheckDistance = 0.3f;
    public float wallCheckDistance = 0.2f;

    [Header("Attack")]
    public int attackDamage = 20;
    public float attackCooldown = 1.5f;

    [Header("References")]
    public Transform player;
    public Animator animator;

    private Rigidbody2D rb;
    private Collider2D bodyCollider;

    private float attackTimer;
    private bool isAttacking;
    private bool isHurt;

    private int facingDirection = 1;

    protected override void Start()
    {
        base.Start();

        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                player = playerObject.transform;
        }

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.freezeRotation = true;
    }

    private void Update()
    {
        if (isDead)
            return;

        if (player == null || animator == null)
        {
            StopHorizontalMovement();
            SetIdleAnimation();
            return;
        }

        attackTimer -= Time.deltaTime;

        if (isHurt || isAttacking)
            return;

        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        if (distance > detectionDistance)
        {
            StopHorizontalMovement();
            SetIdleAnimation();
            return;
        }

        // ќрк не преследует игрока на другой высоте.
        if (!IsPlayerOnReachableHeight(player))
        {
            StopHorizontalMovement();
            SetIdleAnimation();
            return;
        }

        FacePlayer();

        if (distance <= attackDistance)
        {
            StopHorizontalMovement();
            Attack();
        }
        else
        {
            MoveToPlayer();
        }
    }

    private void FixedUpdate()
    {
        if (isDead || rb == null)
            return;

        if (isAttacking || isHurt || player == null)
        {
            StopHorizontalMovement();
            return;
        }

        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        // ѕовторно провер€ем услови€ непосредственно перед движением.
        if (distance > detectionDistance ||
            distance <= attackDistance ||
            !IsPlayerOnReachableHeight(player) ||
            !IsGroundAhead() ||
            IsWallAhead())
        {
            StopHorizontalMovement();
        }
    }

    private void MoveToPlayer()
    {
        // ≈сли впереди край или стена, остаемс€ на месте.
        if (!IsGroundAhead() || IsWallAhead())
        {
            StopHorizontalMovement();
            SetIdleAnimation();
            return;
        }

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(
                facingDirection * moveSpeed,
                rb.linearVelocity.y
            );
        }

        animator.SetBool("IsWalking", true);
        animator.SetBool("IsIdle", false);
    }

    private void FacePlayer()
    {
        float directionX = player.position.x - transform.position.x;

        if (directionX > 0.05f)
            facingDirection = 1;
        else if (directionX < -0.05f)
            facingDirection = -1;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * facingDirection;
        transform.localScale = scale;
    }

    private bool IsGroundAhead()
    {
        if (groundCheck == null)
            return true;

   
        Vector2 origin = (Vector2)groundCheck.position +
            Vector2.right * facingDirection * edgeCheckDistance;

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        return hit.collider != null;
    }

    private bool IsWallAhead()
    {
        if (bodyCollider == null)
            return false;

        Bounds bounds = bodyCollider.bounds;

        Vector2 origin = new Vector2(
            bounds.center.x,
            bounds.center.y
        );

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            Vector2.right * facingDirection,
            bounds.extents.x + wallCheckDistance,
            groundLayer
        );

        return hit.collider != null;
    }

    private void StopHorizontalMovement()
    {
        if (rb == null)
            return;

        rb.linearVelocity = new Vector2(
            0f,
            rb.linearVelocity.y
        );
    }

    private void SetIdleAnimation()
    {
        if (animator == null)
            return;

        animator.SetBool("IsWalking", false);
        animator.SetBool("IsIdle", true);
    }

    private void Attack()
    {
        if (attackTimer > 0f)
            return;

        attackTimer = attackCooldown;
        isAttacking = true;

        StopHorizontalMovement();

        animator.SetBool("IsWalking", false);
        animator.SetBool("IsIdle", false);

        // —лучайно выбираем одну из двух атак.
        int attackNumber = Random.Range(0, 2);

        if (attackNumber == 0)
            animator.SetTrigger("Attack1");
        else
            animator.SetTrigger("Attack2");
    }

    public override void TakeDamage(int damage)
    {
        if (isDead)
            return;

        base.TakeDamage(damage);

        if (isDead)
            return;

        isHurt = true;
        isAttacking = false;

        StopHorizontalMovement();

        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsIdle", false);
            animator.SetTrigger("Hurt");
        }

        CancelInvoke(nameof(EndHurt));
        Invoke(nameof(EndHurt), 0.5f);
    }

    private void EndHurt()
    {
        if (isDead)
            return;

        isHurt = false;
    }

    protected override void Die()
    {
        if (isDead)
            return;

        isDead = true;
        isAttacking = false;
        isHurt = false;

        CancelInvoke(nameof(EndHurt));

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsIdle", false);
            animator.SetTrigger("Death");
        }

        Destroy(gameObject, 2f);
    }

    // Animation Event в конце каждой атаки.
    public void FinishAttack()
    {
        isAttacking = false;
    }


    public void DealDamage()
    {
        if (isDead || !isAttacking || player == null)
            return;

        if (!IsPlayerOnReachableHeight(player))
            return;

        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        if (distance > attackDistance + 0.3f)
            return;

        Player playerComponent = player.GetComponent<Player>();

        if (playerComponent == null)
            playerComponent = player.GetComponentInParent<Player>();

        if (playerComponent != null)
        {
            Debug.Log("ќрк наносит игроку урон: " + attackDamage);
            playerComponent.TakeDamage(attackDamage);
        }
        else
        {
            Debug.LogWarning("Player не найден!");
        }
    }
}

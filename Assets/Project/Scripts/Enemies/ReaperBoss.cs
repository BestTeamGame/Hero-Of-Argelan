
using Project.Scripts;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class ReaperBoss : Enemy
{
    [Header("Movement")]
    public float moveSpeed = 1.5f;
    public float detectionDistance = 10f;
    public float attackDistance = 2f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.3f;
    public float edgeCheckDistance = 0.3f;
    public float wallCheckDistance = 0.2f;

    [Header("Attack")]
    public int attackDamage = 40;
    public float attackCooldown = 2f;

    [Header("References")]
    public Transform player;
    public Animator animator;
    public Slider healthSlider;

    [Header("Boss Settings")]
    public float hurtDuration = 0.5f;
    public float destroyDelay = 2f;

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

        UpdateHealthBar();
    }

    private void Update()
    {
        if (isDead)
            return;

        if (player == null || animator == null)
        {
            StopHorizontalMovement();
            SetWalkingAnimation(false);
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
            SetWalkingAnimation(false);
            return;
        }

        // Жнец не преследует игрока, если тот находится
        // слишком высоко или низко относительно босса.
        if (!IsPlayerOnReachableHeight(player))
        {
            StopHorizontalMovement();
            SetWalkingAnimation(false);
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

        // Проверяем высоту, расстояние и препятствия перед движением.
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
        // Не позволяем боссу сходить с края или идти сквозь стену.
        if (!IsGroundAhead() || IsWallAhead())
        {
            StopHorizontalMovement();
            SetWalkingAnimation(false);
            return;
        }

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(
                facingDirection * moveSpeed,
                rb.linearVelocity.y
            );
        }

        SetWalkingAnimation(true);
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

        // Проверяем наличие платформы немного впереди ног.
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

    private void SetWalkingAnimation(bool walking)
    {
        if (animator == null)
            return;

        animator.SetBool("IsWalking", walking);
    }

    private void Attack()
    {
        if (attackTimer > 0f)
            return;

        attackTimer = attackCooldown;
        isAttacking = true;

        StopHorizontalMovement();
        SetWalkingAnimation(false);

        animator.SetTrigger("Attack");
    }

    public override void TakeDamage(int damage)
    {
        if (isDead)
            return;

        base.TakeDamage(damage);

        UpdateHealthBar();

        if (isDead)
            return;

        isHurt = true;
        isAttacking = false;

        StopHorizontalMovement();
        CancelInvoke(nameof(EndHurt));

        SetWalkingAnimation(false);
        animator.SetTrigger("Hurt");

        Invoke(nameof(EndHurt), hurtDuration);
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
        StopHorizontalMovement();

        if (rb != null)
            rb.simulated = false;

        SetWalkingAnimation(false);

        if (healthSlider != null)
            healthSlider.value = 0;

        if (animator != null)
            animator.SetTrigger("Death");

        Destroy(gameObject, destroyDelay);
    }

    // Animation Event в конце анимации атаки.
    public void FinishAttack()
    {
        isAttacking = false;
    }

    // Animation Event в кадре попадания оружия.
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
            Debug.Log("Reaper наносит игроку урон: " + attackDamage);
            playerComponent.TakeDamage(attackDamage);
        }
        else
        {
            Debug.LogWarning("Player не найден!");
        }
    }

    private void UpdateHealthBar()
    {
        if (healthSlider == null)
            return;

        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
    }
}

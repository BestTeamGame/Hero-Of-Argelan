
using Project.Scripts;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class GoblinEnemy : Enemy
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float detectionDistance = 10f;
    public float daggerDistance = 1.5f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer;
    public float wallCheckDistance = 0.3f;
    public float edgeCheckDistance = 0.3f;

    [Header("Dagger Attack")]
    public int daggerDamage = 15;
    public float daggerCooldown = 1f;

    [Header("Bomb Attack")]
    public GameObject bombPrefab;
    public Transform bombSpawnPoint;
    public int bombDamage = 25;
    public float bombCooldown = 2.5f;
    public float bombThrowForce = 7f;

    [Header("References")]
    public Transform player;
    public Animator animator;

    private Rigidbody2D rb;
    private Collider2D bodyCollider;

    private float daggerTimer;
    private float bombTimer;

    private bool isAttacking;
    private bool isHurt;
    private bool isThrowingBomb;
    private bool daggerDamageDealt;

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

        daggerTimer -= Time.deltaTime;
        bombTimer -= Time.deltaTime;

        if (player == null || animator == null)
        {
            SetIdle();
            return;
        }

        if (isHurt || isAttacking || isThrowingBomb)
            return;

        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        if (distance > detectionDistance)
        {
            SetIdle();
            return;
        }

        // Гоблин не бежит за игроком на другой высоте,
        // но при готовности может атаковать его бомбой.
        if (!IsPlayerOnReachableHeight(player))
        {
            StopHorizontalMovement();

            if (bombTimer <= 0f &&
                bombPrefab != null &&
                bombSpawnPoint != null)
            {
                FacePlayer();
                ThrowBomb();
            }
            else
            {
                SetIdle();
            }

            return;
        }

        FacePlayer();

        if (distance <= daggerDistance)
        {
            StopHorizontalMovement();
            AttackWithDagger();
        }
        else if (bombTimer <= 0f &&
                 bombPrefab != null &&
                 bombSpawnPoint != null)
        {
            StopHorizontalMovement();
            ThrowBomb();
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

        if (isHurt || isAttacking || isThrowingBomb || player == null)
        {
            StopHorizontalMovement();
            return;
        }

        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        if (!IsPlayerOnReachableHeight(player) ||
            distance > detectionDistance ||
            distance <= daggerDistance ||
            (bombTimer <= 0f &&
             bombPrefab != null &&
             bombSpawnPoint != null))
        {
            StopHorizontalMovement();
            return;
        }

        // Не двигаемся, если под ногами нет земли,
        // впереди край платформы или стена.
        if (!IsGrounded() || !IsGroundAhead() || IsWallAhead())
        {
            StopHorizontalMovement();
            return;
        }

        rb.linearVelocity = new Vector2(
            facingDirection * moveSpeed,
            rb.linearVelocity.y
        );
    }

    private bool IsGrounded()
    {
        if (groundCheck != null)
        {
            RaycastHit2D hit = Physics2D.Raycast(
                groundCheck.position,
                Vector2.down,
                groundCheckDistance,
                groundLayer
            );

            return hit.collider != null;
        }

        if (bodyCollider == null)
            return false;

        Bounds bounds = bodyCollider.bounds;

        Vector2 origin = new Vector2(
            bounds.center.x,
            bounds.min.y + 0.02f
        );

        RaycastHit2D groundHit = Physics2D.Raycast(
            origin,
            Vector2.down,
            groundCheckDistance + 0.05f,
            groundLayer
        );

        return groundHit.collider != null;
    }

    private bool IsGroundAhead()
    {
        if (groundCheck == null)
            return true;

        // Проверяем пол немного впереди ног по направлению движения.
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

    private void MoveToPlayer()
    {
        animator.SetBool("Run", true);
        animator.SetBool("Idle", false);
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

    private void FacePlayer()
    {
        float directionX =
            player.position.x - transform.position.x;

        if (directionX > 0.05f)
            facingDirection = 1;
        else if (directionX < -0.05f)
            facingDirection = -1;


        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * facingDirection;
        transform.localScale = scale;
    }

    private void SetIdle()
    {
        StopHorizontalMovement();

        if (animator == null)
            return;

        animator.SetBool("Run", false);
        animator.SetBool("Idle", true);
    }

    private void AttackWithDagger()
    {
        if (daggerTimer > 0f)
            return;

        daggerTimer = daggerCooldown;
        isAttacking = true;
        daggerDamageDealt = false;

        animator.SetBool("Run", false);
        animator.SetBool("Idle", false);

        animator.SetTrigger("Attack");
    }

    private void ThrowBomb()
    {
        bombTimer = bombCooldown;
        isThrowingBomb = true;

        animator.SetBool("Run", false);
        animator.SetBool("Idle", false);
        animator.SetTrigger("Attack2");
    }

    // Вызывается Animation Event в кадре удара кинжалом.
    public void DealDaggerDamage()
    {
        if (isDead || !isAttacking || daggerDamageDealt || player == null)
            return;

        if (!IsPlayerOnReachableHeight(player))
            return;

        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        if (distance > daggerDistance + 0.3f)
            return;

        Player playerComponent =
            player.GetComponentInParent<Player>();

        if (playerComponent != null)
        {
            playerComponent.TakeDamage(daggerDamage);
            daggerDamageDealt = true;
        }
    }

    // Вызывается Animation Event в момент выпуска бомбы.
    public void SpawnBomb()
    {
        if (isDead || !isThrowingBomb ||
            bombPrefab == null || bombSpawnPoint == null)
            return;

        GameObject bomb = Instantiate(
            bombPrefab,
            bombSpawnPoint.position,
            Quaternion.identity
        );

        GoblinBomb bombScript = bomb.GetComponent<GoblinBomb>();

        if (bombScript == null)
        {
            Debug.LogError("На префабе бомбы отсутствует GoblinBomb!");
            Destroy(bomb);
            return;
        }

        bombScript.damage = bombDamage;
        bombScript.throwForce = bombThrowForce;
        bombScript.SetTarget(player, this);
    }

    // Animation Event в конце анимации удара.
    public void FinishDaggerAttack()
    {
        isAttacking = false;
    }

    // Animation Event в конце анимации броска.
    public void FinishBombAttack()
    {
        isThrowingBomb = false;
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
        isThrowingBomb = false;

        StopHorizontalMovement();

        animator.SetBool("Run", false);
        animator.SetBool("Idle", false);
        animator.ResetTrigger("Attack");
        animator.ResetTrigger("Attack2");
        animator.SetTrigger("TakeHit");

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
        isThrowingBomb = false;
        isHurt = false;

        CancelInvoke(nameof(EndHurt));

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        if (animator != null)
        {
            animator.SetBool("Run", false);
            animator.SetBool("Idle", false);
            animator.ResetTrigger("Attack");
            animator.ResetTrigger("Attack2");
            animator.SetTrigger("Death");
        }

        Destroy(gameObject, 2f);
    }
}

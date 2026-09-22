using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
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
    private bool isGrounded;
    private bool facingRight = true;
    private float moveInput;
    private float lastAttackTime = -Mathf.Infinity;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>(); // может быть null, если Animator ещё не добавлен — это ок
    }

    void Update()
    {
        // Считываем ввод в Update (это привязано к кадрам, а не к физике)
        moveInput = Input.GetAxisRaw("Horizontal"); // A/D или стрелки, -1..0..1

        // Проверяем, стоит ли персонаж на земле
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Прыжок по нажатию пробела, только если стоим на земле
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // Разворот спрайта в сторону движения
        if (moveInput > 0f && !facingRight) Flip();
        else if (moveInput < 0f && facingRight) Flip();

        // Передаём скорость в Animator для переключения Idle/Walk
        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(moveInput));
        }

        // Атака по левой кнопке мыши, с учётом кулдауна
        if (Input.GetButtonDown("Fire1") && Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            Attack();
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        sr.flipX = !facingRight;

        // Точка атаки — дочерний объект, flipX на неё не действует, двигаем вручную
        if (attackPoint != null)
        {
            Vector3 pos = attackPoint.localPosition;
            pos.x = -pos.x;
            attackPoint.localPosition = pos;
        }
    }

    void Attack()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        if (attackPoint == null) return;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);
        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log("Ударили: " + enemy.name);
            // Позже здесь будет: enemy.GetComponent<EnemyHealth>().TakeDamage(damage);
        }
    }

    void FixedUpdate()
    {
        // Физику двигаем в FixedUpdate — это правильно для Rigidbody2D
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    // Визуализация зон в редакторе (удобно для отладки)
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (attackPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}
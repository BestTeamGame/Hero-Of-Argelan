
using System.Collections.Generic;
using Project.Scripts;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class GoblinBomb : MonoBehaviour
{
    [Header("Урон")]
    public int damage = 25;
    public float explosionRadius = 1.5f;

    [Header("Полёт")]
    public float throwForce = 7f;
    public float arcHeight = 3f;
    public float gravityScale = 1.2f;

    [Tooltip("Случайное отклонение точки приземления по горизонтали")]
    public float horizontalAccuracy = 1.2f;

    [Tooltip("Случайное отклонение точки приземления по высоте")]
    public float verticalAccuracy = 0.4f;

    [Header("Взрыв")]
    public float explosionDuration = 0.6f;
    public Animator animator;

    private Rigidbody2D rb;
    private GoblinEnemy owner;

    private bool exploded;
    private bool damageApplied;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void SetTarget(Transform target, GoblinEnemy goblin)
    {
        owner = goblin;

        if (target == null || rb == null)
            return;

        exploded = false;
        damageApplied = false;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.simulated = true;
        rb.gravityScale = gravityScale;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        rb.linearVelocity = Vector2.zero;

        IgnoreOwnerCollisions();
        IgnorePlayerCollisions(target);

        // Запоминаем положение игрока только в момент броска.
        Vector2 start = transform.position;
        Vector2 end = target.position;

        // Добавляем погрешность, чтобы бомбы не летели
        // каждый раз точно в одну и ту же точку.
        end.x += Random.Range(
            -horizontalAccuracy,
            horizontalAccuracy
        );

        end.y += Random.Range(
            -verticalAccuracy,
            verticalAccuracy
        );

        // Вычисляем траекторию один раз.
        // После броска движение игрока уже не влияет на бомбу.
        float gravity = Mathf.Abs(
            Physics2D.gravity.y * rb.gravityScale
        );

        gravity = Mathf.Max(gravity, 0.1f);

        float highestPoint = Mathf.Max(start.y, end.y) + arcHeight;

        float heightUp = Mathf.Max(highestPoint - start.y, 0.1f);
        float heightDown = Mathf.Max(highestPoint - end.y, 0.1f);

        float velocityY = Mathf.Sqrt(2f * gravity * heightUp);
        float timeUp = velocityY / gravity;
        float timeDown = Mathf.Sqrt(2f * heightDown / gravity);
        float flightTime = timeUp + timeDown;

        // Рассчитываем горизонтальную скорость до выбранной точки.
        float velocityX = (end.x - start.x) / flightTime;

        // Дополнительная случайная погрешность скорости по горизонтали.
        velocityX += Random.Range(-0.6f, 0.6f);

        rb.linearVelocity = new Vector2(velocityX, velocityY);

        // Разворачиваем спрайт в сторону броска.
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * Mathf.Sign(velocityX);
        transform.localScale = scale;
    }

    private void IgnoreOwnerCollisions()
    {
        if (owner == null)
            return;

        Collider2D[] bombColliders =
            GetComponentsInChildren<Collider2D>();

        Collider2D[] ownerColliders =
            owner.GetComponentsInChildren<Collider2D>();

        foreach (Collider2D bombCol in bombColliders)
        {
            foreach (Collider2D ownerCol in ownerColliders)
            {
                if (bombCol != null && ownerCol != null)
                {
                    Physics2D.IgnoreCollision(
                        bombCol, ownerCol, true
                    );
                }
            }
        }
    }

    private void IgnorePlayerCollisions(Transform target)
    {
        if (target == null)
            return;

        Player playerComponent = target.GetComponentInParent<Player>();

        if (playerComponent == null)
            playerComponent = target.GetComponentInChildren<Player>();

        if (playerComponent == null)
            return;

        Collider2D[] bombColliders =
            GetComponentsInChildren<Collider2D>();

        Collider2D[] playerColliders =
            playerComponent.GetComponentsInChildren<Collider2D>();

        foreach (Collider2D bombCol in bombColliders)
        {
            foreach (Collider2D playerCol in playerColliders)
            {
                if (bombCol != null && playerCol != null)
                {
                    Physics2D.IgnoreCollision(
                        bombCol, playerCol, true
                    );
                }
            }
        }
    }

    private bool IsPlayer(Collider2D other)
    {
        if (other == null)
            return false;

        if (other.CompareTag("Player"))
            return true;

        return other.GetComponentInParent<Player>() != null;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleCollision(other);
    }

    private void HandleCollision(Collider2D other)
    {
        if (exploded || other == null)
            return;

        // При касании игрока не взрываем бомбу.
        if (IsPlayer(other))
            return;

        // Игнорируем столкновения с гоблином.
        if (owner != null &&
            other.GetComponentInParent<GoblinEnemy>() == owner)
            return;

        Explode();
    }

    private void Explode()
    {
        if (exploded)
            return;

        exploded = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        Collider2D[] colliders =
            GetComponentsInChildren<Collider2D>();

        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        if (animator != null)
            animator.SetTrigger("Explode");

        // Уничтожаем бомбу после завершения анимации.
        Destroy(gameObject, explosionDuration);
    }

    public void DealExplosionDamage()
    {
        if (!exploded || damageApplied)
            return;

        damageApplied = true;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            explosionRadius
        );

        HashSet<Player> damagedPlayers = new HashSet<Player>();

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            Player playerComponent =
                hit.GetComponentInParent<Player>();

            if (playerComponent == null)
                continue;

            if (damagedPlayers.Add(playerComponent))
                playerComponent.TakeDamage(damage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}

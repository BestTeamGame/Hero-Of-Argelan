using Project.Scripts;
using UnityEngine;
using UnityEngine.UI;

public class ReaperBoss : Enemy
{
    [Header("Movement")]
    public float moveSpeed = 1.5f;
    public float detectionDistance = 10f;
    public float attackDistance = 2f;

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

    private float attackTimer;
    private bool isAttacking;
    private bool isHurt;

    protected override void Start()
    {
        base.Start();

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    private void Update()
    {
        if (isDead)
            return;

        if (player == null)
        {
            SetWalkingAnimation(false);
            return;
        }

        attackTimer -= Time.deltaTime;

        if (isHurt || isAttacking)
            return;

        UpdateHealthBar();

        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        if (distance <= attackDistance)
        {
            Attack();
        }
        else if (distance <= detectionDistance)
        {
            MoveToPlayer();
        }
        else
        {
            SetWalkingAnimation(false);
        }
    }

    private void MoveToPlayer()
    {
        Vector3 direction =
            player.position - transform.position;

        direction.y = 0f;

        if (direction.x > 0f)
        {
            transform.localScale = new Vector3(
                Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z
            );
        }
        else if (direction.x < 0f)
        {
            transform.localScale = new Vector3(
                -Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z
            );
        }

        transform.position +=
            direction.normalized *
            moveSpeed *
            Time.deltaTime;

        SetWalkingAnimation(true);
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

        SetWalkingAnimation(false);

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
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

        CancelInvoke(nameof(EndHurt));

        SetWalkingAnimation(false);

        if (animator != null)
        {
            animator.SetTrigger("Hurt");
        }

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

        SetWalkingAnimation(false);

        if (healthSlider != null)
        {
            healthSlider.value = 0;
        }

        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        Destroy(gameObject, destroyDelay);
    }

    public void FinishAttack()
    {
        isAttacking = false;
    }

    public void DealDamage()
    {
        if (isDead || player == null)
            return;

        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        if (distance > attackDistance + 0.3f)
            return;

        Player playerComponent =
            player.GetComponent<Player>();

        if (playerComponent == null)
        {
            playerComponent =
                player.GetComponentInParent<Player>();
        }

        if (playerComponent != null)
        {
            Debug.Log(
                "Reaper наносит игроку урон: " +
                attackDamage
            );

            playerComponent.TakeDamage(attackDamage);
        }
        else
        {
            Debug.LogWarning(
                "Player не найден!"
            );
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
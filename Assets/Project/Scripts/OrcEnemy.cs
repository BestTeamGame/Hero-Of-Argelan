using Project.Scripts;
using UnityEngine;

public class OrcEnemy : Enemy
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float detectionDistance = 7f;
    public float attackDistance = 1.5f;

    [Header("Attack")]
    public int attackDamage = 20;
    public float attackCooldown = 1.5f;

    [Header("References")]
    public Transform player;
    public Animator animator;

    private float attackTimer;
    private bool isAttacking;
    private bool isHurt;

    protected override void Start()
    {
        base.Start();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                player = playerObject.transform;
        }
    }

    private void Update()
    {
        if (isDead)
            return;

        if (player == null)
        {
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
            SetIdleAnimation();
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

        animator.SetBool("IsWalking", true);
        animator.SetBool("IsIdle", false);
    }

    private void SetIdleAnimation()
    {
        animator.SetBool("IsWalking", false);
        animator.SetBool("IsIdle", true);
    }

    private void Attack()
    {
        if (attackTimer > 0f)
            return;

        attackTimer = attackCooldown;
        isAttacking = true;

        animator.SetBool("IsWalking", false);
        animator.SetBool("IsIdle", false);

        int attackNumber = Random.Range(0, 2);

        if (attackNumber == 0)
        {
            animator.SetTrigger("Attack1");
        }
        else
        {
            animator.SetTrigger("Attack2");
        }
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

        animator.SetBool("IsWalking", false);
        animator.SetBool("IsIdle", false);

        animator.SetTrigger("Hurt");

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

        animator.SetBool("IsWalking", false);
        animator.SetBool("IsIdle", false);

        animator.SetTrigger("Death");

        Destroy(gameObject, 2f);
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
                "Орк наносит игроку урон: " +
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
}

using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    protected int currentHealth;

    protected bool isDead;

    [Header("Platform Safety")]
    [Tooltip("Максимальная разница высоты для преследования игрока.")]
    public float maxVerticalChaseDistance = 1.5f;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
    }

    // Проверяем, находится ли игрок примерно на той же высоте.
    // Это предотвращает преследование через недоступные платформы.
    protected bool IsPlayerOnReachableHeight(Transform target)
    {
        if (target == null)
            return false;

        float verticalDistance =
            Mathf.Abs(target.position.y - transform.position.y);

        return verticalDistance <= maxVerticalChaseDistance;
    }

    public virtual void TakeDamage(int damage)
    {
        if (isDead || damage <= 0)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        Debug.Log(gameObject.name + " получил урон: " + damage);

        if (currentHealth <= 0)
            Die();
    }

    protected virtual void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log(gameObject.name + " умер");

        Destroy(gameObject, 2f);
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }
}

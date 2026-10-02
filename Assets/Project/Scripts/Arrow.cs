using UnityEngine;

using UnityEngine;

namespace Project.Scripts
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Arrow : MonoBehaviour
    {
        private Rigidbody2D rb;

        private float direction;
        private float speed;
        private int damage;
        private LayerMask enemyLayer;

        private bool initialized;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public void Initialize(
    float direction,
    float speed,
    int damage,
    LayerMask enemyLayer)
        {
            this.direction = direction;
            this.speed = speed;
            this.damage = damage;
            this.enemyLayer = enemyLayer;

            initialized = true;

            rb.linearVelocity =
                new Vector2(direction * speed, 0f);


            if (direction > 0)
            {
                transform.rotation =
                    Quaternion.Euler(0f, 0f, 0f);
            }
            else
            {
                transform.rotation =
                    Quaternion.Euler(0f, 0f, 180f);
            }
        }

        private void Update()
        {
            if (!initialized)
                return;


        }

        private void OnTriggerEnter2D(
            Collider2D collision)
        {
            if (!initialized)
                return;


            if ((enemyLayer.value &
                 (1 << collision.gameObject.layer)) == 0)
            {
                return;
            }

            Enemy enemy =
                collision.GetComponent<Enemy>();

            if (enemy == null)
            {
                enemy =
                    collision.GetComponentInParent<Enemy>();
            }

            if (enemy != null)
            {
                Debug.Log(
                    "Стрела попала в: " +
                    enemy.name
                );

                enemy.TakeDamage(damage);

                Destroy(gameObject);
            }
        }

        private void OnBecameInvisible()
        {
            Destroy(gameObject);
        }
    }
}
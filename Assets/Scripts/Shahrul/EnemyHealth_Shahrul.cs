using UnityEngine;

public class EnemyHealth_Shahrul : MonoBehaviour
{
    [Header("Enemy Animation")]
    public Animator enemyAnimator;

    [Header("Effects")]
    public GameObject deathParticle;

    [Header("Stats")]
    public int maxHealth = 50;
    public bool isDead = false;

    private int currentHealth;
    private Rigidbody2D rb;
    private Collider2D col;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        if (enemyAnimator == null)
        {
            enemyAnimator = GetComponent<Animator>();
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"Enemy (Shahrul) took {damage} damage.");

        if (enemyAnimator != null)
        {
            enemyAnimator.SetTrigger("Hurt");
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("Enemy (Shahrul) Died!");

        if (deathParticle != null)
        {
            Instantiate(deathParticle, transform.position, Quaternion.identity);
        }

        if (enemyAnimator != null)
        {
            enemyAnimator.SetBool("IsDead", true);
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        if (col != null)
        {
            col.enabled = false;
        }

        // REFERS TO SHAHRUL CONTROLLER
        EnemyController_Shahrul controller = GetComponent<EnemyController_Shahrul>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        Destroy(gameObject, 2f);
    }
}
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth_Ikmal : MonoBehaviour
{
    [Header("Enemy Animation")]
    public Animator enemyAnimator;

    [Header("Effects")]
    public GameObject deathParticle;

    [Header("Health UI")]
    public Slider healthSlider; // DRAG SLIDER HERE

    [Header("Stats")]
    public int maxHealth = 50;
    public bool isDead = false;

    private int currentHealth;
    private Rigidbody2D rb;
    private Collider2D col;

    void Start()
    {
        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        if (enemyAnimator == null) enemyAnimator = GetComponent<Animator>();
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (healthSlider != null) healthSlider.value = currentHealth;

        if (enemyAnimator != null) enemyAnimator.SetTrigger("Hurt");

        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        // FIX: Notify HUD that this enemy died with Safety Check
        if (HUDManager.instance != null)
        {
            HUDManager.instance.RegisterEnemyDeath();
            Debug.Log("Enemy Died: Notification sent to HUDManager");
        }
        else
        {
            Debug.LogError("HUDManager instance is NULL! Ensure HUDManager is in the scene.");
        }

        if (deathParticle != null) Instantiate(deathParticle, transform.position, Quaternion.identity);
        if (enemyAnimator != null) enemyAnimator.SetBool("IsDead", true);
        if (healthSlider != null) healthSlider.gameObject.SetActive(false);

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        if (col != null) col.enabled = false;

        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            if (script != this) script.enabled = false;
        }

        // FIX: Destroy the enemy object after 2 seconds so it disappears
        Destroy(gameObject, 2f);
    }
}
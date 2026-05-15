using UnityEngine;

public class GuardHealth_Irfan : MonoBehaviour
{
    [Header("Guard Animation")]
    public Animator guardAnimator;

    [Header("Effects")]
    public GameObject deathParticle;

    [Header("Stats")]
    public int maxHealth = 80;
    public int currentHealth;
    public bool isDead = false;

    // The reference to the missing Guard_Level1_Special_Irfan has been removed.
    // private Guard_Level1_Special_Irfan specialController;

    void Start()
    {
        currentHealth = maxHealth;
        // The attempt to find the missing controller has been removed.
        // specialController = GetComponent<Guard_Level1_Special_Irfan>();

        if (guardAnimator == null)
        {
            guardAnimator = GetComponent<Animator>();
        }
    }

    public void TakeDamage(int damage)
    {
        // Debug Log to confirm hit
        Debug.Log($"Guard {gameObject.name} taking damage: {damage}. Current HP: {currentHealth}");

        if (isDead) return;

        // The entire Level 1 Special Logic block has been removed, 
        // as the Guard_Level1_Special_Irfan script is not available.

        currentHealth -= damage;

        if (guardAnimator != null)
        {
            guardAnimator.SetTrigger("Hurt");
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        Debug.Log("Guard (Irfan) died!");

        if (deathParticle != null)
        {
            Instantiate(deathParticle, transform.position, Quaternion.identity);
        }

        if (guardAnimator != null)
        {
            guardAnimator.SetBool("IsDead", true);
        }

        GetComponent<Collider2D>().enabled = false;

        // Stop physics
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // Disable ANY controller script that might be attached 
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            // Removed the check for 'Guard_Level1_Special_Irfan' as it caused the error.
            if (script is Guard_Chase_Irfan || script is Guard_Patrol_Irfan)
            {
                script.enabled = false;
            }
        }

        Destroy(gameObject, 2f);
    }
}
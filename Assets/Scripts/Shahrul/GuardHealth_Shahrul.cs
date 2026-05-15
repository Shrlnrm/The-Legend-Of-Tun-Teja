using UnityEngine;
using UnityEngine.UI; // Added for Slider

public class GuardHealth_Shahrul : MonoBehaviour
{
    [Header("Guard Animation")]
    public Animator guardAnimator;

    [Header("Effects")]
    public GameObject deathParticle;

    [Header("Health UI")]
    public Slider healthSlider;

    [Header("Stats")]
    public int maxHealth = 80;
    public int currentHealth;
    public bool isDead = false;

    private Guard_Level1_Special_Shahrul specialController;

    void Start()
    {
        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        specialController = GetComponent<Guard_Level1_Special_Shahrul>();
        if (guardAnimator == null) guardAnimator = GetComponent<Animator>();
    }

    public void TakeDamage(int damage)
    {
        Debug.Log($"Guard {gameObject.name} taking damage: {damage}. Current HP: {currentHealth}");

        if (isDead) return;

        // Special Level 1 Logic (Surrender at 5HP)
        if (specialController != null)
        {
            if (currentHealth - damage <= 5)
            {
                currentHealth = 5;
                UpdateHealthUI(); // Update UI

                if (!specialController.hasSurrendered)
                {
                    specialController.TriggerSurrender();

                    // NEW: Hide health bar immediately upon surrender
                    if (healthSlider != null) healthSlider.gameObject.SetActive(false);
                }
                return;
            }
        }

        currentHealth -= damage;
        UpdateHealthUI(); // Update UI

        if (guardAnimator != null) guardAnimator.SetTrigger("Hurt");

        if (currentHealth <= 0) Die();
    }

    void UpdateHealthUI()
    {
        if (healthSlider != null) healthSlider.value = currentHealth;
    }

    void Die()
    {
        isDead = true;
        if (deathParticle != null) Instantiate(deathParticle, transform.position, Quaternion.identity);
        if (guardAnimator != null) guardAnimator.SetBool("IsDead", true);

        GetComponent<Collider2D>().enabled = false;

        if (healthSlider != null) healthSlider.gameObject.SetActive(false); // Hide bar on death

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            if (script is Guard_Level1_Special_Shahrul || script is Guard_Chase_Friendly_Shahrul || script.GetType().Name.Contains("Controller"))
                script.enabled = false;
        }
    }
}
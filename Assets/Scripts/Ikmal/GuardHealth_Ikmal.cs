using UnityEngine;
using UnityEngine.UI;

public class GuardHealth_Ikmal : MonoBehaviour
{
    [Header("Guard Animation")]
    public Animator guardAnimator;

    [Header("Effects")]
    public GameObject deathParticle;

    [Header("Health UI")]
    public Slider healthSlider; // DRAG SLIDER HERE

    [Header("Stats")]
    public int maxHealth = 80;
    public int currentHealth;
    public bool isDead = false;

    private Guard_Chase_Friendly_Ikmal controller; // Updated reference to friendly controller

    void Start()
    {
        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        controller = GetComponent<Guard_Chase_Friendly_Ikmal>();
        if (guardAnimator == null) guardAnimator = GetComponent<Animator>();
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (healthSlider != null) healthSlider.value = currentHealth;

        if (guardAnimator != null) guardAnimator.SetTrigger("Hurt");
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        if (isDead) return; // Prevent double counting
        isDead = true;

        // FIX: Notify HUD that this guard died
        if (HUDManager.instance != null)
        {
            HUDManager.instance.RegisterEnemyDeath();
            Debug.Log("Guard Died: Notification sent to HUDManager");
        }
        else
        {
            Debug.LogError("HUDManager instance is NULL! Ensure HUDManager is in the scene.");
        }

        if (deathParticle != null) Instantiate(deathParticle, transform.position, Quaternion.identity);
        if (guardAnimator != null) guardAnimator.SetBool("IsDead", true);
        if (healthSlider != null) healthSlider.gameObject.SetActive(false);

        GetComponent<Collider2D>().enabled = false;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) { rb.linearVelocity = Vector2.zero; rb.bodyType = RigidbodyType2D.Kinematic; }

        if (controller != null) controller.enabled = false;

        // FIX: Destroy the guard object after 2 seconds so it disappears
        Destroy(gameObject, 2f);
    }
}
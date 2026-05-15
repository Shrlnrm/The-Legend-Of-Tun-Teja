using UnityEngine;
using UnityEngine.UI;

public class TunTejaHealth_Ikmal : MonoBehaviour
{
    [Header("Animation")]
    public Animator tejaAnimator;
    public GameObject deathParticle;

    [Header("Health UI")]
    public Slider healthSlider; // DRAG SLIDER HERE

    [Header("Stats")]
    public int maxHealth = 100;
    public int currentHealth;
    public bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (tejaAnimator == null) tejaAnimator = GetComponent<Animator>();
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        // Debug.Log("Tun Teja HP: " + currentHealth);

        if (healthSlider != null) healthSlider.value = currentHealth;

        if (tejaAnimator != null) tejaAnimator.SetTrigger("Hurt");

        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (deathParticle != null) Instantiate(deathParticle, transform.position, Quaternion.identity);
        if (healthSlider != null) healthSlider.gameObject.SetActive(false);

        if (tejaAnimator != null)
        {
            tejaAnimator.SetBool("IsDead", true);
            tejaAnimator.SetBool("IsRunning", false);
        }

        // Disable Physics
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        GetComponent<Collider2D>().enabled = false;

        if (GetComponent<TunTejaController_Ikmal>())
            GetComponent<TunTejaController_Ikmal>().enabled = false;

        // --- NEW LOGIC: Trigger Player Respawn ---
        // Find the player and tell them to start the checkpoint respawn sequence
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerHealth_Ikmal playerHealth = player.GetComponent<PlayerHealth_Ikmal>();
            if (playerHealth != null)
            {
                // This will cause the screen to reset after the delay, bringing both back to life
                playerHealth.TriggerRespawnFromTejaDeath();
            }
        }
    }
}
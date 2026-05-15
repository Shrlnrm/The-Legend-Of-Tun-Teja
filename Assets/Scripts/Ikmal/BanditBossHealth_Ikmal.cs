using UnityEngine;
using UnityEngine.UI;

public class BanditBossHealth_Ikmal : MonoBehaviour
{
    [Header("Boss Animation")]
    public Animator bossAnimator;

    [Header("Effects")]
    public GameObject deathParticle;

    [Header("Health UI")]
    public Slider healthSlider; // DRAG SLIDER HERE

    [Header("Stats")]
    public int maxHealth = 200; // Boss has 200 HP
    public int currentHealth;
    public bool isDead = false;

    private BanditBossController_Ikmal controller;

    void Start()
    {
        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        controller = GetComponent<BanditBossController_Ikmal>();
        if (bossAnimator == null) bossAnimator = GetComponent<Animator>();
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log("Boss HP: " + currentHealth);

        if (healthSlider != null) healthSlider.value = currentHealth;

        if (bossAnimator != null)
        {
            bossAnimator.Play("Boss_Hurt", -1, 0f);
        }

        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (deathParticle != null) Instantiate(deathParticle, transform.position, Quaternion.identity);
        if (bossAnimator != null) bossAnimator.SetBool("IsDead", true);
        if (healthSlider != null) healthSlider.gameObject.SetActive(false);

        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;

        if (controller != null) controller.enabled = false;
    }
}
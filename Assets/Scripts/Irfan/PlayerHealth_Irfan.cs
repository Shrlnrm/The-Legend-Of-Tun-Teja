using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth_Irfan : MonoBehaviour
{
    public Animator playerAnimator;
    public GameObject deathParticle;
    public float respawnDelay = 2f;
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Regeneration")]
    public int regenAmount = 20;
    public float regenInterval = 2f;

    private bool isDead = false;
    private float lastDamageTime;
    private float regenTimer;
    private PlayerCombat_Irfan combatScript; // Reference for special move (Block/Dodge)

    void Start()
    {
        currentHealth = maxHealth;
        if (playerAnimator == null) playerAnimator = GetComponent<Animator>();
        combatScript = GetComponent<PlayerCombat_Irfan>();
        UpdateHUD();
    }

    void Update()
    {
        if (isDead) return;

        // Health Regen Logic
        if (Time.time > lastDamageTime + 2f && currentHealth < maxHealth)
        {
            regenTimer += Time.deltaTime;
            if (regenTimer >= regenInterval)
            {
                currentHealth += regenAmount;
                if (currentHealth > maxHealth) currentHealth = maxHealth;
                UpdateHUD();
                regenTimer = 0f;
            }
        }
        else
        {
            regenTimer = 0f;
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        // SPECIAL MOVE: BLOCK/DODGE CHECK
        if (combatScript != null && (combatScript.isBlocking || combatScript.isDodging))
        {
            Debug.Log("Irfan defended/dodged the attack!");
            return;
        }

        currentHealth -= damage;
        lastDamageTime = Time.time;
        UpdateHUD();

        Debug.Log($"Player (Irfan) HP: {currentHealth}");

        // Add Hurt animation trigger
        if (playerAnimator != null) playerAnimator.SetTrigger("Hurt");

        if (currentHealth <= 0) Die();
    }

    void UpdateHUD()
    {
        if (HUDManager.instance != null)
            HUDManager.instance.UpdateHealthBar(currentHealth, maxHealth);
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log("Player (Irfan) died!");

        PlayerController_Irfan controller = GetComponent<PlayerController_Irfan>();
        if (controller != null) controller.enabled = false;

        if (combatScript != null) combatScript.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) { rb.linearVelocity = Vector2.zero; rb.gravityScale = 0; rb.bodyType = RigidbodyType2D.Kinematic; }

        if (deathParticle != null) Instantiate(deathParticle, transform.position, Quaternion.identity);

        if (playerAnimator != null)
        {
            playerAnimator.SetBool("IsRunning", false);
            playerAnimator.SetBool("IsJumping", false);
            playerAnimator.SetBool("IsDead", true);
        }

        // NEW: Call UI instead of reloading scene immediately
        if (LevelManager.instance != null)
        {
            StartCoroutine(ShowDeathUI());
        }
        else
        {
            StartCoroutine(RespawnAfterDelay());
        }
    }

    IEnumerator ShowDeathUI()
    {
        yield return new WaitForSeconds(respawnDelay); // Wait for death anim
        LevelManager.instance.ShowDeadPanel();
    }

    IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
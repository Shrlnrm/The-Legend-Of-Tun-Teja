using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI; // Needed for Slider (if accessing Teja's UI)

public class PlayerHealth_Ikmal : MonoBehaviour
{
    public Animator playerAnimator;
    public GameObject deathParticle;
    public float respawnDelay = 2f;
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Regeneration")]
    public float regenAmount = 5f;      // How much HP to heal
    public float regenInterval = 1f;    // How often to heal (every 1 second)
    public float regenDelay = 3f;       // How long to wait after taking damage before healing starts

    [Header("Checkpoint Settings")]
    public bool enableCheckpointSystem = false; // Tick for ANY level with checkpoints
    public bool respawnTunTeja = false;         // Tick ONLY for levels where Tun Teja follows you

    private bool isDead = false;
    private PlayerCombat_Ikmal combatScript;
    private PlayerController_Ikmal movementScript;
    private Rigidbody2D rb;
    private Collider2D col;

    // CHECKPOINT VARIABLES
    private Vector3 respawnPoint;
    private float defaultGravity;

    // REGEN VARIABLES
    private float lastDamageTime;
    private float regenTimer;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        movementScript = GetComponent<PlayerController_Ikmal>();
        combatScript = GetComponent<PlayerCombat_Ikmal>();

        if (!playerAnimator) playerAnimator = GetComponent<Animator>();

        // Store default gravity to reset it later
        if (rb != null) defaultGravity = rb.gravityScale;

        // Set initial respawn point to where the player starts the game
        respawnPoint = transform.position;

        // Initialize HUD
        if (HUDManager.instance != null)
        {
            HUDManager.instance.UpdateHealthBar(currentHealth, maxHealth);
        }
    }

    void Update()
    {
        if (isDead) return;

        // --- REGENERATION LOGIC ---
        // Check if enough time has passed since the last damage
        if (Time.time > lastDamageTime + regenDelay && currentHealth < maxHealth)
        {
            regenTimer += Time.deltaTime;
            if (regenTimer >= regenInterval)
            {
                // Heal
                currentHealth += (int)regenAmount;
                if (currentHealth > maxHealth) currentHealth = maxHealth;

                // Update HUD
                if (HUDManager.instance != null)
                {
                    HUDManager.instance.UpdateHealthBar(currentHealth, maxHealth);
                }

                regenTimer = 0f; // Reset interval timer
            }
        }
    }

    // Called by Checkpoint_Ikmal.cs
    public void UpdateCheckpoint(Vector3 newPosition)
    {
        respawnPoint = newPosition;
        Debug.Log("Checkpoint Updated to: " + newPosition);
    }

    // Called by TunTejaHealth_Ikmal.cs when she dies
    public void TriggerRespawnFromTejaDeath()
    {
        if (isDead) return;

        Debug.Log("Tun Teja Died! Resetting to Checkpoint...");

        // 1. Disable Controls (Stop player input)
        if (movementScript) movementScript.enabled = false;
        if (combatScript) combatScript.enabled = false;

        // 2. Stop Physics (Freeze player so they don't keep moving/falling)
        if (rb)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // 3. Force Idle Animation
        if (playerAnimator)
        {
            playerAnimator.Rebind(); // Force reset to default state
            playerAnimator.Update(0f);
            playerAnimator.SetBool("IsRunning", false);
            playerAnimator.SetBool("IsJumping", false);
            playerAnimator.SetBool("IsBlocking", false);
        }

        // 4. Start the Respawn/Reset Timer directly (Skip Player Death logic)
        StartCoroutine(RespawnRoutine());
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        // 1. CHECK IF BLOCKING
        if (combatScript != null && combatScript.isBlocking) return;

        // 2. APPLY DAMAGE
        currentHealth -= damage;
        lastDamageTime = Time.time; // RESET REGEN TIMER

        // Update HUD
        if (HUDManager.instance != null)
        {
            HUDManager.instance.UpdateHealthBar(currentHealth, maxHealth);
        }

        // Play Hurt Animation
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("Hurt");
        }

        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log("Player (Ikmal) died!");

        // Disable controls
        if (movementScript) movementScript.enabled = false;
        if (combatScript) combatScript.enabled = false;

        // Stop physics movement
        if (rb)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        if (deathParticle) Instantiate(deathParticle, transform.position, Quaternion.identity);

        if (playerAnimator)
        {
            playerAnimator.SetBool("IsRunning", false);
            playerAnimator.SetBool("IsJumping", false);
            playerAnimator.SetBool("IsBlocking", false);
            playerAnimator.SetBool("IsDead", true);
        }

        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        if (enableCheckpointSystem)
        {
            // --- OPTION A: RESPAWN AT CHECKPOINT ---
            Debug.Log("Respawning at checkpoint...");

            // 1. RESET PLAYER POSITION & STATUS
            transform.position = respawnPoint;
            currentHealth = maxHealth;
            isDead = false;

            // Reset HUD
            if (HUDManager.instance != null)
            {
                HUDManager.instance.UpdateHealthBar(currentHealth, maxHealth);
            }

            // Reset Physics
            if (rb)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.gravityScale = defaultGravity;
                rb.linearVelocity = Vector2.zero;
            }

            // Reset Animation (FIXED WITH REBIND)
            if (playerAnimator)
            {
                playerAnimator.Rebind();
                playerAnimator.Update(0f);
                playerAnimator.SetBool("IsDead", false);
                playerAnimator.SetBool("IsRunning", false);
                playerAnimator.SetBool("IsJumping", false);
                playerAnimator.SetBool("IsBlocking", false);
                playerAnimator.Play("Idle", -1, 0f);
            }

            // Re-enable Controls
            if (movementScript) movementScript.enabled = true;
            if (combatScript) combatScript.enabled = true;

            // 2. CHECK IF WE NEED TO RESPAWN TUN TEJA TOO
            if (respawnTunTeja)
            {
                GameObject teja = GameObject.FindGameObjectWithTag("TunTeja");
                if (teja != null)
                {
                    // A. Move Teja to checkpoint
                    teja.transform.position = respawnPoint + new Vector3(-1f, 0f, 0f);

                    // B. RESET PHYSICS
                    Rigidbody2D tejaRb = teja.GetComponent<Rigidbody2D>();
                    if (tejaRb != null)
                    {
                        tejaRb.linearVelocity = Vector2.zero;
                        tejaRb.bodyType = RigidbodyType2D.Dynamic; // Turn physics back on
                    }

                    Collider2D tejaCol = teja.GetComponent<Collider2D>();
                    if (tejaCol != null) tejaCol.enabled = true; // Ensure she can collide again

                    // C. RESET CONTROLLER
                    TunTejaController_Ikmal tejaController = teja.GetComponent<TunTejaController_Ikmal>();
                    if (tejaController != null) tejaController.enabled = true; // Turn her brain back on

                    // D. RESET HEALTH SCRIPT & ANIMATION
                    TunTejaHealth_Ikmal tejaHealth = teja.GetComponent<TunTejaHealth_Ikmal>();
                    if (tejaHealth != null)
                    {
                        tejaHealth.currentHealth = tejaHealth.maxHealth;
                        tejaHealth.isDead = false;

                        // Re-enable Health Bar UI if it exists inside her script
                        if (tejaHealth.healthSlider != null)
                        {
                            tejaHealth.healthSlider.gameObject.SetActive(true);
                            tejaHealth.healthSlider.value = tejaHealth.currentHealth;
                        }

                        Animator tejaAnim = teja.GetComponent<Animator>();
                        if (tejaAnim != null)
                        {
                            tejaAnim.Rebind(); // Force reset Teja as well
                            tejaAnim.Update(0f);
                            tejaAnim.SetBool("IsDead", false);
                            tejaAnim.Play("Idle", -1, 0f);
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("You ticked 'Respawn Tun Teja' but no object with tag 'TunTeja' was found!");
                }
            }
        }
        else
        {
            // --- OPTION B: RELOAD SCENE (NO CHECKPOINT) ---
            Debug.Log("Reloading Level...");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
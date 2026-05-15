using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth_Shahrul : MonoBehaviour
{
    [Header("Player Animation")]
    public Animator playerAnimator;
    public GameObject deathParticle;
    public float respawnDelay = 2f;
    public int maxHealth = 100;
    public int currentHealth;
    public int regenAmount = 20;
    public float regenInterval = 2f;

    [Header("Checkpoint Settings")]
    public bool enableCheckpointSystem = false;

    private bool isDead = false;
    private float lastDamageTime;
    private float regenTimer;
    private PlayerCombat_Shahrul combatScript;
    private PlayerController_Shahrul controllerScript;
    private Rigidbody2D rb;

    private Vector3 respawnPoint;
    private float defaultGravity;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHUD();

        if (playerAnimator == null) playerAnimator = GetComponent<Animator>();
        combatScript = GetComponent<PlayerCombat_Shahrul>();
        controllerScript = GetComponent<PlayerController_Shahrul>();
        rb = GetComponent<Rigidbody2D>();

        if (rb != null) defaultGravity = rb.gravityScale;

        respawnPoint = transform.position;
    }

    void Update()
    {
        if (isDead) return;

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
        else regenTimer = 0f;
    }

    public void UpdateCheckpoint(Vector3 newPosition)
    {
        respawnPoint = newPosition;
        Debug.Log("Checkpoint Updated: " + newPosition);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        if (combatScript != null && (combatScript.isBlocking || combatScript.isDodging)) return;

        currentHealth -= damage;
        lastDamageTime = Time.time;
        UpdateHUD();

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
        Debug.Log("Player (Shahrul) died!");

        if (controllerScript != null) controllerScript.enabled = false;
        if (combatScript != null) combatScript.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        if (deathParticle != null) Instantiate(deathParticle, transform.position, Quaternion.identity);

        if (playerAnimator != null)
        {
            playerAnimator.SetBool("IsRunning", false);
            playerAnimator.SetBool("IsJumping", false);
            playerAnimator.SetBool("IsDead", true);
        }

        if (enableCheckpointSystem)
        {
            StartCoroutine(RespawnAtCheckpointRoutine());
        }
        else
        {
            if (LevelManager.instance != null)
                StartCoroutine(ShowDeathUI());
            else
                StartCoroutine(ReloadSceneRoutine());
        }
    }

    IEnumerator RespawnAtCheckpointRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        Debug.Log("Respawning at checkpoint...");

        transform.position = respawnPoint;
        currentHealth = maxHealth;
        isDead = false;
        UpdateHUD();

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = defaultGravity;
            rb.linearVelocity = Vector2.zero;
        }

        if (playerAnimator != null)
        {
            playerAnimator.Rebind();
            playerAnimator.Update(0f);
            playerAnimator.SetBool("IsDead", false);
            playerAnimator.Play("Idle", -1, 0f);
        }

        if (controllerScript != null) controllerScript.enabled = true;
        if (combatScript != null) combatScript.enabled = true;
    }

    IEnumerator ShowDeathUI()
    {
        yield return new WaitForSeconds(respawnDelay);
        LevelManager.instance.ShowDeadPanel();
    }

    IEnumerator ReloadSceneRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
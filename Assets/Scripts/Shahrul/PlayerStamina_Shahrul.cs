using UnityEngine;

public class PlayerStamina_Shahrul : MonoBehaviour
{
    [Header("Stamina Stats")]
    public float maxStamina = 100f;
    public float currentStamina;

    [Header("Drain Rules")]
    public float walkTimeLimit = 60f; // 1 minute
    public int attackCountLimit = 5;
    public float walkDrainRate = 10f; // -10 every 2 sec
    public float walkDrainInterval = 2f;
    public float attackDrainAmount = 20f;

    [Header("Regeneration")]
    public float regenAmount = 20f; // +20 Stamina
    public float regenInterval = 2f; // Every 2 seconds
    public float regenDelay = 2f; // Wait 2 seconds after using stamina before regen starts

    [Header("Token Status")]
    public bool hasUnlimitedStamina = false;

    private float walkTimer;
    private int attackCounter;
    private float drainTimer;

    // New variables for regeneration
    private float lastDrainTime;
    private float regenTimer;

    void Start()
    {
        currentStamina = maxStamina;
        UpdateUI();
    }

    void Update()
    {
        if (hasUnlimitedStamina)
        {
            currentStamina = maxStamina;
            UpdateUI();
            return;
        }

        // --- DRAIN LOGIC ---
        // Check Walking Input
        if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f)
        {
            walkTimer += Time.deltaTime;

            if (walkTimer > walkTimeLimit)
            {
                drainTimer += Time.deltaTime;
                if (drainTimer >= walkDrainInterval)
                {
                    DrainStamina(walkDrainRate);
                    drainTimer = 0f;
                }
            }
        }

        // --- REGENERATION LOGIC (Like Health) ---
        // If enough time has passed since the last stamina drain...
        if (Time.time > lastDrainTime + regenDelay && currentStamina < maxStamina)
        {
            regenTimer += Time.deltaTime;
            if (regenTimer >= regenInterval)
            {
                currentStamina += regenAmount;
                if (currentStamina > maxStamina) currentStamina = maxStamina;

                UpdateUI();
                regenTimer = 0f;
            }
        }
        else
        {
            regenTimer = 0f;
        }
    }

    public void OnAttack()
    {
        if (hasUnlimitedStamina) return;

        attackCounter++;

        if (attackCounter > attackCountLimit)
        {
            DrainStamina(attackDrainAmount);
        }
    }

    // Helper function to handle draining and resetting the regen timer
    // FIX: Made public so Combat script can call it for Dodge/Block drain
    public void DrainStamina(float amount)
    {
        currentStamina -= amount;
        if (currentStamina < 0) currentStamina = 0;

        lastDrainTime = Time.time; // Mark the time we lost stamina
        UpdateUI();
        Debug.Log($"Stamina Drained: {currentStamina}");
    }

    public void ActivateUnlimitedStamina()
    {
        hasUnlimitedStamina = true;
        if (HUDManager.instance != null)
            HUDManager.instance.SetUnlimitedStaminaVisuals(true);
    }

    void UpdateUI()
    {
        if (HUDManager.instance != null)
            HUDManager.instance.UpdateStaminaBar(currentStamina, maxStamina);
    }
}
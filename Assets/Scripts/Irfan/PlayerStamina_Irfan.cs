using UnityEngine;

public class PlayerStamina_Irfan : MonoBehaviour
{
    [Header("Stamina Stats")]
    public float maxStamina = 100f;
    public float currentStamina;

    [Header("Drain Rules")]
    public float walkTimeLimit = 60f; // Matched Shahrul
    public int attackCountLimit = 5;  // Matched Shahrul
    public float walkDrainRate = 10f; // Matched Shahrul
    public float walkDrainInterval = 2f; // Matched Shahrul
    public float attackDrainAmount = 20f; // Matched Shahrul

    [Header("Regeneration")]
    public float regenAmount = 20f; // Matched Shahrul
    public float regenInterval = 2f; // Matched Shahrul
    public float regenDelay = 2f; // Matched Shahrul

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
    void DrainStamina(float amount)
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
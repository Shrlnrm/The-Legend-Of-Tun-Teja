using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HUDManager : MonoBehaviour
{
    public static HUDManager instance;

    [Header("Main HUD Parent (Drag your entire HUD here)")]
    public GameObject hudContainer;

    [Header("Health UI")]
    public Slider healthSlider;

    [Header("Stamina UI")]
    public Slider staminaSlider;
    public Image staminaFillImage;
    public GameObject staminaLightningIcon;

    [Header("Stamina Colors")]
    public Color normalStaminaColor = Color.yellow;
    public Color unlimitedStaminaColor = Color.cyan;

    [Header("Enemy Counter UI")]
    public GameObject enemyCounterPanel;
    public Text enemyCountText;

    [Header("Token UI")]
    public Text tokenCountText;

    [Header("Settings")]
    public bool isTutorialLevel = false; // TICK THIS ONLY FOR LEVEL 1

    // New variables for counting
    private int totalEnemies;
    private int currentEnemiesLeft;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 1. HIDE HUD ONLY IF IT IS TICKED AS TUTORIAL LEVEL
        if (hudContainer != null)
        {
            hudContainer.SetActive(!isTutorialLevel);
        }
        else
        {
            // Fallback for individual components
            if (healthSlider != null) healthSlider.gameObject.SetActive(!isTutorialLevel);
            if (staminaSlider != null) staminaSlider.gameObject.SetActive(!isTutorialLevel);
            if (enemyCounterPanel != null) enemyCounterPanel.SetActive(!isTutorialLevel);
        }

        // Default Logic
        if (healthSlider != null) { healthSlider.minValue = 0; healthSlider.maxValue = 1; }
        if (staminaSlider != null) { staminaSlider.minValue = 0; staminaSlider.maxValue = 1; }

        // 2. AUTO-COUNT ENEMIES ON START
        CountEnemies();
    }

    void CountEnemies()
    {
        // Find all objects with EnemyHealth or GuardHealth scripts
        EnemyHealth_Ikmal[] enemies = FindObjectsOfType<EnemyHealth_Ikmal>();
        GuardHealth_Ikmal[] guards = FindObjectsOfType<GuardHealth_Ikmal>();

        totalEnemies = enemies.Length + guards.Length;
        currentEnemiesLeft = totalEnemies;

        // Initialize the UI text
        UpdateEnemyCount(currentEnemiesLeft, totalEnemies);
    }

    // Called by EnemyHealth_Ikmal or GuardHealth_Ikmal when they die
    public void RegisterEnemyDeath()
    {
        currentEnemiesLeft--;
        if (currentEnemiesLeft < 0) currentEnemiesLeft = 0;

        UpdateEnemyCount(currentEnemiesLeft, totalEnemies);

        // Optional: Check for win condition here if needed
        if (currentEnemiesLeft == 0)
        {
            Debug.Log("All enemies defeated!");
        }
    }

    // Called by TutorialOverlay
    public void ShowHUD()
    {
        if (hudContainer != null)
        {
            hudContainer.SetActive(true);
        }
        else
        {
            if (healthSlider != null) healthSlider.gameObject.SetActive(true);
            if (staminaSlider != null) staminaSlider.gameObject.SetActive(true);
            if (enemyCounterPanel != null) enemyCounterPanel.SetActive(true);
        }
    }

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthSlider != null) healthSlider.value = currentHealth / maxHealth;
    }

    public void UpdateStaminaBar(float currentStamina, float maxStamina)
    {
        if (staminaSlider != null) staminaSlider.value = currentStamina / maxStamina;
    }

    public void SetUnlimitedStaminaVisuals(bool isUnlimited)
    {
        if (staminaFillImage != null) staminaFillImage.color = isUnlimited ? unlimitedStaminaColor : normalStaminaColor;
        if (staminaLightningIcon != null) staminaLightningIcon.SetActive(isUnlimited);
    }

    public void UpdateEnemyCount(int current, int total)
    {
        if (enemyCountText != null) enemyCountText.text = $"Enemies Left : {current}/{total}";
    }

    public void SetupEnemyCounter(bool isVisible)
    {
        if (enemyCounterPanel != null) enemyCounterPanel.SetActive(isVisible);
        if (enemyCountText != null) enemyCountText.gameObject.SetActive(isVisible);
    }

    public void UpdateTokenCount(int current, int total)
    {
        if (tokenCountText != null) tokenCountText.text = $"Token of Loyalty : {current}/{total}";
    }
}
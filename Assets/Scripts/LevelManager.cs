using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    [Header("Level Settings")]
    public bool isTutorial = false;
    public bool isLevel4Truth = false;

    [Header("Unlock Settings")]
    public bool unlockNextLevelOnComplete = true;
    public bool useManualUnlockValue = false;
    [Tooltip("If checked, finishing this level sets LevelsUnlocked to THIS value.")]
    public int manualUnlockValue = 0;

    [Header("Taming Sari Settings")]
    public bool isLevel2TamingSari = false;
    public bool startWithTamingSari = false;

    [Header("Taming Sari Dialogue (Level 2 Only)")]
    // Drag your Dialogue info here in Inspector
    public DialogueTrigger.DialogueLine[] tamingSariDialogue;

    [Header("UI Panels")]
    public GameObject proceedPanel;
    public GameObject choicePanel;

    [Header("Ending Availability")]
    public bool isTruthLevelReady = false;
    public bool isLieLevelReady = false;
    public GameObject lockedMessagePanel;

    private int totalEnemies;
    private int currentEnemiesAlive;
    private int totalTokens;
    private int currentTokensCollected;
    private string pendingNextLevel;

    private string pendingTruthLevel;
    private string pendingLieLevel;

    private bool tamingSariUnlocked = false;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        Time.timeScale = 1f;

        if (proceedPanel != null) proceedPanel.SetActive(false);
        if (choicePanel != null) choicePanel.SetActive(false);
        if (lockedMessagePanel != null) lockedMessagePanel.SetActive(false);

        bool showCounterVisuals = !isTutorial;
        if (HUDManager.instance != null) HUDManager.instance.SetupEnemyCounter(showCounterVisuals);

        CheckEnemyCount();
        InvokeRepeating(nameof(CheckEnemyCount), 0.5f, 0.5f);

        TokenCollectible[] tokens = FindObjectsOfType<TokenCollectible>();
        totalTokens = tokens.Length;
        currentTokensCollected = 0;

        if (HUDManager.instance != null) HUDManager.instance.UpdateTokenCount(0, totalTokens);

        if (startWithTamingSari) UnlockForAnyCharacter();
    }

    void CheckEnemyCount()
    {
        int currentCount = FindObjectsOfType<EnemyHealth_Shahrul>().Length +
                           FindObjectsOfType<EnemyHealth_Ikmal>().Length +
                           FindObjectsOfType<EnemyHealth_Irfan>().Length;

        // ... Guard counting logic preserved ...
        GuardHealth_Shahrul[] shahrulGuards = FindObjectsOfType<GuardHealth_Shahrul>();
        foreach (var guard in shahrulGuards)
        {
            if (guard.GetComponent<Guard_Chase_Friendly_Shahrul>() == null) currentCount++;
        }
        GuardHealth_Ikmal[] ikmalGuards = FindObjectsOfType<GuardHealth_Ikmal>();
        foreach (var guard in ikmalGuards)
        {
            if (guard.GetComponent<Guard_Chase_Friendly_Ikmal>() == null) currentCount++;
        }
        GuardHealth_Irfan[] irfanGuards = FindObjectsOfType<GuardHealth_Irfan>();
        foreach (var guard in irfanGuards)
        {
            if (guard.GetComponent("Guard_Chase_Friendly_Irfan") == null) currentCount++;
        }

        if (totalEnemies == 0 && currentCount > 0) totalEnemies = currentCount;
        if (currentCount > totalEnemies) totalEnemies = currentCount;
        currentEnemiesAlive = currentCount;

        UpdateHUD();

        if (isLevel2TamingSari && !tamingSariUnlocked && currentEnemiesAlive == 0) UnlockTamingSari();
    }

    void UnlockTamingSari()
    {
        tamingSariUnlocked = true;
        UnlockForAnyCharacter();

        // NEW: Trigger the dialogue if lines are assigned
        if (tamingSariDialogue != null && tamingSariDialogue.Length > 0)
        {
            if (DialogueManager.instance != null)
            {
                // Start dialogue, passing null for the guard since we don't need one here
                DialogueManager.instance.StartDialogue(tamingSariDialogue, null);
            }
        }
    }

    void UnlockForAnyCharacter()
    {
        var ikmal = FindObjectOfType<PlayerCombat_Ikmal>();
        if (ikmal != null) ikmal.EquipTamingSari();
        var irfan = FindObjectOfType<PlayerCombat_Irfan>();
        if (irfan != null) irfan.EquipTamingSari();
    }

    public void TokenCollected()
    {
        currentTokensCollected++;
        if (HUDManager.instance != null) HUDManager.instance.UpdateTokenCount(currentTokensCollected, totalTokens);
    }

    void UpdateHUD()
    {
        if (HUDManager.instance != null) HUDManager.instance.UpdateEnemyCount(currentEnemiesAlive, totalEnemies);
    }

    // --- UI & PROGRESS SAVING ---

    public void ShowProceedPanel(string nextLevel)
    {
        pendingNextLevel = nextLevel;
        if (proceedPanel != null) { proceedPanel.SetActive(true); Time.timeScale = 0f; }

        if (unlockNextLevelOnComplete)
        {
            int levelToUnlock = 0;
            if (useManualUnlockValue)
            {
                levelToUnlock = manualUnlockValue;
            }
            else
            {
                int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
                levelToUnlock = currentSceneIndex + 1;
            }

            int currentSaved = PlayerPrefs.GetInt("LevelsUnlocked", 1);
            if (levelToUnlock > currentSaved)
            {
                PlayerPrefs.SetInt("LevelsUnlocked", levelToUnlock);
                PlayerPrefs.Save();
            }
        }
    }

    public void ShowDeadPanel()
    {
        ReloadCurrentLevel();
    }

    public void ShowChoicePanel(string truthScene, string lieScene)
    {
        pendingTruthLevel = truthScene;
        pendingLieLevel = lieScene;
        if (choicePanel != null) { choicePanel.SetActive(true); Time.timeScale = 0f; }

        if (unlockNextLevelOnComplete)
        {
            int levelToUnlock = 0;
            if (useManualUnlockValue) levelToUnlock = manualUnlockValue;
            else levelToUnlock = SceneManager.GetActiveScene().buildIndex + 1;

            int currentSaved = PlayerPrefs.GetInt("LevelsUnlocked", 1);
            if (levelToUnlock > currentSaved)
            {
                PlayerPrefs.SetInt("LevelsUnlocked", levelToUnlock);
                PlayerPrefs.Save();
            }
        }
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(pendingNextLevel)) SceneManager.LoadScene(pendingNextLevel);
    }

    public void LoadTruthLevel()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(pendingTruthLevel)) SceneManager.LoadScene(pendingTruthLevel);
        else ShowLockedMessage();
    }

    public void PlayLieCutscene()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(pendingLieLevel)) SceneManager.LoadScene(pendingLieLevel);
        else ShowLockedMessage();
    }

    void ShowLockedMessage()
    {
        if (lockedMessagePanel != null)
        {
            lockedMessagePanel.SetActive(true);
            StartCoroutine(HideLockedMessage());
        }
    }

    IEnumerator HideLockedMessage()
    {
        yield return new WaitForSecondsRealtime(2f);
        if (lockedMessagePanel != null) lockedMessagePanel.SetActive(false);
    }

    public void ReloadCurrentLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    [ContextMenu("Reset Level Progress")]
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("LevelsUnlocked");
        PlayerPrefs.Save();
        Debug.Log("Level Progress has been reset to Level 1.");
    }
}
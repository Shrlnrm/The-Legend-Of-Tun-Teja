using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject levelSelectPanel;
    public GameObject settingsPanel;

    [Header("Level Buttons")]
    public Button[] levelButtons;

    [Header("Level Scene Names")]
    public string[] levelSceneNames;

    void Start()
    {
        Time.timeScale = 1f; // Ensure time is running

        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        UpdateLevelButtons();
    }

    void Update()
    {
        // DEBUG: Press F10 OR 'R' to reset progress
        if (Input.GetKeyDown(KeyCode.F10) || Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Reset Input Detected!");
            ResetProgress();
        }
    }

    void UpdateLevelButtons()
    {
        int levelsUnlocked = PlayerPrefs.GetInt("LevelsUnlocked", 1);
        Debug.Log("Updating Buttons. Levels Unlocked: " + levelsUnlocked);

        for (int i = 0; i < levelButtons.Length; i++)
        {
            if (i < levelsUnlocked)
            {
                if (levelButtons[i] != null) levelButtons[i].interactable = true;
            }
            else
            {
                if (levelButtons[i] != null) levelButtons[i].interactable = false;
            }
        }
    }

    // --- BUTTON FUNCTIONS ---

    public void OpenLevelSelect()
    {
        if (mainMenuPanel) mainMenuPanel.SetActive(false);
        if (levelSelectPanel) levelSelectPanel.SetActive(true);
        if (settingsPanel) settingsPanel.SetActive(false);
        UpdateLevelButtons();
    }

    public void OpenSettings()
    {
        if (mainMenuPanel) mainMenuPanel.SetActive(false);
        if (levelSelectPanel) levelSelectPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(true);
    }

    public void BackToMainMenu()
    {
        if (levelSelectPanel) levelSelectPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
        if (mainMenuPanel) mainMenuPanel.SetActive(true);
    }

    public void PlayGame()
    {
        int levelsUnlocked = PlayerPrefs.GetInt("LevelsUnlocked", 1);
        int levelIndex = Mathf.Clamp(levelsUnlocked - 1, 0, levelSceneNames.Length - 1);
        Debug.Log($"PlayGame (Continue) clicked. Loading Index: {levelIndex}");
        LoadLevel(levelIndex);
    }

    public void StartNewGame()
    {
        Debug.Log("StartNewGame clicked. Loading Level 1 (Index 0).");
        // Optional: Reset progress when starting new game?
        // ResetProgress(); 
        LoadLevel(0);
    }

    public void LoadLevel(int levelIndex)
    {
        Debug.Log($"LoadLevel called with index: {levelIndex}");

        if (levelSceneNames == null || levelSceneNames.Length <= levelIndex)
        {
            Debug.LogError("MainMenuManager: Level Name not defined for index " + levelIndex);
            return;
        }

        string levelName = levelSceneNames[levelIndex];

        if (Application.CanStreamedLevelBeLoaded(levelName))
        {
            SceneManager.LoadScene(levelName);
        }
        else
        {
            Debug.LogError($"Scene '{levelName}' cannot be loaded. Check Build Settings or scene name.");
        }
    }

    public void QuitGame()
    {
        Debug.Log("Quitting...");
        Application.Quit();
    }

    [ContextMenu("Reset Progress")]
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("LevelsUnlocked");
        PlayerPrefs.Save();
        UpdateLevelButtons();
        Debug.Log("Progress Reset to Level 1!");
    }
}
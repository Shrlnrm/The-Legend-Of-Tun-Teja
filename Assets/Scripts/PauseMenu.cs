using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems; // Required for UI input check

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu instance;

    [Header("1. Main Background")]
    public GameObject pauseMenuPanel;

    [Header("2. Views (Drag Panels Here)")]
    public GameObject mainButtons;
    public GameObject infoPanel;
    public GameObject settingsPanel;

    public bool isPaused = false;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // FIX: Create EventSystem if missing (Crucial for buttons to work in new levels)
        if (FindObjectOfType<EventSystem>() == null)
        {
            Debug.Log("PauseMenu: EventSystem missing! Creating one automatically...");
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        if (pauseMenuPanel == gameObject)
        {
            Debug.LogError("PauseMenu: CRITICAL ERROR! Script attached to Panel. Move to empty GameObject.");
            return;
        }

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        ResetPanels();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Resume()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void Pause()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        ResetPanels();
        Time.timeScale = 0f;
        isPaused = true;
    }

    // --- NAVIGATION FUNCTIONS ---

    public void OpenInfo()
    {
        if (mainButtons) mainButtons.SetActive(false);
        if (infoPanel) infoPanel.SetActive(true);
        if (settingsPanel) settingsPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        if (mainButtons) mainButtons.SetActive(false);
        if (infoPanel) infoPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(true);
    }

    public void BackToMain()
    {
        ResetPanels();
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Debug.Log("Quitting...");
        Application.Quit();
    }

    void ResetPanels()
    {
        if (mainButtons) mainButtons.SetActive(true);
        if (infoPanel) infoPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
    }
}
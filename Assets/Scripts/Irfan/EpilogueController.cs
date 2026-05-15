using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using System.Collections; // Required for Coroutines

public class EpilogueController : MonoBehaviour
{
    // --- Configuration Settings ---
    [Header("Settings")]
    // The exact name of your main menu scene
    public string mainMenuSceneName = "Main Menu";
    // The key the player presses to skip the final text
    public KeyCode skipKey = KeyCode.T;
    // Duration to show the "Game Completed" screen before the final message
    public float gameCompleteDuration = 3f;
    // Automatic timeout duration (45 seconds) to return to the main menu
    public float autoSkipTime = 45f;

    // --- References ---
    [Header("References")]
    // The component controlling the Hang Tuah/Tun Teja animation
    public PlayableDirector timelineDirector;
    // The UI panel for the "Game Completed" message
    public GameObject gameCompleteScreen;
    // The UI panel for the final narrative text
    public GameObject truthEndingMessage;

    // --- State Management ---
    // Flag to track if the final text message is currently visible.
    private bool isMessageActive = false;

    void Awake()
    {
        // Ensure UI elements are hidden immediately when the scene starts.
        if (gameCompleteScreen != null) gameCompleteScreen.SetActive(false);
        if (truthEndingMessage != null) truthEndingMessage.SetActive(false);
        isMessageActive = false;
    }

    void Start()
    {
        // Subscribe to the event that fires when the Timeline finishes its run
        if (timelineDirector != null)
        {
            timelineDirector.stopped += OnCutsceneFinished;
        }
    }

    void Update()
    {
        // --- FIX IMPLEMENTED HERE ---
        // Allow skipping the *entire* sequence to the Main Menu 
        // the moment 'T' is pressed, regardless of the current cutscene stage.
        if (Input.GetKeyDown(skipKey))
        {
            LoadMainMenu();
        }
    }

    // Function called automatically when the Timeline (Animation) stops
    void OnCutsceneFinished(PlayableDirector director)
    {
        // Start the controlled sequence of UI elements using a Coroutine
        StartCoroutine(EndingSequenceFlow());
    }

    // Coroutine to manage the sequence: GameComplete -> EndingMessage -> Auto-Skip Timer
    IEnumerator EndingSequenceFlow()
    {
        // Ensure the game is running normally for input checks
        Time.timeScale = 1.0f;
        isMessageActive = false;

        // 1. Show the "Game Completed" Screen
        if (gameCompleteScreen != null)
        {
            gameCompleteScreen.SetActive(true);

            // Wait for the defined duration (e.g., 3 seconds)
            yield return new WaitForSeconds(gameCompleteDuration);

            gameCompleteScreen.SetActive(false);
        }

        // 2. Show the Final Truth Ending Message
        if (truthEndingMessage != null)
        {
            truthEndingMessage.SetActive(true);
            // Set the flag to TRUE. Though Update() is now unconditional, 
            // this flag is still useful for other potential UI management.
            isMessageActive = true;
        }

        // 3. Auto-Skip Timer
        // Wait for 45 seconds. This timer will be cancelled by LoadMainMenu if 'T' is pressed.
        yield return new WaitForSeconds(autoSkipTime);

        // This code only executes if the 45-second timer runs out.
        if (isMessageActive)
        {
            LoadMainMenu();
        }
    }

    // Function to load the main menu scene
    void LoadMainMenu()
    {
        // Stop all coroutines, which cancels the 45-second timer if it's running
        StopAllCoroutines();

        // Reset time scale and load the main menu
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // Unsubscribe from the event when the object is destroyed or disabled
    void OnDisable()
    {
        if (timelineDirector != null)
        {
            timelineDirector.stopped -= OnCutsceneFinished;
        }
    }
}
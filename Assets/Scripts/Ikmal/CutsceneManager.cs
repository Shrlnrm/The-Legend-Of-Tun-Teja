using UnityEngine;
using UnityEngine.SceneManagement; // Needed to change scenes
using UnityEngine.Playables;       // Needed to talk to the Timeline

public class CutsceneManager : MonoBehaviour
{
    [Header("Settings")]
    public string mainMenuSceneName = "MainMenu"; // Type EXACT name of your Main Menu scene here
    public KeyCode skipKey = KeyCode.T;           // The key to press to skip

    [Header("References")]
    public PlayableDirector timelineDirector;     // Drag your Timeline object here

    void Start()
    {
        // This listens for when the timeline finishes playing naturally
        if (timelineDirector != null)
        {
            timelineDirector.stopped += OnCutsceneFinished;
        }
    }

    void Update()
    {
        // Check if the player presses the Skip Key (T)
        if (Input.GetKeyDown(skipKey))
        {
            LoadMainMenu();
        }
    }

    // This function runs automatically when the Timeline stops
    void OnCutsceneFinished(PlayableDirector director)
    {
        LoadMainMenu();
    }

    void LoadMainMenu()
    {
        // Loads the Main Menu scene
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // Good practice: Unsubscribe from the event to prevent errors if the object is destroyed
    void OnDisable()
    {
        if (timelineDirector != null)
        {
            timelineDirector.stopped -= OnCutsceneFinished;
        }
    }
}
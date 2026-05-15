using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorTrigger : MonoBehaviour
{
    public static DoorTrigger instance;

    // Added 'LoadTruthCutscene' option
    public enum DoorAction { ShowProceedPanel, ShowChoicePanel, LoadLevelDirectly, LoadTruthCutscene }

    [Header("Door Settings")]
    public DoorAction actionType = DoorAction.ShowProceedPanel;
    public string nextLevelName;

    [Header("Choice Panel Destinations")]
    public string truthLevelName = "Level 4 Truth";
    public string lieLevelName = "Cutscene_Lie";

    [Header("Truth Cutscene (For End of Truth Level)")]
    public string truthCutsceneName = "Cutscene_Truth"; // Drag/Type scene name here

    [Header("Level 1 Specifics")]
    public bool isLevel1Trigger = false;
    public bool isDoorLocked = false;

    [Header("Components")]
    public Collider2D doorCollider;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        if (doorCollider == null) doorCollider = GetComponent<Collider2D>();
        if (doorCollider != null) doorCollider.isTrigger = true;

        if (isLevel1Trigger) isDoorLocked = true;
        else isDoorLocked = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        bool isPlayer = collision.CompareTag("Player") ||
                        collision.GetComponent<PlayerController_Shahrul>() != null ||
                        collision.GetComponent<PlayerController_Ikmal>() != null ||
                        collision.GetComponent<PlayerController_Irfan>() != null;

        if (isPlayer)
        {
            if (!isDoorLocked)
            {
                ExecuteDoorAction();
            }
            else
            {
                Debug.Log("The door is locked!");
            }
        }
    }

    public void UnlockDoor()
    {
        isDoorLocked = false;
        Debug.Log("Door Unlocked!");
    }

    void ExecuteDoorAction()
    {
        // Simple fallback if LevelManager missing
        if (LevelManager.instance == null)
        {
            if (actionType == DoorAction.LoadTruthCutscene)
                SceneManager.LoadScene(truthCutsceneName);
            else
                SceneManager.LoadScene(nextLevelName);
            return;
        }

        switch (actionType)
        {
            case DoorAction.ShowProceedPanel:
                LevelManager.instance.ShowProceedPanel(nextLevelName);
                break;
            case DoorAction.ShowChoicePanel:
                LevelManager.instance.ShowChoicePanel(truthLevelName, lieLevelName);
                break;
            case DoorAction.LoadLevelDirectly:
                SceneManager.LoadScene(nextLevelName);
                break;
            case DoorAction.LoadTruthCutscene:
                // Direct load logic for cutscene
                Debug.Log("Loading Truth Cutscene: " + truthCutsceneName);
                SceneManager.LoadScene(truthCutsceneName);
                break;
        }
    }
}
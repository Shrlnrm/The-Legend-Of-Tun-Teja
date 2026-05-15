using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TutorialOverlay : MonoBehaviour
{
    public static TutorialOverlay instance;

    [Header("UI Components")]
    public GameObject overlayPanel;
    public Button playButton;

    [Header("Settings")]
    public bool showImmediatelyOnStart = true; // Set TRUE for Level 1

    private bool isTutorialActive = false;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(HideOverlay);
        }

        if (showImmediatelyOnStart)
        {
            ShowOverlayNow();
        }
        else
        {
            if (overlayPanel != null) overlayPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (isTutorialActive)
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                HideOverlay();
            }
        }
    }

    // New Direct Show Method
    public void ShowOverlayNow()
    {
        if (overlayPanel != null)
        {
            overlayPanel.SetActive(true);
            overlayPanel.transform.SetAsLastSibling();
            isTutorialActive = true;
            Time.timeScale = 0f; // Pause Game
        }
    }

    // Called for delays (e.g. after dialogue) - kept for compatibility
    public void ShowOverlay()
    {
        StartCoroutine(ShowOverlayWithDelay());
    }

    IEnumerator ShowOverlayWithDelay()
    {
        yield return new WaitForSecondsRealtime(2f);
        ShowOverlayNow();
    }

    public void HideOverlay()
    {
        if (overlayPanel != null)
        {
            overlayPanel.SetActive(false);
            isTutorialActive = false;
            Time.timeScale = 1f;

            // 1. Tell SceneFader to start fading out (if it hasn't already)
            if (SceneFader.instance != null)
            {
                SceneFader.instance.StartFade();
            }

            // 2. SHOW THE HUD
            if (HUDManager.instance != null)
            {
                HUDManager.instance.ShowHUD();
            }
        }
    }
}
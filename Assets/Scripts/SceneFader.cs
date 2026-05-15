using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    public static SceneFader instance;
    public Image blackScreen;
    public float fadeSpeed = 0.5f;

    [Header("Settings")]
    public bool autoFadeOnStart = true; // Default to TRUE for normal levels

    void Awake()
    {
        if (instance == null) instance = this;
        Time.timeScale = 1f;
    }

    void Start()
    {
        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(true);

            // Special Check: If this is Level 1, we might wait for tutorial.
            // But if autoFadeOnStart is TRUE, we fade anyway.
            // You should UNCHECK autoFadeOnStart only for Level 1 in the Inspector.

            if (autoFadeOnStart)
            {
                StartCoroutine(FadeOutSequence());
            }
        }
    }

    public void StartFade()
    {
        StartCoroutine(FadeOutSequence());
    }

    IEnumerator FadeOutSequence()
    {
        // 1. FREEZE PLAYER AT START
        FreezePlayer(true);

        // 2. FADE OUT BLACK SCREEN
        Color c = blackScreen.color;
        c.a = 1f;
        blackScreen.color = c;

        while (c.a > 0)
        {
            c.a -= Time.deltaTime * fadeSpeed;
            blackScreen.color = c;
            yield return null;
        }

        blackScreen.gameObject.SetActive(false);

        // 3. UNFREEZE PLAYER
        FreezePlayer(false);
    }

    void FreezePlayer(bool freeze)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            MonoBehaviour pScript = FindObjectOfType<PlayerCombat_Shahrul>();
            if (pScript == null) pScript = FindObjectOfType<PlayerCombat_Ikmal>();
            if (pScript == null) pScript = FindObjectOfType<PlayerCombat_Irfan>();
            if (pScript != null) player = pScript.gameObject;
        }

        if (player == null) return;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        Animator anim = player.GetComponent<Animator>();

        if (freeze)
        {
            if (rb) { rb.linearVelocity = Vector2.zero; rb.bodyType = RigidbodyType2D.Kinematic; }
            if (anim) { anim.SetBool("IsRunning", false); anim.SetFloat("Speed", 0); }

            MonoBehaviour[] scripts = player.GetComponents<MonoBehaviour>();
            foreach (var script in scripts)
            {
                string name = script.GetType().Name;
                if (name.Contains("Controller") || name.Contains("Combat")) script.enabled = false;
            }
        }
        else
        {
            if (rb) rb.bodyType = RigidbodyType2D.Dynamic;

            MonoBehaviour[] scripts = player.GetComponents<MonoBehaviour>();
            foreach (var script in scripts)
            {
                string name = script.GetType().Name;
                if (name.Contains("Controller") || name.Contains("Combat")) script.enabled = true;
            }
        }
    }
}
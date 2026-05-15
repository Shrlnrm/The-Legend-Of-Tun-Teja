using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    [Header("UI Components")]
    public GameObject dialoguePanel;
    public Text nameText;
    public Text dialogueText;
    public Image portraitImage; // Optional

    [Header("Settings")]
    public float typingSpeed = 0.04f;

    public bool isDialogueActive = false;
    private Queue<DialogueTrigger.DialogueLine> sentences;
    private bool isTyping = false;
    private string currentFullSentence = "";

    // The guard we need to wake up (Optional)
    private Guard_Level1_Special_Shahrul pendingGuard;

    // Whether to unlock the door after this dialogue
    private bool pendingUnlockDoor;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        // Ensure queue is initialized immediately
        sentences = new Queue<DialogueTrigger.DialogueLine>();
    }

    void Start()
    {
        // Safety check: ensure queue exists if Awake failed
        if (sentences == null) sentences = new Queue<DialogueTrigger.DialogueLine>();

        // FIX: Hide the box at start so it only appears when triggered
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        if (dialoguePanel == null)
            Debug.LogError("DialogueManager: Dialogue Panel is missing in Inspector!");
    }

    void Update()
    {
        // PRESS T TO CONTINUE
        if (isDialogueActive && Input.GetKeyDown(KeyCode.T))
        {
            DisplayNextSentence();
        }
    }

    // Overload 1: Basic start with just lines (for LevelManager Taming Sari)
    public void StartDialogue(DialogueTrigger.DialogueLine[] lines, Guard_Level1_Special_Shahrul guard)
    {
        StartDialogue(lines, guard, false);
    }

    // Overload 2: Full start with optional guard and door unlock
    public void StartDialogue(DialogueTrigger.DialogueLine[] lines, Guard_Level1_Special_Shahrul guard, bool unlockDoor)
    {
        Debug.Log("DialogueManager: StartDialogue called.");

        if (lines == null || lines.Length == 0)
        {
            Debug.LogError("DialogueManager: Conversation has NO lines! Check the Inspector.");
            return;
        }

        if (sentences == null) sentences = new Queue<DialogueTrigger.DialogueLine>();

        isDialogueActive = true;
        pendingGuard = guard;
        pendingUnlockDoor = unlockDoor;

        if (dialoguePanel != null) dialoguePanel.SetActive(true);

        sentences.Clear();
        foreach (var line in lines)
        {
            sentences.Enqueue(line);
        }

        FreezePlayer(true);
        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            if (dialogueText != null) dialogueText.text = currentFullSentence;
            isTyping = false;
            return;
        }

        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueTrigger.DialogueLine currentLine = sentences.Dequeue();
        currentFullSentence = currentLine.sentence;

        if (nameText != null) nameText.text = currentLine.characterName;

        // Handle Portrait
        if (portraitImage != null)
        {
            if (currentLine.portrait != null)
            {
                portraitImage.sprite = currentLine.portrait;
                portraitImage.gameObject.SetActive(true);
            }
            else
            {
                portraitImage.gameObject.SetActive(false);
            }
        }

        StopAllCoroutines();
        StartCoroutine(TypeSentence(currentFullSentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        if (dialogueText != null) dialogueText.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            if (dialogueText != null) dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    public void EndDialogue()
    {
        Debug.Log("DialogueManager: Conversation Ended.");
        isDialogueActive = false;

        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        FreezePlayer(false);

        // WAKE UP THE GUARD (Check for null first!)
        if (pendingGuard != null)
        {
            pendingGuard.BeginSparring();
            pendingGuard = null;
        }

        // UNLOCK DOOR IF REQUESTED
        if (pendingUnlockDoor)
        {
            if (DoorTrigger.instance != null)
            {
                DoorTrigger.instance.UnlockDoor();
            }
            pendingUnlockDoor = false;
        }
    }

    void FreezePlayer(bool freeze)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        // FIX 2: Better Fallback. If Tag is missing, don't grab the Guard!
        if (player == null)
        {
            MonoBehaviour pScript = FindObjectOfType<PlayerCombat_Shahrul>();
            if (pScript == null) pScript = FindObjectOfType<PlayerCombat_Ikmal>();
            if (pScript == null) pScript = FindObjectOfType<PlayerCombat_Irfan>();

            if (pScript != null) player = pScript.gameObject;
        }

        if (player == null)
        {
            Debug.LogError("DialogueManager: Could not find Player! Ensure Player has tag 'Player' or a Combat script.");
            return;
        }

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
                if (name.Contains("Controller") || name.Contains("Combat"))
                    script.enabled = false;
            }
        }
        else
        {
            if (rb) rb.bodyType = RigidbodyType2D.Dynamic;

            MonoBehaviour[] scripts = player.GetComponents<MonoBehaviour>();
            foreach (var script in scripts)
            {
                string name = script.GetType().Name;
                if (name.Contains("Controller") || name.Contains("Combat"))
                    script.enabled = true;
            }
        }
    }
}
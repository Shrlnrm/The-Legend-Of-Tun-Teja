using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        public string characterName;
        public Sprite portrait;
        [TextArea(3, 10)]
        public string sentence;
    }

    [Header("Conversation")]
    public DialogueLine[] conversation;

    [Header("Actions")]
    public Guard_Level1_Special_Shahrul guardToActivate;
    public bool unlockDoorOnEnd = false; // CHECK THIS ONLY FOR SULTAN

    [Header("Settings")]
    public bool oneTimeOnly = true;

    private bool hasTriggered = false;

    void Start()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        bool isPlayer = collision.CompareTag("Player") || collision.GetComponent<Rigidbody2D>() != null;

        if (isPlayer && !hasTriggered)
        {
            TriggerDialogue();
        }
    }

    public void TriggerDialogue()
    {
        if (DialogueManager.instance != null)
        {
            // Pass the 'unlockDoorOnEnd' bool to the manager
            DialogueManager.instance.StartDialogue(conversation, guardToActivate, unlockDoorOnEnd);

            hasTriggered = true;

            if (oneTimeOnly)
            {
                GetComponent<Collider2D>().enabled = false;
            }
        }
        else
        {
            Debug.LogError("DialogueTrigger: Could not find 'DialogueManager'!");
        }
    }
}
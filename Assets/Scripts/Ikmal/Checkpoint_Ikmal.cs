using UnityEngine;

public class Checkpoint_Ikmal : MonoBehaviour
{
    [Header("Visuals (Optional)")]
    public SpriteRenderer spriteRenderer;
    public Sprite activeSprite; // Drag a "Lit Campfire" or "Green Flag" sprite here

    private bool isActivated = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object entering is the player
        if (other.CompareTag("Player"))
        {
            if (isActivated) return; // Don't activate again if already active

            PlayerHealth_Ikmal playerHealth = other.GetComponent<PlayerHealth_Ikmal>();

            if (playerHealth != null)
            {
                // Set the player's new respawn point to this object's position
                playerHealth.UpdateCheckpoint(transform.position);
                ActivateCheckpoint();
            }
        }
    }

    void ActivateCheckpoint()
    {
        isActivated = true;
        Debug.Log("Checkpoint Activated!");

        // Optional: Change the sprite to show it's active
        if (spriteRenderer != null && activeSprite != null)
        {
            spriteRenderer.sprite = activeSprite;
        }
    }
}
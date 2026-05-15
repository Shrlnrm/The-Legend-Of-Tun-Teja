using UnityEngine;

public class Checkpoint_Shahrul : MonoBehaviour
{
    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;
    public Sprite activeSprite;

    private bool isActivated = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (isActivated) return;

            PlayerHealth_Shahrul playerHealth = other.GetComponent<PlayerHealth_Shahrul>();

            if (playerHealth != null)
            {
                playerHealth.UpdateCheckpoint(transform.position);
                ActivateCheckpoint();
            }
        }
    }

    void ActivateCheckpoint()
    {
        isActivated = true;
        Debug.Log("Checkpoint Activated for Shahrul!");

        if (spriteRenderer != null && activeSprite != null)
        {
            spriteRenderer.sprite = activeSprite;
        }
    }
}
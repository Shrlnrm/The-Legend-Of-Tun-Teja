using UnityEngine;

public class TokenCollectible : MonoBehaviour
{
    [Header("Effects")]
    public GameObject pickupEffect;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"Token touched by: {collision.gameObject.name}");

        bool isPlayer = collision.CompareTag("Player");

        if (isPlayer)
        {
            collision.SendMessage("ActivateUnlimitedStamina", SendMessageOptions.DontRequireReceiver);
            collision.SendMessageUpwards("ActivateUnlimitedStamina", SendMessageOptions.DontRequireReceiver);

            if (LevelManager.instance != null)
            {
                LevelManager.instance.TokenCollected();
            }

            if (SoundManager.instance != null)
            {
                SoundManager.instance.PlayTokenSound();
            }

            if (pickupEffect != null)
                Instantiate(pickupEffect, transform.position, Quaternion.identity);

            Debug.Log("Token Collected!");
            Destroy(gameObject);
        }
    }
}
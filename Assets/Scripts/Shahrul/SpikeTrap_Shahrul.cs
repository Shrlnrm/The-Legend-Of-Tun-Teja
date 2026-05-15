using UnityEngine;

public class SpikeTrap_Shahrul : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Kill Player (Shahrul)
        PlayerHealth_Shahrul player = collision.GetComponent<PlayerHealth_Shahrul>();
        if (player != null)
        {
            Debug.Log("Spikes killed Player (Shahrul)!");
            player.TakeDamage(9999);
            return;
        }

        // 2. Kill Enemy (Shahrul)
        EnemyHealth_Shahrul enemy = collision.GetComponent<EnemyHealth_Shahrul>();
        if (enemy != null)
        {
            Debug.Log("Spikes killed Enemy (Shahrul)!");
            enemy.TakeDamage(9999);
            return;
        }

        // 3. Kill Guard (Shahrul)
        GuardHealth_Shahrul guard = collision.GetComponent<GuardHealth_Shahrul>();
        if (guard != null)
        {
            Debug.Log("Spikes killed Guard (Shahrul)!");
            guard.TakeDamage(9999);
        }
    }
}
using UnityEngine;

public class SpikeTrap_Ikmal : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Kill Player (Ikmal)
        PlayerHealth_Ikmal player = collision.GetComponent<PlayerHealth_Ikmal>();
        if (player != null)
        {
            Debug.Log("Spikes killed Player (Ikmal)!");
            player.TakeDamage(9999);
            return;
        }

        // 2. Kill Enemy (Ikmal)
        EnemyHealth_Ikmal enemy = collision.GetComponent<EnemyHealth_Ikmal>();
        if (enemy != null)
        {
            Debug.Log("Spikes killed Enemy (Ikmal)!");
            enemy.TakeDamage(9999);
            return;
        }

        // 3. Kill Guard (Ikmal)
        GuardHealth_Ikmal guard = collision.GetComponent<GuardHealth_Ikmal>();
        if (guard != null)
        {
            Debug.Log("Spikes killed Guard (Ikmal)!");
            guard.TakeDamage(9999);
            return;
        }

        // 4. Kill Tun Teja (Ikmal) - ADDED
        TunTejaHealth_Ikmal teja = collision.GetComponent<TunTejaHealth_Ikmal>();
        if (teja != null)
        {
            Debug.Log("Spikes killed Tun Teja!");
            teja.TakeDamage(9999);
        }
    }
}
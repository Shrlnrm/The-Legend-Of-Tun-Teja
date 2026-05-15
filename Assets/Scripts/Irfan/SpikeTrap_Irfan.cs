using UnityEngine;

public class SpikeTrap_Irfan : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Kill Player (Irfan)
        PlayerHealth_Irfan player = collision.GetComponent<PlayerHealth_Irfan>();
        if (player != null)
        {
            Debug.Log("Spikes killed Player (Irfan)!");
            player.TakeDamage(9999);
            return;
        }

        // 2. Kill Enemy (Irfan)
        EnemyHealth_Irfan enemy = collision.GetComponent<EnemyHealth_Irfan>();
        if (enemy != null)
        {
            Debug.Log("Spikes killed Enemy (Irfan)!");
            enemy.TakeDamage(9999);
            return;
        }

        // 3. Kill Guard (Irfan)
        GuardHealth_Irfan guard = collision.GetComponent<GuardHealth_Irfan>();
        if (guard != null)
        {
            Debug.Log("Spikes killed Guard (Irfan)!");
            guard.TakeDamage(9999);
        }
    }
}
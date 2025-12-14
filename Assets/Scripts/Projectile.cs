using UnityEngine;

public class Projectile : MonoBehaviour
{
    public PowerUpType powerUpType; // Ateş mi Asit mi?

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. ENGEL KONTROLÜ (Eski kodumuz)
        Obstacle obs = collision.GetComponent<Obstacle>();
        if (obs != null)
        {
            obs.Interact(powerUpType);
            Destroy(gameObject);
            return;
        }

        // 2. DÜŞMAN KONTROLÜ (YENİ KISIM)
        // Çarptığım objede 'EnemyController' scripti var mı?
        EnemyController enemy = collision.GetComponent<EnemyController>();

        if (enemy != null)
        {
            Destroy(gameObject); // Mermiyi yok et
            return;
        }

        // 3. DUVAR KONTROLÜ (Boşa giderse)
        // Layer kontrolü veya Tag kontrolü
        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
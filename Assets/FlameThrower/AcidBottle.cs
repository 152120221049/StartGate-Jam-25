using UnityEngine;

public class AcidBottle : MonoBehaviour
{
    public GameObject acidPuddlePrefab; // Yerde oluşacak tuzak prefabı

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Metal Eritme
        Obstacle obs = collision.GetComponent<Obstacle>();
        if (obs != null)
        {
            if (obs.type == ObstacleType.Metal)
            {
                obs.Interact(PowerUpType.Acid);
                // Metal eridi ama şişe de kırıldı, yere dökülsün mü?
                // İstersen burada da SpawnPuddle() çağırabilirsin.
                Destroy(gameObject);
                return;
            }
            else
            {
                // Başka engele (Duvar/Kutu) çarparsa kırılıp yere dökülür
                SpawnPuddle();
                Destroy(gameObject);
            }
        }
        // 2. Duvar/Zemin Çarpması
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Wall") || collision.CompareTag("Ground"))
        {
            SpawnPuddle();
            Destroy(gameObject);
        }
        // 3. Direkt Düşmana Gelirse
        else if (collision.GetComponent<EnemyController>())
        {
            SpawnPuddle(); // Düşmanın ayağının dibine dökülsün
            Destroy(gameObject);
        }
    }

    void SpawnPuddle()
    {
        Instantiate(acidPuddlePrefab, transform.position, Quaternion.identity);
    }
}
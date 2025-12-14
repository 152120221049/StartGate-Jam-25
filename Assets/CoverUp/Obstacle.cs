using UnityEngine;

public enum ObstacleType
{
    Wood,   // Ahşap (Ateş yakar)
    Ice,    // Buz (Ateş eritir)
    Metal,  // Metal (Asit eritir, Büyüme iter)
    SmallHole, // Küçük delik (Küçülme ile geçilir)
    Movable, // Hareket ettirilebilir (Telekinezi/Portal)
    Normal // Hiçbir şeyden etkilenmez
}

public class Obstacle : MonoBehaviour
{
    public ObstacleType type; // Inspector'dan seç: Bu obje ne?

    // Bu objeye bir power-up ile etkileşime girildiğinde ne olsun?
    public void Interact(PowerUpType powerUp)
    {
        switch (type)
        {
            case ObstacleType.Wood:
                if (powerUp == PowerUpType.FireGun) DestroyObject();
                break;

            case ObstacleType.Ice:
                if (powerUp == PowerUpType.FireGun) MeltObject();
                break;

            case ObstacleType.Metal:
                if (powerUp == PowerUpType.Acid) MeltObject();
                break;

                // Diğer etkileşimler buraya...
        }
    }

    void DestroyObject()
    {
        // Efekt patlat, ses çal vs.
        Destroy(gameObject);
    }

    void MeltObject()
    {
        // Erime animasyonu oynatılabilir
        Destroy(gameObject);
    }
}
using UnityEngine;

public class FlameArea : MonoBehaviour
{
    public float slowPercentage = 0.5f; // %50 yavaşlatma

    // Temas Başladığında Yavaşlat
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                // Düşmanı yavaşlat (Süresiz)
                enemy.ApplySlow(slowPercentage);
            }
        }
    }

    // Temas Bittiğinde Hızı Geri Ver
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                // Yavaşlatmayı kaldır
                enemy.RemoveSlow();
            }
        }
    }
}
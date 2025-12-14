using UnityEngine;
using System.Collections;

public class AcidPuddle : MonoBehaviour
{
    public float stunDuration = 3f; // Sersemletme süresi
    public float lifetime = 5f; // Aynı düşmana tekrar sersemletme uygulama süresi

    private float nextStunTime = 0f;
    private void Start()
    {
        // Asit havuzu 5 saniye sonra yok olsun
        Destroy(gameObject, lifetime);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {   
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                // Düşmanı sersemlet (Süresiz sabitleme istiyoruz, bu yüzden duration vermiyoruz)
                enemy.ApplyStun();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                // Düşman alandan çıkınca sabitlemeyi kaldır
                enemy.RemoveStun();
            }
        }
    }
}
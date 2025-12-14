using UnityEngine;
using System.Collections;

public class RoomControl : MonoBehaviour
{
    [Header("Ayarlar")]
    public Transform playerSpawnPoint; // Referans noktamız artık burası
    public float triggerDistance = 1.5f; // Spawn noktasına ne kadar yaklaşınca kilitlensin?

    private BoxCollider2D roomCollider;
    private bool hasEntered = false;
    [Header("Oda Verisi")]
    // Bu değer LevelGenerator tarafından atanacaktır.
    public float roomCountdownDuration = 0f; // Oda için geri sayım süresi (saniye)
    private void Awake()
    {
        roomCollider = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Odaya İLK GİRİŞ
            if (!hasEntered)
            {
                hasEntered = true;

                // 1. Kamera ve Generator (Standart işlemler)
                RoomCamera cam = Camera.main.GetComponent<RoomCamera>();
                if (cam != null && roomCollider != null) cam.MoveToRoom(roomCollider,roomCountdownDuration);

                if (LevelGenerator.Instance != null) LevelGenerator.Instance.OnRoomEntered(this.gameObject);

                // 2. Spawn Noktasına Yaklaşma Kontrolü (Sayacı başlatacak)
                if (playerSpawnPoint != null)
                {
                    StartCoroutine(CheckProximityAndLock(other.transform));
                }
                else
                {
                    Debug.LogWarning("Spawn Point atanmamış. Kapı hemen kilitlendi ve Sayaç Başladı.");
   
                    if (GameManager.Instance != null)
                        GameManager.Instance.StartCountdownForRoom(roomCountdownDuration);
                }
            }
            // ODAYI TERK ETME (Çıkış Kapısı Trigger'ı)
            else
            {
                // KRİTİK: Sonraki odaya geçerken müziği normale döndür
                if (GameManager.Instance != null && GameManager.Instance.musicManager != null)
                {
                    GameManager.Instance.musicManager.PlayGameMusic();
                }

                // NOT: Kapıların kendisi (RoomConnector) LevelGenerator'ın SpawnNextRoom metodunu çağırır.
            }
        }
    }

    private IEnumerator CheckProximityAndLock(Transform playerTransform)
    {
        while (Vector2.Distance(playerTransform.position, playerSpawnPoint.position) > triggerDistance)
        {
            yield return null;
        }

        // KRİTİK: Geri sayımı başlat!
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartCountdownForRoom(roomCountdownDuration);
        }
    }
}
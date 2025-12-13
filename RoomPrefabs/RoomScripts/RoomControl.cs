using UnityEngine;
using System.Collections;

public class RoomControl : MonoBehaviour
{
    [Header("Ayarlar")]
    public Transform playerSpawnPoint; // Referans noktamız artık burası
    public float triggerDistance = 1.5f; // Spawn noktasına ne kadar yaklaşınca kilitlensin?

    private BoxCollider2D roomCollider;
    private bool hasEntered = false;

    private void Awake()
    {
        roomCollider = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasEntered)
        {
            hasEntered = true;

            // 1. Kamera ve Generator (Standart işlemler)
            RoomCamera cam = Camera.main.GetComponent<RoomCamera>();
            if (cam != null && roomCollider != null) cam.MoveToRoom(roomCollider);

            if (LevelGenerator.Instance != null) LevelGenerator.Instance.OnRoomEntered(this.gameObject);

            // 2. Spawn Noktasına Yaklaşma Kontrolü
            if (playerSpawnPoint != null)
            {
                StartCoroutine(CheckProximityAndLock(other.transform));
            }
            else
            {
                // Eğer spawn noktası koymayı unuttuysan hemen kilitle (Hata önlemi)
                Debug.LogWarning("Oda prefabında Player Spawn Point atanmamış! Kapı hemen kilitlendi.");
                LockEntrance();
            }
        }
    }

    private IEnumerator CheckProximityAndLock(Transform playerTransform)
    {
        // --- BEKLEME DÖNGÜSÜ ---
        // Oyuncu ile Spawn Noktası arasındaki mesafe, tetiklenme mesafesinden BÜYÜK olduğu sürece bekle.
        // Yani oyuncu uzaktaysa döngü döner. Yaklaştığı an (Mesafe < triggerDistance) döngü kırılır.
        while (Vector2.Distance(playerTransform.position, playerSpawnPoint.position) > triggerDistance)
        {
            yield return null; // Bir sonraki kareyi bekle
        }

        // Oyuncu spawn noktasına yeterince yaklaştı. KİLİTLE!
        LockEntrance();
    }

    private void LockEntrance()
    {
        RoomConnector[] connectors = GetComponentsInChildren<RoomConnector>();
        foreach (RoomConnector connector in connectors)
        {
            if (connector.isEntrance)
            {
                connector.CloseDoor();
            }
        }
    }
}
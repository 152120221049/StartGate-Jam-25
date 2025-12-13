using UnityEngine;

public class RoomSpawnTrigger : MonoBehaviour
{
    private bool hasSpawned = false; // Aynı odadan 50 tane yaratmasın diye kontrol

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Çarpan şeyin etiketi "Player" mı ve daha önce spawn ettik mi?
        if (other.CompareTag("Player") && !hasSpawned)
        {
            // LevelGenerator'ı bul ve yeni oda yap de
            LevelGenerator generator = FindObjectOfType<LevelGenerator>();
            if (generator != null)
            {
                generator.SpawnNextRoom();
                hasSpawned = true; // Artık bu kapı işlevsiz, tekrar çalışmasın

                // İstersen tetikleyiciyi yok et (Temizlik için)
                // Destroy(gameObject); 
            }
        }
    }
}
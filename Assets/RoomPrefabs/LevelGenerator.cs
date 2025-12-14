using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LevelGenerator : MonoBehaviour
{
    public static LevelGenerator Instance; // Diğer scriptlerden ulaşmak için

    [Header("Listeler")]
    public List<RoomData> allRooms;
    public List<PowerUpType> currentAbilities;

    [Header("Oda Takibi")]
    public int maxActiveRooms = 1; // Sahnede aynı anda kaç oda kalsın? (Arkamızdakileri silmek için)
    private List<GameObject> activeRooms = new List<GameObject>();
    private RoomConnector lastExitPoint;
    [Header("Düşman Ayarları")]
    public GameObject finalRoomEnemyPrefab;
    private void Awake()
    {
        Instance = this;
    }
    public void SpawnFinalEnemy()
    {
        if (finalRoomEnemyPrefab == null)
        {
            Debug.LogError("Son Düşman Prefab'ı LevelGenerator'da atanmamış!");
            return; // HATA NOKTASI 1: Prefab Ataması Eksik
        }

        // 1. Oyuncunun bulunduğu odayı bul (En son oluşturulan oda)
        GameObject currentRoomObj = activeRooms.LastOrDefault();

        if (currentRoomObj == null)
        {
            Debug.LogError("Oda bulunamadı! Düşman çağrılamıyor.");
            return; // HATA NOKTASI 2: Liste Boş
        }

        // 2. RoomControl'ü bul
        RoomControl roomCtrl = currentRoomObj.GetComponent<RoomControl>();

        if (roomCtrl != null && roomCtrl.playerSpawnPoint != null)
        {
            // Düşmanı, PlayerSpawnPoint'in tam pozisyonunda doğur
            Vector3 spawnPos = roomCtrl.playerSpawnPoint.position;

            // Düşmanı oluştur
            GameObject newEnemy = Instantiate(finalRoomEnemyPrefab, spawnPos, Quaternion.identity);
            newEnemy.transform.SetParent(currentRoomObj.transform);

            Debug.Log("Tehdit doğdu: " + newEnemy.name); // Başarılıysa bu log görünmeli
        }
        else
        {
            // HATA NOKTASI 3: RoomControl veya Spawn Point Eksik
            Debug.LogError("Düşman çağrılamadı: RoomControl veya Spawn Point eksik.");
        }
        if (finalRoomEnemyPrefab == null)
        {
            Debug.LogError("Son Düşman Prefab'ı LevelGenerator'da atanmamış!");
            return;
        }

       
        
    }
    public void BeginGeneration()
    {
        if (!currentAbilities.Contains(PowerUpType.None)) currentAbilities.Add(PowerUpType.None);

        // Sanal başlangıç noktası oluşturma kodlarını buraya taşı
        GameObject startRef = new GameObject("StartReference");
        startRef.transform.position = Vector3.zero;
        lastExitPoint = startRef.AddComponent<RoomConnector>();
        lastExitPoint.direction = DoorDirection.Top;

        SpawnNextRoom(true);
    }
    public void ResetLevel()
    {
        // 1. Sahnedeki aktif odaların hepsini yok et
        foreach (GameObject room in activeRooms)
        {
            if (room != null) Destroy(room);
        }

        // 2. Listeyi temizle
        activeRooms.Clear();

        // 3. Yetenekleri sıfırla (Sadece None kalsın)
        currentAbilities.Clear();
        // (BeginGeneration fonksiyonunda tekrar "None" ekleneceği için burası boş kalsa da olur)

        // 4. Eğer varsa eski referans noktasını temizle
        if (lastExitPoint != null)
        {
            // StartReference objesini bulup yok etmemiz lazım
            // lastExitPoint bir Component olduğu için onun GameObject'ini siliyoruz
            Destroy(lastExitPoint.gameObject);
        }
    }

    // isFirstRoom: Eğer true ise karakteri odaya ışınlayacağız
    public void SpawnNextRoom(bool isFirstRoom = false)
    {
        DoorDirection requiredEntryDir = GetOppositeDirection(lastExitPoint.direction);
        Debug.Log("Sistem şu özellikte oda arıyor -> Yön: " + requiredEntryDir + " | Yetenek: None (veya varolanlar)");
        List<RoomData> possibleRooms = allRooms.Where(room =>
            // 1. Odanın istediği TÜM yetenekler bende var mı? (ALL fonksiyonu)
            room.requiredPowerUp.All(req => currentAbilities.Contains(req)) &&

            // 2. Yön tutuyor mu?
            room.entryDirection == requiredEntryDir
        ).ToList();
        Debug.Log("Bulunan uygun oda sayısı: " + possibleRooms.Count);

        if (possibleRooms.Count == 0)
        {
            Debug.LogError("KRİTİK HATA: Uygun oda bulunamadı! Listenizi veya StartReference yönünü kontrol edin.");
            return;
        }

        if (possibleRooms.Count == 0) return;

        RoomData selectedRoom = possibleRooms[Random.Range(0, possibleRooms.Count)];
        GameObject newRoomObj = Instantiate(selectedRoom.roomPrefab);
        // Odayı listeye ekle (Silmek için takip ediyoruz)
        activeRooms.Add(newRoomObj);
        RoomControl roomCtrl = newRoomObj.GetComponent<RoomControl>();
        if (roomCtrl != null)
        {
            // KRİTİK: Oda süresini RoomData'dan alıp RoomControl'e atıyoruz
            // Bu değerin RoomData'da 'enemySpawnCountdown' olarak tanımlı olduğunu varsayıyoruz.
            roomCtrl.roomCountdownDuration = selectedRoom.timeLimit;
        }
        // --- POZİSYONLAMA KISMI ---
        RoomConnector[] connectors = newRoomObj.GetComponentsInChildren<RoomConnector>();
        RoomConnector newEntry = connectors.FirstOrDefault(x => x.isEntrance);
        RoomConnector newExit = connectors.FirstOrDefault(x => !x.isEntrance);

        if (newEntry != null && newExit != null)
        {
            Vector3 offset = newEntry.transform.position - newRoomObj.transform.position;
            newRoomObj.transform.position = lastExitPoint.transform.position - offset;
            lastExitPoint = newExit;
        }

        // --- KARAKTERİ İÇERİ IŞINLAMA KISMI (Sadece ilk oda için) ---
        if (isFirstRoom)
        {
            RoomControl currentroomCtrl = newRoomObj.GetComponent<RoomControl>();
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (currentroomCtrl != null && roomCtrl.playerSpawnPoint != null && player != null)
            {
                // Spawn noktasının X ve Y'sini al, ama Z'yi zorla SIFIR yap.
                Vector3 spawnPos = roomCtrl.playerSpawnPoint.position;
                player.transform.position = new Vector3(spawnPos.x, spawnPos.y, 0f);

                // Kamerayı da oraya odakla (Kamera Z = -10 kalmalı)
                Camera.main.transform.position = new Vector3(spawnPos.x, spawnPos.y, -10f);
            }
        }
    }

    // Odaya girildiğinde çağrılır, eski odaları temizler
    public void OnRoomEntered(GameObject currentRoom)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetRoomState();
        }

        // MÜZİĞİ SIFIRLAMA MANTIĞI
        if (GameManager.Instance != null && GameManager.Instance.musicManager != null)
        {
            GameManager.Instance.musicManager.PlayGameMusic();
        }

        // Eğer listemiz sınırın üstüne çıktıysa
        if (activeRooms.Count > maxActiveRooms)
        {
            // ... (Geri kalan temizlik kodları) ...
            GameObject oldRoom = activeRooms[0];
            activeRooms.RemoveAt(0);
            CleanRoomContents(oldRoom);
            Destroy(oldRoom);
        }
    }
    private void CleanRoomContents(GameObject roomToClean)
    {
        // Yalnızca bu odanın altındaki objeleri temizle
        // Örn: Bu odada kalan düşman (finalEnemy), mermi artıkları, asit alanları vb.

        // Düşmanı kontrol et (Enemy tag'li objeler)
        // Eğer düşman currentRoomObj'nin altındaysa:
        Transform[] children = roomToClean.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child.CompareTag("Enemy") || child.name.Contains("FinalEnemy")) // Veya direk adı ile
            {
                Destroy(child.gameObject);
            }
            // Örn: Asit alanını temizleme
            if (child.GetComponent<AcidPuddle>() != null)
            {
                Destroy(child.gameObject);
            }
        }

        // NOT: Düşmanı LevelGenerator.SpawnFinalEnemy() metodunda odanın altına (SetParent) koyduğumuz için bu yöntem çalışır.
    }

    private DoorDirection GetOppositeDirection(DoorDirection dir)
    {
        switch (dir)
        {
            case DoorDirection.Top: return DoorDirection.Bottom;
            case DoorDirection.Bottom: return DoorDirection.Top;
            case DoorDirection.Left: return DoorDirection.Right;
            case DoorDirection.Right: return DoorDirection.Left;
            default: return DoorDirection.Bottom;
        }
    }
}
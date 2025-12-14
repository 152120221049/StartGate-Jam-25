using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Panelleri")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject inGameHUD;
    public GameObject creditsUI;
    public GameObject pausePanel;
    public GameObject gameOverPanel;

    [Header("Oyun Bileşenleri")]
    public GameObject player;
    public LevelGenerator levelGenerator;
    public RoomCamera roomCamera;
    public SkillUIManager skillUIManager;
    public MusicManager musicManager;
    private bool isGameActive = false;
    private bool isPaused = false;
    private bool isGameOver = false; // YENİ: Game Over durumunu takip et
    [Header("Geri Sayım Ayarları")]
    public TextMeshProUGUI countdownText; // Ekranda sayacı gösteren Text bileşeni
    private float currentCountdownTime;
    private bool isCountingDown = false;
    private bool enemiesSpawned = false;
    private RoomControl currentRoomCtrl;
    [Header("Puan Sistemi")]
    public int currentScore = 0;
    public TextMeshProUGUI scoreText; // Puanı gösterecek UI elementi
    [Header("Game Over UI")]
    public TextMeshProUGUI finalScoreText;                                  // ...

    public void AddScore(int amount)
    {
        currentScore += amount;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "PUAN: " + currentScore.ToString();
        }
    }
    private void Awake()
    {
        // Singleton Deseni
        if (Instance == null)
        {
            Instance = this;
            // SceneManagement kullanıyorsanız: DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        ShowMainMenu();
        if (player != null) player.SetActive(false);
    }
    private void Start()
    {
        musicManager.PlayMenuMusic();
    }
    private void Update()
    {
        // YENİ: Sadece oyun aktif ve Game Over değilse ESC tuşunu dinle
        if (isGameActive && !isGameOver)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (isPaused) ResumeGame();
                else PauseGame();
            }
        }
        if (isCountingDown && !enemiesSpawned && !isGameOver)
        {
            currentCountdownTime -= Time.deltaTime;
            UpdateCountdownUI();

            if (currentCountdownTime <= 0f)
            {
                currentCountdownTime = 0f;
                StopCountdown();

                // Sayım bitti, düşmanları çağır ve müziği değiştir
                SpawnEnemyAndChangeMusic();
            }
        }
    }
    // Odadan Geçiş/Giriş Sırasında Çağrılacak
    public void StartCountdownForRoom(float roomCountdownDuration)
    {
        if (enemiesSpawned) return; // Düşmanlar zaten çağrıldıysa tekrar başlatma

        if (roomCountdownDuration <= 0)
        {
            // Sayaç sıfırsa hemen düşmanları çağır
            currentCountdownTime = 0;
            SpawnEnemyAndChangeMusic();
            return;
        }

        currentCountdownTime = roomCountdownDuration;
        isCountingDown = true;

        if (countdownText != null) countdownText.gameObject.SetActive(true);
        UpdateCountdownUI();
    }

    private void StopCountdown()
    {
        isCountingDown = false;
        // UI'ı gizle
        if (countdownText != null)
        {
            // KRİTİK: Sayaç bittiğinde gizle
            countdownText.gameObject.SetActive(false);
        }
    }

    private void UpdateCountdownUI()
    {
        if (countdownText != null)
        {
            countdownText.text = Mathf.CeilToInt(currentCountdownTime).ToString();
        }
    }

    // YENİ: Düşman Çağırma ve Müzik Tetikleme
    private void SpawnEnemyAndChangeMusic()
    {
        if (enemiesSpawned) return;
        enemiesSpawned = true;

        Debug.Log("Geri Sayım Bitti! Düşman Çağrılıyor ve Müzik Değişiyor.");

        // 1. Düşman Çağır
        if (LevelGenerator.Instance != null)
        {
            LevelGenerator.Instance.SpawnFinalEnemy(); // LevelGenerator'daki yeni metot
        }

        // 2. Müzik Değiştir
        if (musicManager != null)
        {
            musicManager.PlayEnemyMusic();
        }

        
    }
    

    // YENİ: Düşman Çağırma Mantığı

    // --- ÖLÜM YÖNETİMİ ---
    // PlayerHealth.cs'ten çağrılacak kritik metot
    public void OnGameOver()
    {
        if (isGameOver) return;
        if (LevelGenerator.Instance != null)
        {
            LevelGenerator.Instance.ResetLevel(); // Level Generator'un temizliğini yap
        }
        musicManager.PlayGameOverMusic();
        // KRİTİK: DİNAMİK OBJELERİ TEMİZLE
        ClearSceneObjects();
        player.SetActive(false);
        isGameOver = true;
        isGameActive = false;
        isPaused = false;
        if (finalScoreText != null)
        {
            finalScoreText.text = "Veri Değeri\n" + currentScore.ToString()+" MB";
        }
        // 1. Oyunu Durdur
        Time.timeScale = 0f;

        // 2. Panelleri ayarla
        gameOverPanel.SetActive(true);
        pausePanel.SetActive(false);
        inGameHUD.SetActive(false);

        Debug.Log("Game Over! Oyuncu öldü.");
        
    }

    // --- PAUSE SİSTEMİ ---
    public void PauseGame()
    {
        if (!isGameActive || isGameOver) return;

        isPaused = true;
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
        inGameHUD.SetActive(false);
    }

    public void ResumeGame()
    {
        if (!isPaused) return;

        isPaused = false;
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        inGameHUD.SetActive(true);
    }

    // --- ANA MENÜYE DÖNÜŞ VE OYUN SIFIRLAMA ---
    public void OnMainMenuButton()
    {
        isGameActive = false;
        isGameOver = false;
        isPaused = false;
        StopCountdown(); // Sayacı durdur
        enemiesSpawned = false;
        // 1. Zamanı düzelt
        Time.timeScale = 1f;

        if (LevelGenerator.Instance != null)
        {
            LevelGenerator.Instance.ResetLevel(); // Level Generator'un temizliğini yap
        }

        // KRİTİK: DİNAMİK OBJELERİ TEMİZLE
        ClearSceneObjects();
        // 2. Panelleri ayarla
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false); // Game Over panelini de kapat
        ShowMainMenu();
        musicManager.PlayMenuMusic();

        // 4. OYUNCU TEMİZLİĞİ
        if (player != null)
        {
            player.SetActive(false);
            player.transform.position = Vector3.zero;

            // PlayerHealth'in varsa, onu da sıfırla
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Eğer PlayerHealth.cs'te reset metodu varsa çağır
                // playerHealth.ResetHealth(); 
            }
        }

        // 5. KAMERA SIFIRLAMA
        if (roomCamera != null)
        {
            roomCamera.transform.position = new Vector3(0, 0, -10);
            roomCamera.target = null;
        }

        Debug.Log("Oyun sıfırlandı ve Ana Menüye dönüldü.");
    }

    // --- BUTON FONKSİYONLARI ---
    public void ResetRoomState()
    {
        enemiesSpawned = false; // KRİTİK: Düşman çağrıldı bayrağını sıfırla
        isCountingDown = false; // Sayaç durumunu sıfırla
        currentCountdownTime = 0f;

        // Sayaç yazısını varsayılan olarak gizle (StartCountdownForRoom tekrar açacak)
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }

        Debug.Log("GameManager: Oda durumu sıfırlandı. Yeni sayaç başlatılabilir.");
    }
    public void OnPlayButton()
    {
        // 1. Durumları sıfırla ve zamanı başlat
        isGameActive = true;
        isGameOver = false;
        isPaused = false;
        Time.timeScale = 1f;

        // 2. UI'ı hazırla
        gameOverPanel.SetActive(false);
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        inGameHUD.SetActive(true);

        // 3. Oyuncuyu aktif et ve canını sıfırla
        if (player != null)
        {
            player.SetActive(true);
            // ... (PlayerHealth reset kodları) ...

            // KRİTİK: Player aktif olduktan SONRA SkillUIManager'ı başlat!
            PlayerSkillManager playerSkillManager = player.GetComponent<PlayerSkillManager>();

            if (skillUIManager != null && playerSkillManager != null)
            {
                // SkillUIManager'a PlayerSkillManager referansını vererek başlat
                skillUIManager.InitializeUI(playerSkillManager);
            }
            else
            {
                Debug.LogError("SkillUIManager veya PlayerSkillManager referansı eksik! Lütfen GameManager Inspector'ını kontrol edin.");
            }
        }

        if (LevelGenerator.Instance != null)
        {
            LevelGenerator.Instance.ResetLevel();
        }
        ClearSceneObjects(); // Düşmanları ve eşyaları yok et

        // ... (UI ve Oyuncu Aktif Etme Kodları) ...
        enemiesSpawned = false;
        currentCountdownTime = 0; // Sayacı sıfırla

        // 4. Bölüm Oluşturucuyu Çalıştır
        if (levelGenerator != null)
        {
            levelGenerator.BeginGeneration();
        }

        // 5. Kamerayı ayarla
        if (roomCamera != null)
        {
            roomCamera.target = player.transform;
        }
        musicManager.PlayGameMusic();
        Debug.Log("Oyun Başladı!");
    }

    public void OnSettingsButton()
    {
        // ... (Mevcut kodlar) ...
        mainMenuPanel.SetActive(false);
        pausePanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void OnBackToMenuButton()
    {
        if (LevelGenerator.Instance != null)
        {
            LevelGenerator.Instance.ResetLevel(); // Level Generator'un temizliğini yap
        }

        // KRİTİK: DİNAMİK OBJELERİ TEMİZLE
        ClearSceneObjects();
        player.SetActive(false);
        // ... (Mevcut kodlar) ...
        gameOverPanel.SetActive(false);
        settingsPanel.SetActive(false);
        creditsUI.SetActive(false);

        // Nereden geldiysek oraya dön
        if (isPaused) { pausePanel.SetActive(true); }
        else { mainMenuPanel.SetActive(true); }
        musicManager.PlayMenuMusic();
    }

    public void OnCreditButton()
    {
        // ... (Mevcut kodlar) ...
        mainMenuPanel.SetActive(false);
        creditsUI.SetActive(true);
    }

    public void OnQuitButton()
    {
        // ... (Mevcut kodlar) ...
        Debug.Log("Oyundan çıkılıyor...");
        Application.Quit();
    }

    private void ShowMainMenu()
    {
        // ... (Mevcut kodlar) ...
        isGameActive = false;
        Time.timeScale = 1f;

        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        inGameHUD.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
    }
    private void ClearSceneObjects()
    {
        // 1. DÜŞMANLARI TEMİZLE
        // "Enemy" tag'ine sahip tüm aktif objeleri bul ve yok et.
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            // Debug.Log($"Temizlenen Düşman: {enemy.name}");
            Destroy(enemy);
        }

        // 2. YETENEK KUTULARI/EŞYALARI TEMİZLE
        // "SkillPickup" veya "Loot" gibi bir tag kullanıyorsanız, o tag'i kullanın.
        // Eğer SkillPickup'lar bir konteyner objesindeyse, o objeyi temizleyin.

        // Geçici çözüm: Sahnedeki SkillPickup scriptine sahip tüm objeleri bulalım
        SkillPickup[] pickups = FindObjectsOfType<SkillPickup>();
        foreach (SkillPickup pickup in pickups)
        {
            // Debug.Log($"Temizlenen Eşya: {pickup.name}");
            Destroy(pickup.gameObject);
        }

        // Not: Eğer LevelGenerator.ResetLevel() düşmanları temizlemiyorsa,
        // o metodun içindeki temizlik mantığını da buraya taşıyabilirsiniz.
    }
}
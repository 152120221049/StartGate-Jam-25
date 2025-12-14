using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    // Temel Referanslar
    private Rigidbody2D rb;
    private Transform player;

    [Header("Temel Ayarlar")]
    public float baseSpeed = 3f;
    public float visionRange = 10f;
    public float stoppingDistance = 1f;

    [Header("Durum Çarpanları")]
    private float currentSpeedMultiplier = 1f;
    private bool isStunned = false;
    [Header("Saldırı Ayarları")]
    public float damageAmount = 1f; // Oyuncuya verilecek hasar
    public float attackCooldown = 1.5f; // Saldırı sonrası düşmanın hareketsiz kalma süresi

    private bool isOnAttackCooldown = false; // Düşman şu an hareketsiz mi?
   
    public AudioClip walkClip;         // Yürüme döngüsü (isteğe bağlı)
    private AudioSource audioSource;
    void Start()
    {
        
        rb = GetComponent<Rigidbody2D>();
        // Oyuncuyu sahneden bul (Player tag'ini kontrol et)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("Sahneden 'Player' tag'li obje bulunamadı!");
        }
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource != null && walkClip != null)
        {
            audioSource.clip = walkClip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. Saldırı Cooldown'unda değilse ve Oyuncuya çarptıysa devam et
        if (isOnAttackCooldown || collision.gameObject.CompareTag("Player") == false)
        {
            return;
        }

        // Oyuncu Health scriptini bul
        PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            // 2. Oyuncuya Hasar Ver
            playerHealth.TakeDamage(damageAmount);
            SoundManager.Instance.PlayOneShot(SoundManager.Instance.enemyAttackSFX);
            // 3. Düşman için Saldırı Hareketsizliğini Başlat
            StartCoroutine(AttackCooldownRoutine());
        }
    }

    // Düşmanın Saldırı Hareketsizliği Rutini
    IEnumerator AttackCooldownRoutine()
    {
        isOnAttackCooldown = true;

        // Saldırı anında düşmanı tam durdurmak için hızını sıfırla
        rb.linearVelocity = Vector2.zero;

        // Hareketsiz Kalma Süresi
        yield return new WaitForSeconds(attackCooldown);

        // Cooldown bitti
        isOnAttackCooldown = false;
    }
    void FixedUpdate()
    {
        if (player == null || isStunned||isOnAttackCooldown) return;

        
        Vector2 directionToPlayer = (player.position - transform.position);
        float distance = directionToPlayer.magnitude;

        // Takip Mantığı
        if (distance <= visionRange && distance > stoppingDistance)
        {
            // Hareket Vektörü
            Vector2 movementVector = directionToPlayer.normalized * baseSpeed * currentSpeedMultiplier;
            rb.linearVelocity = movementVector;
            SoundManager.Instance.StartLoopingSound(walkClip);
            // Çevreye Duyarlı Tepki (Basitçe oyuncuya dönme)
            // (Karmaşık çevre tepkileri için A* veya NavMesh sistemleri gerekir, şimdilik yön değiştirme yeterli)

            // Düşmanın yönünü harekete göre çevirme (Opsiyonel görsel iyileştirme)
            if (movementVector.x != 0)
            {
                // Eğer Sprite Renderer kullanıyorsanız:
                // GetComponent<SpriteRenderer>().flipX = movementVector.x < 0; 
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    // --- YETENEK TEPKİLERİ ---

    // Alev Silahı Yavaşlatma Etkisi
    public void ApplySlow(float slowAmount)
    {
        // Çarpanı ayarla (Örn: 1 - 0.5 = 0.5 kat hız)
        currentSpeedMultiplier = 1f - slowAmount;
        
    }

    // YENİ METOT: Yavaşlatmayı Kaldırma
    public void RemoveSlow()
    {
        // Hızı normale döndür
        currentSpeedMultiplier = 1f;
    }

    // Asit Sabitleme/Sersemletme Etkisi
    // EnvironmentAwareEnemy.cs içinde:

    public void ApplyStun()
    {
       
        if (isStunned) return;
        SoundManager.Instance.PlayOneShot(SoundManager.Instance.enemyStunSFX);
        isStunned = true;
        // Hareketi sıfırla
        rb.linearVelocity = Vector2.zero;
    }

    // YENİ METOT: Sabitlemeyi Kaldırma
    public void RemoveStun()
    {
        isStunned = false;
        // Hız çarpanlarını normale döndür (Hemen hareket etmeye başlasın)
        currentSpeedMultiplier = 1f;
    }

   

}
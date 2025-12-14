using UnityEngine;

// ARTIK BASEWEAPON DEĞİL, SKILLBASE'DEN TÜREYORUZ!
public class AcidWeapon : SkillBase
{
    [Header("Asit Ayarları")]
    public GameObject acidBottlePrefab;
    public Transform firePoint;
    public float throwForce = 10f;

    // Start metodu gerekirse buraya ekle (Şimdilik boş kalabilir)
    private void Start()
    {
        // FirePoint'i otomatik bulma
        if (firePoint == null)
        {
            // 1. Önce Player objesinin Transform'una (Parent'ına) eriş
            Transform playerParent = transform.parent;

            if (playerParent != null)
            {
                // 2. Player altındaki "FirePoint" adlı alt objeyi ara
                firePoint = playerParent.Find("Firepoint");

                if (firePoint == null)
                {
                    Debug.LogError("AcidWeapon hatası: 'FirePoint' adlı alt obje Player objesinde (Parent'ta) bulunamadı!");
                }
            }
            else
            {
                Debug.LogError("AcidWeapon hatası: Scriptin Parent'ı (Player) yok!");
            }
        }
    }
    // --- BİRİNCİL KULLANIM (E tuşu) ---
    public override void UsePrimary()
    {
        // 1. Can ve Cooldown kontrolü (SkillBase'den gelir)
        if (!IsReady() || !ConsumeDurability())
        {
            return; // Cooldown'da veya cephane yoksa çık
        }

        // Yönü bulma (PlayerMovement'tan almalısın)
        PlayerMovement playerMovement = GetComponentInParent<PlayerMovement>();
        if (playerMovement == null) return;
        Vector2 direction = playerMovement.FacingDirection;

        // 2. Şişeyi yarat ve fırlat
        GameObject bottle = Instantiate(acidBottlePrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D rb = bottle.GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction * throwForce;

        // 3. Başarılı kullanım sonrası cooldown'ı başlat
        // NOT: Cooldown zaten ConsumeDurability içinde değil, manuel başlatılmalı
        StartCooldown();
    }

    // AcidWeapon'ın ikincil kullanımı (Q) yoksa burası boş kalır.
    public override void UseSecondary()
    {
        // Boş bırakılabilir
    }
}
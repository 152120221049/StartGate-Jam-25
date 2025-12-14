using UnityEngine;
using System.Collections;

public class SizeController : SkillBase
{
    // Kopyalama sisteminde referanslarýn kodla bulunmasý gerekir.
    private PlayerMovement playerMovement;
    private Rigidbody2D rb;

    [Header("Ayarlar")]
    public float effectDuration = 5f;

    [Header("Boyut Ayarlarý")]
    public float normalScale = 1.0f;
    public float giantScale = 2.0f;
    public float tinyScale = 0.5f;

    [Header("Hýz Çarpanlarý")]
    public float giantSpeedMult = 0.6f;
    public float tinySpeedMult = 1.3f;

    [Header("Kütle (Mass) Ayarlarý")]
    public float normalMass = 1f;
    public float giantMass = 200f;
    public float tinyMass = 0.5f;

    private enum SizeState { Normal, Giant, Tiny }
    private SizeState currentSize = SizeState.Normal;
    private Coroutine activeTimer;

    // YENÝ EK: Start() yerine yetenek takýldýðýnda kesin çalýþacak olan Start() metodu
    void Start()
    {
        // Rigidbody2D'nin Player üzerinde olduğunu varsayıyoruz.
        playerMovement = GetComponentInParent<PlayerMovement>();
        rb = GetComponentInParent<Rigidbody2D>();

        if (playerMovement == null || rb == null)
        {
            // PlayerSkillManager'ın da parent olduğunu varsayarsak,
            // PlayerMovement/Rigidbody'nin Player objesinde olması gerekir.
            Debug.LogError("SizeController: PlayerMovement veya Rigidbody2D 'Player' objesinde bulunamadı!");
            enabled = false; // Hata varsa scripti kapat
            return;
        }

        // Başlangıçta boyutu sıfırla (Çünkü bu skill objesi yeni Instantiate edildi)
        ResetSize();
    }
    //...
    void OnDisable()
    {
        // Yalnızca aktif olarak boyutu değiştiren bu script ise sıfırlama yap
        if (currentSize != SizeState.Normal && transform.parent != null)
        {
            ResetSize(); // Sadece Normal değilken ve parent mevcutken sıfırla.
        }
        if (activeTimer != null) StopCoroutine(activeTimer);
    }

    // --- SKILLBASE ENTEGRASYONU ---

    // E Tuþu (UsePrimary) = Devleþme
    public override void UsePrimary()
    {
        // Cooldown kontrolü (SkillBase'den gelir)
        if (!IsReady())
        {
            // Eðer IsReady false döndürürse, yukarýdaki SkillBase metodundan uyarý logu zaten çýkar.
            return; // Hýzlýca çýkýþ yap.
        }

        if (currentSize != SizeState.Giant)
        {
            ApplySize(SizeState.Giant);
            nextFireTime = Time.time + cooldownTime;
        }
    }

    // Q Tuþu (UseSecondary) = Küçülme
    public override void UseSecondary()
    {
        // Cooldown kontrolü (SkillBase'den gelir)
        if (!IsReady())
        {
            // Eðer IsReady false döndürürse, yukarýdaki SkillBase metodundan uyarý logu zaten çýkar.
            return; // Hýzlýca çýkýþ yap.
        }

        if (currentSize != SizeState.Tiny)
        {
            ApplySize(SizeState.Tiny);
            nextFireTime = Time.time + cooldownTime;
        }
    }

    // --- TEMÝZLÝK VE KONTROL ---

    void ApplySize(SizeState newState)
    {
        // Eðer zaten o boyuttaysak, timer'ý durdurup yeniden baþlatmaya gerek yok.
        if (currentSize == newState) return;

        if (activeTimer != null) StopCoroutine(activeTimer);

        currentSize = newState;

        float targetScale = normalScale;
        float targetSpeedMult = 1f;
        float targetMass = normalMass;

        switch (newState)
        {
            case SizeState.Giant:
                targetScale = giantScale;
                targetSpeedMult = giantSpeedMult;
                targetMass = giantMass;
                break;
            case SizeState.Tiny:
                targetScale = tinyScale;
                targetSpeedMult = tinySpeedMult;
                targetMass = tinyMass;
                break;
            case SizeState.Normal:
                targetScale = normalScale;
                targetSpeedMult = 1f;
                targetMass = normalMass;
                break;
        }

        // 1. Boyutu Güncelle
        // DÝKKAT: Objenin scale'ini deðiþtirmek yerine, Player'ýn scale'ini deðiþtirmeliyiz.
        // Bu script Player'ýn alt objesi olduðu için, Player'ýn Transform'unu almalýyýz.
        if (transform.parent != null)
        {
            transform.parent.localScale = new Vector3(targetScale, targetScale, 1f);
        }

        // 2. Hýzý Güncelle
        if (playerMovement != null) playerMovement.speedMultiplier = targetSpeedMult;

        // 3. Kütleyi (Mass) Güncelle
        if (rb != null) rb.mass = targetMass;

        // Süre sayacý
        if (newState != SizeState.Normal)
        {
            activeTimer = StartCoroutine(RevertToNormalRoutine());
        }
    }

    void ResetSize()
    {
        ApplySize(SizeState.Normal);
    }

    IEnumerator RevertToNormalRoutine()
    {
        yield return new WaitForSeconds(effectDuration);
        ApplySize(SizeState.Normal);
    }
}
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System.Collections; // Coroutine için şart

public class PlayerHealth : MonoBehaviour
{
    [Header("Can Ayarları")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Görsel Efektler")]
    public PostProcessVolume postProcessVolume;
    private Vignette vignette;
    public GameObject gameOverPanel;
    // Pulse ve Low Health ayarları
    private float targetIntensity = 0f;
    public float damagePulseAmount = 0.8f; // Darbe şiddetini artırdım (0.8 daha iyidir)
    public float recoverySpeed = 2f; // Ekranın düzelme hızı (Daha yavaş olsun ki görelim)
    public float lowHealthIntensity = 0.6f;

    // Hasar yediğimiz anı kontrol etmek için
    private bool isHit = false;

    private void Start()
    {
        currentHealth = maxHealth;
        if (postProcessVolume != null) postProcessVolume.profile.TryGetSettings(out vignette);
    }

    private void Update()
    {
        if (vignette == null) return;
        if (currentHealth <= 0) Die(); // Öldüysek efektleri güncelleme
        // 1. Düşük Can Hesaplaması (Taban Değer)
        float healthPercent = currentHealth / maxHealth;
        float baseIntensity = 0f;

        if (healthPercent < 0.4f)
        {
            // Can azaldıkça koyuluk artar
            baseIntensity = (1f - healthPercent) * lowHealthIntensity;
        }

        // 2. Darbe Sonrası Toparlanma
        // Eğer darbe yemediysek (Coroutine çalışmıyorsa), yavaşça taban değere dön
        if (!isHit)
        {
            // Mathf.MoveTowards, Lerp'ten daha kontrollüdür. Sabit hızla düşer.
            vignette.intensity.value = Mathf.MoveTowards(vignette.intensity.value, baseIntensity, recoverySpeed * Time.deltaTime);
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        // --- YENİ DARBE SİSTEMİ ---
        // Hasar yediğinde ayrı bir işlem (Routine) başlat
        if (vignette != null)
        {
            StopAllCoroutines(); // Üst üste hasar yerse eskisin iptal et, yenisini yak
            StartCoroutine(HitFlash());
        }

        Debug.Log("Can: " + currentHealth);
        SoundManager.Instance.PlayOneShot(SoundManager.Instance.playerHitSFX);
        if (currentHealth <= 0) Die();
    }

    // Bu fonksiyon darbeyi "çakar" ve biraz bekletir
    private IEnumerator HitFlash()
    {
        isHit = true; // Update fonksiyonuna "Sen karışma, kontrol bende" diyoruz

        // 1. ŞAK! Diye ekranı kızart (Pürüzsüz değil, aniden)
        vignette.intensity.value = damagePulseAmount;

        // 2. Çok kısa bir süre (0.1 saniye) kıpkırmızı kalsın (Gözün algılaması için)
        yield return new WaitForSeconds(0.1f);

        isHit = false; // Kontrolü tekrar Update'e bırak, o yavaşça indirecek
    }
    public void ResetHealth()
    {
        currentHealth = maxHealth;
    }

    // ... (TakeDamage metodu) ...

    void Die()
    {
        if (currentHealth > 0) return;
        
        // 1. GameManager'a Ölümü bildir
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver();
        }
        vignette.intensity.value = 0; 
        currentHealth = maxHealth; // Canı tam doldur
        // NOT: Zamanı durdurma ve panel gösterme işini GameManager yapacak.
    }
}
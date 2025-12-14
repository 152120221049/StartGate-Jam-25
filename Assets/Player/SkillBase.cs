using UnityEngine;

    
public abstract class SkillBase : MonoBehaviour
{
    public string skillName;
    public float cooldownTime = 1f;
    protected float nextFireTime = 0f;
    [Header("Görsel")]
    public Sprite skillSprite; // İkon için yeni alan
    [Header("Dayanýklýlýk")]
    public int maxDurability = 4;
    [HideInInspector]
    public int currentDurability;
    [Header("Sesler")]
    public AudioClip useSoundClip;
    public AudioClip outOfDurabilityClip;

    // Ortak Ses Çalma Metodu
    protected void PlaySound(AudioClip clip)
    {
        // Audio Source'u Player'dan al
        AudioSource playerAudioSource = GetComponentInParent<AudioSource>();

        if (playerAudioSource != null && clip != null)
        {
            playerAudioSource.PlayOneShot(clip);
        }
    }
    public bool ConsumeDurability()
    {
        if (currentDurability > 0 && IsReady())
        {
            currentDurability--;
            Debug.Log($"[{skillName}] kullanýldý. Kalan Can: {currentDurability}");
            return true;
        }
        return false; // Can kalmadý, kullanýma izin verilmedi.
    }


    private void Awake()
    {
        currentDurability = maxDurability;
        nextFireTime = 0f;
    }
    public bool IsReady()
    {
        bool ready = Time.time >= nextFireTime;

        // Eðer hazýr deðilse, ne kadar kaldýðýný loglayalým.
        if (!ready)
        {
            Debug.LogWarning($"[{skillName}]: Cooldown'da! Kalan süre: {nextFireTime - Time.time:F2}s");
            return false;
        }

        return true;
    }

    // YENÝ: Cooldown'ý baþarýlý kullanýmdan sonra baþlatalým
    public void StartCooldown()
    {
        nextFireTime = Time.time + cooldownTime;
    }
    public virtual void UsePrimary()
    {
        // Boþ býrakýyoruz. Türeten sýnýf dolduracak.
    }

    public virtual void UseSecondary()
    {
        // Boþ. Eðer bir silahýn Q özelliði yoksa burasý çalýþýr (yani hiçbir þey yapmaz).
    }

    
}


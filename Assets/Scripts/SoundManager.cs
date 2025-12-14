using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("SFX Ses Kaynağı")]
    // Ses efektlerini çalmak için kullanılan AudioSource
    public AudioSource sfxSource;

    // Yürüme gibi sürekli çalması gereken sesler için ayrı bir AudioSource
    public AudioSource loopSource;

    [Header("Genel SFX Sesleri")]
    public AudioClip buttonClickSFX;
    public AudioClip slotChangeSFX;
    public AudioClip playerFootstepSFX; // Player yürüme sesi
    public AudioClip playerHitSFX;      // Player hasar yeme sesi

    [Header("Power-Up SFX Sesleri")]
    // Her PowerUpType için sesleri tutan esnek yapı
    public List<PowerUpSoundData> powerUpSounds = new List<PowerUpSoundData>();

    [Header("Düşman SFX Sesleri")]
    public AudioClip enemyFootstepSFX;
    public AudioClip enemyStunSFX;     // Sersemlemeye tepki verme
    public AudioClip enemyAttackSFX;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Sahne değişimlerinde SoundManager'ın yok olmasını engellemek için
            // DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- GENEL ÇALMA METOTLARI ---

    // Tek seferlik sesleri çalar (Mermi atma, tıklama vb.)
    public void PlayOneShot(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    // Döngülü sesleri çalar (Yürüme, motor sesi vb.)
    public void StartLoopingSound(AudioClip clip)
    {
        if (clip != null && loopSource != null && loopSource.clip != clip)
        {
            loopSource.clip = clip;
            loopSource.loop = true;
            loopSource.Play();
        }
    }

    public void StopLoopingSound()
    {
        if (loopSource != null && loopSource.isPlaying)
        {
            loopSource.Stop();
            loopSource.clip = null;
        }
    }

    // --- ÖZEL ÇAĞRI METOTLARI ---

    public void PlayPowerUpSFX(PowerUpType type)
    {
        // Listeden ilgili PowerUp'ın sesini bul
        AudioClip clip = powerUpSounds.Find(data => data.type == type)?.sfxClip;

        PlayOneShot(clip);
    }
}

// PowerUp seslerini Inspector'da kolayca atamak için serileştirilebilir sınıf
[System.Serializable]
public class PowerUpSoundData
{
    public PowerUpType type;
    public AudioClip sfxClip;
}
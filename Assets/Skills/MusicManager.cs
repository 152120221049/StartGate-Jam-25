using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Müzik Klipleri")]
    public AudioClip menuMusic;
    public AudioClip gameMusic;
    public AudioClip gameOverMusic;
    public AudioClip enemyCountdownMusic;

    private AudioSource audioSource;

    private void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();
        // Müzik seslerinin birbiriyle çakışmaması için 3D ayarlarını kapatın (Spatial Blend: 0)
    }

    // GameManager.ShowMainMenu() içinde çağrılacak
    public void PlayEnemyMusic()
    {
        // Müzik zaten çalıyorsa ve aynı müzik değilse değiştir
        if (audioSource.clip != enemyCountdownMusic && enemyCountdownMusic != null)
        {
            // Önceki müziği yumuşakça durdurma mantığı eklenebilir (örneğin Fade-out),
            // ama şimdilik direkt değiştiriyoruz.
            audioSource.clip = enemyCountdownMusic;
            audioSource.loop = true;
            audioSource.Play();
            Debug.Log("Son Düşman Müziği Başladı!");
        }
    }
    public void PlayMenuMusic()
    {
        if (audioSource.clip != menuMusic)
        {
            audioSource.clip = menuMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    // GameManager.OnPlayButton() içinde çağrılacak
    public void PlayGameMusic()
    {
        if (audioSource.clip != gameMusic)
        {
            audioSource.clip = gameMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
    public void PlayGameOverMusic()
    {
        if (audioSource.clip != gameOverMusic)
        {
            audioSource.clip = gameOverMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
    public void StopMusic()
    {
        audioSource.Stop();
    }
}
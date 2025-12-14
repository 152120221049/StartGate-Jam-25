using UnityEngine;

// ARTIK BASEWEAPON DEĞİL, SKILLBASE'DEN TÜREYORUZ!
public class FlamethrowerWeapon : SkillBase
{
    [Header("Alev Ayarları")]
    public GameObject flameColliderObject;
    public ParticleSystem flameParticles;

    private bool isAttacking = false;

    [SerializeField] private PlayerMovement playerMovement;
    private bool durabilityConsumedInCurrentFire = false;
    // Başlangıçta PlayerMovement'ı bul
    private void Start()
    {
        // PlayerMovement'ı bulma
        if (playerMovement == null)
        {
            playerMovement = GetComponentInParent<PlayerMovement>();
            if (playerMovement == null)
                Debug.LogError("FlamethrowerWeapon: PlayerMovement bulunamadı!");
        }

        // ... (Diğer Start kodları) ...
        StopAttack();
    }

    // --- BİRİNCİL KULLANIM (E tuşu, basılı tutulabilir) ---
    public override void UsePrimary()
    {
        // Yönü bulma (PlayerMovement'tan almalısın)
        PlayerMovement playerMovement = GetComponentInParent<PlayerMovement>();

        if (playerMovement != null)
        {
            StartAttack(playerMovement.FacingDirection);
        }
        else
        {
            Debug.LogError("PlayerMovement bulunamadı! Alev silahı yön bulamıyor.");
            // Eğer yön bulunamazsa bile, sadece aç/kapa mantığı için StartAttack'ın yön parametresini kaldırabiliriz.
            // Ama şimdilik yönü bulmaya odaklanalım.
        }
    }

    // Kullanıcı E tuşunu bıraktığında çağrılması gereken yeni bir metot eklememiz gerekiyor.
    // Ancak bu, PlayerSkillManager'da yönetilmelidir (Aşağıda yöneteceğiz).

    // --- Önceki Attack Metodunun Yeni Adı ---
    

    // --- Önceki StopAttack Metodunun Yeni Adı ---
    public void StopAttack()
    {
        if (flameColliderObject.activeSelf)
        {
            flameColliderObject.SetActive(false);
            if (flameParticles != null) flameParticles.Stop();
            isAttacking = false;
        }
    }

    // YENİ MANTIK: Ateş ederken sürekli cephane harca (Her saniye 1 cephane gibi)
    private void Update()
    {
        // Yön takibi ve Cephane Harcama Mantığı
        if (isAttacking && playerMovement != null)
        {
            Vector2 direction = playerMovement.FacingDirection;

            if (direction != Vector2.zero)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
        }
    }

    // StartAttack metodunu basitleştir (Yön artık Update'te güncelleniyor)
    public void StartAttack(Vector2 direction)
    {
        // Artık yön ayarı burada kalıcı olarak yapılmayacak, sadece başlangıçta açılacak.
        if (!flameColliderObject.activeSelf)
        {
            // Start'ta bir kere yönü ayarla (opsiyonel, Update halledecek)
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            flameColliderObject.SetActive(true);
            if (flameParticles != null) flameParticles.Play();
            isAttacking = true;
            durabilityConsumedInCurrentFire = false;
        }
    }
}
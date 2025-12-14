using System;
using UnityEngine;

public class PlayerSkillManager : MonoBehaviour
{
    // Slotlar (Inspector'da görünecek)
    public SkillBase[] skillSlots = new SkillBase[3];
    public int selectedSlotIndex = 0;
    [Header("Sesler")]
    public AudioClip slotChangeSound;
    // SABİT KONTEYNER ADI: Geri iade mekanizması için
    private const string SKILL_CONTAINER_NAME = "Skills";

    // PlayerMovement referansı (Yönü almak için)
    private PlayerMovement playerMovement;

    public static event Action<int> OnSlotSelected; // Yeni slot seçildiğinde
    public static event Action<int> OnSkillEquipped; // Yetenek takıldığında
    public static event Action<int> OnDurabilityChanged; // Dayanıklılık değiştiğinde
    public static event Action<int> OnSkillDiscarded; // Yetenek atıldığında
    void Start()
    {
        // PlayerMovement'ı Start'ta bul
        playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement == null)
        {
            Debug.LogError("PlayerSkillManager: PlayerMovement bileşeni bulunamadı!");
        }

        // Başlangıçta 1. slotu seç
        SelectSlot(0);
    }

    void Update()
    {
        // --- 1. SLOT SEÇİMİ (1, 2, 3 tuşları) ---
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectSlot(2);

        SkillBase currentSkill = skillSlots[selectedSlotIndex];

        if (currentSkill != null)
        {
            // --- 2. E TUŞU (BİRİNCİL KULLANIM) ---
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                // Alev silahı ise özel yol: Sadece cooldown kontrolü yap, harcama yapma
                if (currentSkill is FlamethrowerWeapon flameGun)
                {
                    TryStartFlamethrower(flameGun);
                }
                // Diğer tüm yetenekler (Asit, Portal, Telekinezi, Küçülme):
                else
                {
                    TryUseSkill(currentSkill, false); // false = UsePrimary
                }
            }

            // --- 3. E TUŞU BIRAKILDIĞINDA (ALEV SİLAHI DURDURMA & HARCAMA) ---
            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                if (currentSkill is FlamethrowerWeapon flameGun)
                {
                    TryStopFlamethrower(flameGun);
                }
            }

            // --- 4. Q TUŞU (İKİNCİL KULLANIM) ---
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                // Tüm yetenekler için standart harcama mantığı
                TryUseSkill(currentSkill, true);
            }
        }
    }

    // --- YETENEK KULLANIM METOTLARI ---

    // Standart Yetenekler (Portal, Asit, Küçülme vb.) - Mouse Down'da harcar
    private void TryUseSkill(SkillBase skill, bool isSecondary)
    {
        // 1. Cooldown kontrolü ve Dayanıklılık Tüketimi
        bool canConsume = skill.IsReady() && skill.ConsumeDurability();

        if (canConsume)
        {
            // 2. Kullanım başarılı, yeteneği tetikle
            if (isSecondary)
            {
                skill.UseSecondary();
            }
            else
            {
                skill.UsePrimary();
            }
            OnDurabilityChanged?.Invoke(selectedSlotIndex);
            // 3. Cooldown'ı başlat (ConsumeDurability'de zaten yapılmıyor olabilir)
            skill.StartCooldown();
        }

        // 4. Kullanım sonrası can 0'a düştüyse otomatik discard et
        if (skill.currentDurability <= 0)
        {
            Debug.LogWarning($"{skill.skillName} dayanıklılığı 0 oldu. Otomatik olarak slottan çıkarılıyor!");
            DiscardCurrentSkill();
        }
    }

    // Alev Silahını Başlatma Metodu (Dayanıklılığı harcamadan sadece açar)
    private void TryStartFlamethrower(FlamethrowerWeapon flameGun)
    {
        // Cooldown kontrolü: Ateşi başlatmak için hazırolmalı
        if (!flameGun.IsReady())
        {
            return;
        }

        // Yönü al ve ateşi başlat
        if (playerMovement != null)
        {
            flameGun.StartAttack(playerMovement.FacingDirection);
        }
    }

    // Alev Silahını Durdurma Metodu (Dayanıklılığı burada harcar)
    private void TryStopFlamethrower(FlamethrowerWeapon flameGun)
    {
        // 1. Ateşi durdur
        flameGun.StopAttack();

        // 2. HARCAMA: Cooldown'da değilse ve açık bırakıldıysa (sadece bir kere) harca
        // Not: Flamethrower mühimmatı bittiği için otomatik durduysa burası tekrar harcamaz.
        if (flameGun.IsReady())
        {
            // Dayanıklılık harcanır ve ses çalınır
            flameGun.ConsumeDurability();

            // Kullanım sonrası cooldown'ı başlat (Atış yapıldı)
            flameGun.StartCooldown();
        }

        // 3. Kullanım sonrası can 0'a düştüyse otomatik discard et (Tekrar kontrol)
        if (flameGun.currentDurability <= 0)
        {
            Debug.LogWarning($"{flameGun.skillName} dayanıklılığı 0 oldu. Otomatik olarak slottan çıkarılıyor!");
            DiscardCurrentSkill();
        }
    }

    // --- SLOT YÖNETİMİ METOTLARI ---

    void SelectSlot(int index)
    {
        // Seçili yetenek alev silahı ise slotu değiştirince durdur
        if (skillSlots[selectedSlotIndex] is FlamethrowerWeapon oldFlame)
        {
            oldFlame.StopAttack();
        }

        selectedSlotIndex = index;
        Debug.Log("Seçilen Slot: " + (index + 1));
        PlayUIAudio(slotChangeSound);
        // Olayı tetikle
        OnSlotSelected?.Invoke(selectedSlotIndex);
    }
    private void PlayUIAudio(AudioClip clip)
    {
        AudioSource playerAudioSource = GetComponent<AudioSource>(); // Player üzerindeki AudioSource

        if (playerAudioSource != null && clip != null)
        {
            playerAudioSource.PlayOneShot(clip);
        }
    }
    // ... (Diğer metotlar: FindNextEmptySlot, EquipSkillToCurrentSlot, EquipSkill, DiscardCurrentSkill)

    private int FindNextEmptySlot(int startSlotIndex)
    {
        for (int i = 0; i < skillSlots.Length; i++)
        {
            int checkIndex = (startSlotIndex + i) % skillSlots.Length;

            if (skillSlots[checkIndex] == null)
            {
                return checkIndex; // Boş slot bulundu
            }
        }
        return -1; // Boş slot yok
    }

    public void EquipSkillToCurrentSlot(SkillBase newSkill)
    {
        int targetSlotIndex = selectedSlotIndex;

        if (skillSlots[selectedSlotIndex] != null)
        {
            int emptyIndex = FindNextEmptySlot(selectedSlotIndex);

            if (emptyIndex != -1)
            {
                targetSlotIndex = emptyIndex;
                Debug.LogWarning($"Slot {selectedSlotIndex + 1} dolu. Yetenek otomatik olarak boş olan {emptyIndex + 1}. slota yerleştirildi.");
            }
            else
            {
                // Tüm slotlar doluysa geri iade et
                Debug.LogWarning("Tüm slotlar dolu. Yetenek alınamadı ve yerine geri döndü.");

                GameObject skillObj = newSkill.gameObject;
                GameObject container = GameObject.Find(SKILL_CONTAINER_NAME);

                if (container != null)
                {
                    skillObj.transform.SetParent(container.transform);
                    skillObj.gameObject.SetActive(false);
                }

                return;
            }
        }

        EquipSkill(newSkill, targetSlotIndex);
        selectedSlotIndex = targetSlotIndex;
    }

    public void EquipSkill(SkillBase newSkill, int slotIndex)
    {
        newSkill.transform.SetParent(this.transform);
        newSkill.transform.localPosition = Vector3.zero;
        newSkill.transform.localRotation = Quaternion.identity;
        newSkill.gameObject.SetActive(true);

        skillSlots[slotIndex] = newSkill;

        // Olayı tetikle
        OnSkillEquipped?.Invoke(slotIndex);
        OnSlotSelected?.Invoke(slotIndex);
    }

    public void DiscardCurrentSkill()
    {
        int discardedIndex = selectedSlotIndex;
        if (skillSlots[selectedSlotIndex] == null)
        {
            Debug.Log("Seçili slota takılı yetenek yok.");
            return;
        }

        SkillBase discardedSkill = skillSlots[selectedSlotIndex];
        string skillName = discardedSkill.skillName;

        // YENİ KONTROL: Eğer Flamethrower ise slot bırakılınca da durdur.
        if (discardedSkill is FlamethrowerWeapon flameGun)
        {
            flameGun.StopAttack();
        }

        // 1. Slotu temizle
        skillSlots[selectedSlotIndex] = null;

        // 2. Yetenek objesinin kendisini tamamen YOK ET!
        Destroy(discardedSkill.gameObject);

        Debug.Log($"[{selectedSlotIndex + 1}. SLOTTAN ÇIKARILDI] {skillName} yeteneği tamamen yok edildi.");
        OnSkillDiscarded?.Invoke(discardedIndex);
        // Slot boşaldığı için, seçimi tekrar aynı slota çağır (temizlenmesi için)
        OnSlotSelected?.Invoke(discardedIndex);
    }
}
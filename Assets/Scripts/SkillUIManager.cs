using UnityEngine;

public class SkillUIManager : MonoBehaviour
{
    [Header("Slot Referansları")]
    // Inspector'dan 3 slot objesini (Slot_1, Slot_2, Slot_3) atayın
    public SkillSlotUI[] skillSlotsUI = new SkillSlotUI[3];
    
    private PlayerSkillManager skillManager;
    private bool isInitialized = false; // Tekrar başlatmayı önler

    // Start metodunu kaldırın veya boş bırakın. Artık burada arama yapmayacağız.
    // void Start() { } 

    // YENİ: GameManager'dan çağrılacak başlatma metodu
    public void InitializeUI(PlayerSkillManager manager)
    {
        if (isInitialized) return; // Zaten başlatıldıysa çık

        skillManager = manager;
        if (skillManager == null)
        {
            Debug.LogError("SkillUIManager: PlayerSkillManager referansı geçersiz!");
            return;
        }

        // 1. Olaylara abone ol
        SubscribeToEvents();

        // 2. Başlangıçta tüm slotları sıfırla/güncelle
        for (int i = 0; i < skillSlotsUI.Length; i++)
        {
            UpdateSlotUI(i);
        }

        isInitialized = true;
    }

    // YENİ: Abonelik ve Abonelikten Çıkma metotları
    private void SubscribeToEvents()
    {
        PlayerSkillManager.OnSlotSelected += HandleSlotSelected;
        PlayerSkillManager.OnSkillEquipped += HandleSlotChanged;
        PlayerSkillManager.OnDurabilityChanged += HandleSlotChanged;
        PlayerSkillManager.OnSkillDiscarded += HandleSlotChanged;
    }

    private void UnsubscribeFromEvents()
    {
        PlayerSkillManager.OnSlotSelected -= HandleSlotSelected;
        PlayerSkillManager.OnSkillEquipped -= HandleSlotChanged;
        PlayerSkillManager.OnDurabilityChanged -= HandleSlotChanged;
        PlayerSkillManager.OnSkillDiscarded -= HandleSlotChanged;
    }

    private void OnDisable()
    {
        // Temizlik: Obje pasif hale gelirse abonelikleri iptal et
        if (isInitialized)
        {
            UnsubscribeFromEvents();
            isInitialized = false;
        }
    }

    // --- OLAY YÖNETİCİLERİ ---

    private void HandleSlotChanged(int slotIndex)
    {
        // Yetenek takıldı/atıldı/can azaldı, sadece ilgili slotu güncelle
        UpdateSlotUI(slotIndex);
    }

    private void HandleSlotSelected(int selectedIndex)
    {
        // Slot seçimi değiştiğinde tüm slotları dolaş
        for (int i = 0; i < skillSlotsUI.Length; i++)
        {
            bool isSelected = (i == selectedIndex);

            // İkon, can vb. güncellemesi yapılmaz, sadece çerçeve değişir.
            if (skillSlotsUI[i] != null)
            {
                skillSlotsUI[i].SetSelected(isSelected);
            }
        }
    }

    // --- TEMEL GÜNCELLEME METODU ---
    private void UpdateSlotUI(int index)
    {
        if (index < 0 || index >= skillSlotsUI.Length || skillSlotsUI[index] == null || skillManager == null)
        {
            return;
        }

        SkillBase skill = skillManager.skillSlots[index];
        bool isSelected = (index == skillManager.selectedSlotIndex);

        skillSlotsUI[index].UpdateSlot(skill, isSelected);
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro kullanıyorsanız

public class SkillSlotUI : MonoBehaviour
{
    // Inspector'dan atanacak görsel referanslar
    [Header("Görsel Referanslar")]
    public GameObject selectionIndicator;
    public Image skillIcon;
    public TextMeshProUGUI durabilityText; // Ya da UnityEngine.UI.Text

    public void SetSelected(bool isSelected)
    {
        selectionIndicator.SetActive(isSelected);
    }

    public void UpdateSlot(SkillBase skill, bool isSelected)
    {
        SetSelected(isSelected);

        if (skill == null)
        {
            // Slot boşsa
            skillIcon.enabled = false;
            skillIcon.sprite = null; // Bellek temizliği için sprite'ı da sıfırla
            durabilityText.text = "";
        }
        else
        {
            // Slot doluysa
            skillIcon.enabled = true;

            // KRİTİK ADIM: SkillBase'den gelen Sprite'ı Image bileşenine atama
            if (skill.skillSprite != null)
            {
                skillIcon.sprite = skill.skillSprite;
            }
            else
            {
                // Eğer ikon atanmamışsa, en azından boş bir placeholder göster
                Debug.LogWarning($"Yetenek ({skill.skillName}) için Skill Sprite atanmamış!");
                skillIcon.sprite = null;
            }

            // Dayanıklılık bilgisini güncelle
            durabilityText.text = $"{skill.currentDurability}/{skill.maxDurability}";
        }
    }
}
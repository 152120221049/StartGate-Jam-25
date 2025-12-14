using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public struct ItemIcon
{
    public PowerUpType type;
    public Sprite icon;
}

public class InventoryUI : MonoBehaviour
{
    [Header("Bağlantılar")]
    public PlayerInventory playerInventory;
    public List<Image> slotImages; // Ekrandaki 3 kutucuğun Image bileşenleri
    public Sprite emptySlotSprite; // Boş kutu görseli

    [Header("İkonlar")]
    public List<ItemIcon> iconDatabase; // Hangi PowerUp hangi resim?

    [Header("Görsel Ayarlar")]
    public Color selectedColor = Color.white; // Seçili kutu parlak olsun
    public Color unselectedColor = new Color(1, 1, 1, 0.5f); // Seçili olmayan sönük olsun

    private void Start()
    {
        // PlayerInventory'yi bul (Eğer atanmadıysa)
        if (playerInventory == null)
            playerInventory = FindObjectOfType<PlayerInventory>();

        // Olaylara abone ol (Inventory değişince beni haberdar et)
        playerInventory.OnInventoryChanged += UpdateUI;
        playerInventory.OnSelectionChanged += UpdateSelection;

        // Başlangıçta bir kere çalıştır
        UpdateUI();
        UpdateSelection();
    }

    private void OnDestroy()
    {
        // Abonelikten çık (Hata almamak için önemli)
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged -= UpdateUI;
            playerInventory.OnSelectionChanged -= UpdateSelection;
        }
    }

    // 1. İkonları Çiz
    void UpdateUI()
    {
        List<PowerUpType> items = playerInventory.collectedPowerUps;

        for (int i = 0; i < slotImages.Count; i++)
        {
            if (i < items.Count)
            {
                // Bu slot dolu, ikonunu bul ve koy
                slotImages[i].sprite = GetSpriteForType(items[i]);
                slotImages[i].enabled = true; // Resmi göster
            }
            else
            {
                // Bu slot boş
                if (emptySlotSprite != null)
                    slotImages[i].sprite = emptySlotSprite;
                else
                    slotImages[i].enabled = false; // Boşsa gizle
            }
        }
    }

    // 2. Seçili Olanı Parlat
    void UpdateSelection()
    {
        int currentIndex = playerInventory.GetCurrentIndex();
        int itemCount = playerInventory.collectedPowerUps.Count;

        for (int i = 0; i < slotImages.Count; i++)
        {
            // Eğer bu slot seçiliyse VE içi doluysa
            if (i == currentIndex && i < itemCount)
            {
                // Çerçevesini büyüt, rengini aç vs.
                slotImages[i].color = selectedColor;
                slotImages[i].transform.localScale = Vector3.one * 1.2f; // Biraz büyüt
            }
            else
            {
                // Normal hal
                slotImages[i].color = unselectedColor;
                slotImages[i].transform.localScale = Vector3.one;
            }
        }
    }

    // Yardımcı: Tipe göre resim bul
    Sprite GetSpriteForType(PowerUpType type)
    {
        foreach (var item in iconDatabase)
        {
            if (item.type == type) return item.icon;
        }
        return null;
    }
}
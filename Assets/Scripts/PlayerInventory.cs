using UnityEngine;
using System.Collections.Generic;
using System; // Action kullanmak için gerekli

public class PlayerInventory : MonoBehaviour
{
    [Header("Ayarlar")]
    public int maxSlots = 3;

    [Header("Durum")]
    public List<PowerUpType> collectedPowerUps = new List<PowerUpType>();
    private int currentIndex = 0;

    // UI'ın dinleyeceği "Olaylar" (Events)
    public Action OnInventoryChanged; // Eşya alınınca veya resetlenince tetiklenir
    public Action OnSelectionChanged; // Q'ya basıp eşya değiştirince tetiklenir

    public PowerUpType GetCurrentPowerUp()
    {
        if (collectedPowerUps.Count == 0) return PowerUpType.None;
        return collectedPowerUps[currentIndex];
    }

    public void SwitchItem()
    {
        if (collectedPowerUps.Count == 0) return;

        // Sıradakine geç (Döngüsel)
        currentIndex = (currentIndex + 1) % collectedPowerUps.Count;

        Debug.Log("Seçili Eşya: " + collectedPowerUps[currentIndex]);

        // UI'a haber ver: "Seçim değişti!"
        OnSelectionChanged?.Invoke();
    }

    public void AddItem(PowerUpType newItem)
    {
        if (collectedPowerUps.Count < maxSlots)
        {
            if (!collectedPowerUps.Contains(newItem))
            {
                collectedPowerUps.Add(newItem);

                // Yeni eşya gelince otomatik olarak onu seçelim (İsteğe bağlı)
                currentIndex = collectedPowerUps.Count - 1;

                Debug.Log("Eşya alındı: " + newItem);

                // UI'a haber ver: "Liste değişti!"
                OnInventoryChanged?.Invoke();
                OnSelectionChanged?.Invoke();
            }
        }
    }

    public void ResetInventory()
    {
        collectedPowerUps.Clear();
        currentIndex = 0;

        // UI'a haber ver: "Her şey silindi!"
        OnInventoryChanged?.Invoke();
        OnSelectionChanged?.Invoke();
    }

    // UI scriptinin, hangi index'in seçili olduğunu bilmesi için
    public int GetCurrentIndex()
    {
        return currentIndex;
    }
}
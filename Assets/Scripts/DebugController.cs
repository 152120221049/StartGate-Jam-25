using UnityEngine;

public class DebugController : MonoBehaviour
{
    private PlayerInventory inventory;

    void Update()
    {
        // Envanteri her karede bulmaya çalışma, yoksa bul
        if (inventory == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) inventory = player.GetComponent<PlayerInventory>();
        }

        if (inventory == null) return;

        // --- HİLE TUŞLARI ---

        // F1'e basınca ATEŞ SİLAHI ver
        if (Input.GetKeyDown(KeyCode.F1))
        {
            inventory.AddItem(PowerUpType.FireGun);
            Debug.Log("Hile: Ateş Silahı eklendi.");
        }

        // F2'ye basınca ASİT ver
        if (Input.GetKeyDown(KeyCode.F2))
        {
            inventory.AddItem(PowerUpType.Acid);
            Debug.Log("Hile: Asit eklendi.");
        }

        // F3'e basınca PORTAL ver
        if (Input.GetKeyDown(KeyCode.F3))
        {
            inventory.AddItem(PowerUpType.PortalGun);
            Debug.Log("Hile: Portal eklendi.");
        }

        // F4'e basınca ENVANTERİ TEMİZLE
        if (Input.GetKeyDown(KeyCode.F4))
        {
            inventory.ResetInventory();
            Debug.Log("Hile: Envanter temizlendi.");
        }
    }
}
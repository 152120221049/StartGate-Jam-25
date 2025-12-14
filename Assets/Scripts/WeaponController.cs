using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Referanslar")]
    public PlayerInventory inventory;
    public Transform weaponHolder; // Silahların child olarak durduğu obje

    private BaseWeapon currentWeapon;
    private UsePortal portalGun; // Arkadaşının portal scripti

    private void Start()
    {
        portalGun = GetComponent<UsePortal>();
        // Başlangıçta silahları tara ve kapat
        UpdateCurrentWeapon();
    }

    private void Update()
    {
        // Q tuşu ile envanter değişince silahı güncelle
        if (Input.GetKeyDown(KeyCode.Q))
        {
            inventory.SwitchItem();
            UpdateCurrentWeapon();
        }

        HandleShooting();
    }

    void UpdateCurrentWeapon()
    {
        PowerUpType type = inventory.GetCurrentPowerUp();

        // Önce tüm silahları (child objeleri) kapat
        foreach (Transform child in weaponHolder)
        {
            child.gameObject.SetActive(false);
        }

        // Portal scriptini kapat (sadece portal seçiliyse açacağız)
        if (portalGun != null) portalGun.enabled = false;

        currentWeapon = null;

        // Seçili tipe göre doğru silahı aç
        if (type == PowerUpType.PortalGun)
        {
            if (portalGun != null) portalGun.enabled = true;
        }
        else
        {
            // WeaponHolder altındaki doğru silah scriptini bul ve aç
            foreach (Transform child in weaponHolder)
            {
                BaseWeapon w = child.GetComponent<BaseWeapon>();
                if (w != null && w.weaponType == type)
                {
                    child.gameObject.SetActive(true);
                    currentWeapon = w;
                    break;
                }
            }
        }
    }

    void HandleShooting()
    {
        // Portalın kendi ateş mekaniği var, ona karışmıyoruz.
        if (currentWeapon == null) return;

        // Fare yönünü bul
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - transform.position).normalized;

        // Ateş Etme
        if (Input.GetButton("Fire1")) // Basılı tutma desteği (Flamethrower için)
        {
            currentWeapon.Attack(direction);
        }
        else if (Input.GetButtonUp("Fire1")) // Tuşu bırakınca
        {
            currentWeapon.StopAttack(); // Flamethrower'ı durdur
        }
    }
}
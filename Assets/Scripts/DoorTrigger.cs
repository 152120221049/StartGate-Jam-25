using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    // DoorController2D referansý otomatik atanacak
    [HideInInspector]
    public DoorController targetDoor;

    [Header("Etkileþim Ayarlarý")]
    public float timeRequired = 1.0f; // Ýstenen bekleme süresi (1.0 saniye)
    public string doorObjectName = "Door";

    [Header("Tetikleyici Ayarlarý")]
    public string[] validTags = new string[] { "Player", "Box" };

    private float currentHoldTime = 0f;
    private int objectCount = 0;

    void Awake()
    {
        // Otomatik kapý bulma mantýðý (Ebeveyn objesi üzerinden)
        Transform parentTransform = transform.parent;

        if (parentTransform != null)
        {
            Transform doorTransform = parentTransform.Find(doorObjectName);
            if (doorTransform != null)
            {
                targetDoor = doorTransform.GetComponent<DoorController>();
            }
        }

        if (targetDoor == null)
        {
            Debug.LogError($"HATA: '{doorObjectName}' objesi veya 'DoorController2D' bileþeni bulunamadý. Hiyerarþiyi kontrol edin.");
        }
    }

    void Update()
    {
        if (targetDoor == null) return;

        if (objectCount > 0)
        {
            currentHoldTime += Time.deltaTime;

#if UNITY_EDITOR
            Debug.Log($"Alanýn içinde bekleme süresi: {currentHoldTime:F2} / {timeRequired:F2} sn");
#endif

            // Yeterli süre beklendiyse, kapýyý aç
            if (currentHoldTime >= timeRequired)
            {
                targetDoor.OpenDoor();
            }
        }
        else // objectCount == 0 (Alanýn içinde geçerli obje yok)
        {
            // Süreyi sýfýrla
            currentHoldTime = 0f;

            // Alan boþ olduðu sürece kapýyý kapatma komutu gönder
            // DoorController2D, zaten kapalýysa hareketi durduracaktýr.
            targetDoor.CloseDoor();
        }
    }

    // Nesne tetikleyici alana girdiðinde
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsTagValid(other.tag))
        {
            objectCount++;
        }
    }

    // Nesne tetikleyici alandan çýktýðýnda
    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsTagValid(other.tag))
        {
            if (objectCount > 0)
            {
                objectCount--;
            }
            // Alan boþaldýðýnda, Update döngüsü otomatik olarak CloseDoor() çaðýracaktýr.
        }
    }

    private bool IsTagValid(string tagToCheck)
    {
        foreach (string tag in validTags)
        {
            if (tag == tagToCheck)
            {
                return true;
            }
        }
        return false;
    }
}
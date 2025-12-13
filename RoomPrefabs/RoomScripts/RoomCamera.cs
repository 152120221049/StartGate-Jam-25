using UnityEngine;

public class RoomCamera : MonoBehaviour
{
    [Header("Hedef")]
    public Transform target; // Karakterin (Player)
    public float smoothSpeed = 5f;

    [Header("Durum")]
    public BoxCollider2D currentRoomBounds; // Şu anki odanın sınırları

    private Camera cam;
    private float camHeight;
    private float camWidth;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        // Eğer target atanmadıysa otomatik bul
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }
    }

    // Odaya her girildiğinde bu çalışacak
    public void MoveToRoom(BoxCollider2D roomCollider)
    {
        currentRoomBounds = roomCollider;
    }

    private void LateUpdate()
    {
        // Hedef veya oda yoksa çalışma
        if (target == null || currentRoomBounds == null) return;

        // 1. Kameranın Yarı Boyutlarını Hesapla (Zoom değişimine karşı her karede)
        camHeight = cam.orthographicSize;
        camWidth = camHeight * cam.aspect;

        // 2. Odanın Sınırlarını Al
        float roomMinX = currentRoomBounds.bounds.min.x;
        float roomMaxX = currentRoomBounds.bounds.max.x;
        float roomMinY = currentRoomBounds.bounds.min.y;
        float roomMaxY = currentRoomBounds.bounds.max.y;

        // 3. Kameranın Gidebileceği Limitleri Hesapla
        // (Oda sınırından, kameranın kendi genişliğini çıkarıyoruz ki dışarı taşmasın)
        float minX = roomMinX + camWidth;
        float maxX = roomMaxX - camWidth;
        float minY = roomMinY + camHeight;
        float maxY = roomMaxY - camHeight;

        // 4. Oyuncunun Pozisyonunu Al
        float targetX = target.position.x;
        float targetY = target.position.y;

        // 5. Eğer oda ekrandan KÜÇÜKSE, ortala. BÜYÜKSE, oyuncuyu takip et (Clamp)

        // Genişlik Kontrolü
        if (camWidth * 2 > (roomMaxX - roomMinX)) // Oda ekrandan darsa
        {
            targetX = currentRoomBounds.bounds.center.x; // Ortala
        }
        else // Oda genişse
        {
            targetX = Mathf.Clamp(targetX, minX, maxX); // Sınırla
        }

        // Yükseklik Kontrolü
        if (camHeight * 2 > (roomMaxY - roomMinY)) // Oda ekrandan kısaysa
        {
            targetY = currentRoomBounds.bounds.center.y; // Ortala
        }
        else // Oda yüksekse
        {
            targetY = Mathf.Clamp(targetY, minY, maxY); // Sınırla
        }

        // 6. Son Pozisyonu Belirle ve Pürüzsüz Git
        Vector3 finalPos = new Vector3(targetX, targetY, -10f);
        transform.position = Vector3.Lerp(transform.position, finalPos, smoothSpeed * Time.deltaTime);
    }
}
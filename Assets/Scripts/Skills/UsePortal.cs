using System.Net; // Bu kütüphane genelde Unity'de gerekmez, ancak kodda tutuldu.
using UnityEngine;
using System.Collections;

public class UsePortal : SkillBase
{
    [Header("Portal Prefablarý")]
    [SerializeField] private GameObject bluePortalPrefab;
    [SerializeField] private GameObject orangePortalPrefab;

    [Header("Referanslar")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private Transform firePoint;

    [Header("Görsel Efektler")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float laserDuration = 0.1f;

    private GameObject currentBluePortal;
    private GameObject currentOrangePortal;

    private Coroutine laserCoroutine;

    [Header("Ayarlar")]
    [SerializeField] private float rayDistance = 10f;
    [SerializeField] private float rotationOffset = 90f; // Portal döndürme ofseti

    private void Start()
    {
        // 1. Oyuncu Referanslarýný Bul
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (playerMovement == null)
            playerMovement = GetComponentInParent<PlayerMovement>();

        // 2. Line Renderer'ý Bul (LaserEffect objesinin içinden)
        if (player != null && lineRenderer == null)
        {
            // Dikkat: Burada GetComponent<LineRenderer>() ASLA KULLANMIYORUZ.
            // Player'ýn içindeki "LaserEffect" isimli çocuðu arýyoruz.
            Transform laserObj = player.transform.Find("LaserEffect");

            if (laserObj != null)
            {
                Debug.Log("2. LaserEffect Objesi Bulundu!");
                lineRenderer = laserObj.GetComponent<LineRenderer>();

                if (lineRenderer != null)
                {
                    lineRenderer.enabled = false; // Baþlangýçta kapalý
                    Debug.Log("3. LineRenderer Baþarýyla Alýndý ve Kapatýldý.");
                }
                else
                {
                    Debug.LogError("HATA: 'LaserEffect' objesi var ama üzerinde 'Line Renderer' bileþeni YOK!");
                }
            }
            else
            {
                // Eðer bulamazsa hiyerarþiyi kontrol et
                Debug.LogError("HATA: Player'ýn içinde tam olarak 'LaserEffect' adýnda bir obje bulunamadý! Ýsim hatasý olabilir.");
            }
        }
        else if (player == null)
        {
            Debug.LogError("HATA: Sahnede 'Player' tagine sahip bir obje bulunamadý!");
        }

        // 3. FirePoint Transformunu Bulma:
        if (firePoint == null && player != null)
        {
            firePoint = player.transform.Find("FirePoint");
            if (firePoint == null)
            {
                Debug.LogWarning("UYARI: FirePoint referansý atanmadý ve 'Player' objesi içinde 'FirePoint' adýnda bir alt obje bulunamadý.");
            }
        }
    }

    // --- Q TUÞU (Primary) - Mavi Portal Atar ---
    public override void UsePrimary()
    {
        if (!IsReady()) return;
        Debug.Log(">> Portal: Mavi Portal Atýlýyor (Primary) <<");
        TryFirePortal(true); // true = Mavi Portal
        nextFireTime = Time.time + cooldownTime;
    }

    // --- E TUÞU (Secondary) - Turuncu Portal Atar ---
    public override void UseSecondary()
    {
        if (!IsReady()) return;
        Debug.Log(">> Portal: Turuncu Portal Atýlýyor (Secondary) <<");
        TryFirePortal(false); // false = Turuncu Portal
        nextFireTime = Time.time + cooldownTime;
    }

    void TryFirePortal(bool isBlue)
    {
        if (playerMovement == null) return;

        Vector2 rawDirection = playerMovement.FacingDirection;
        Vector2 finalDirection = GetCardinalDirection(rawDirection);

        // 1. Raycast baþlangýç noktasý
        Vector2 origin = (firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position) + finalDirection * 0.1f;

        // 2. Raycast at
        RaycastHit2D hit = Physics2D.Raycast(origin, finalDirection, rayDistance, wallLayer);

        // --- GÖRSEL KISIM: Lazer Çizimi ---
        Color laserColor = isBlue ? Color.blue : new Color(1f, 0.5f, 0f); // Mavi veya Turuncu
        Vector2 endPoint = hit.collider != null ? hit.point : (origin + finalDirection * rayDistance);
        DrawLaser(origin, endPoint, laserColor);

        // Debug Rengi: Mavi ise Mavi, deðilse Turuncu çizgi çek
        Color debugColor = isBlue ? Color.blue : new Color(1f, 0.5f, 0f);
        Debug.DrawRay(origin, finalDirection * rayDistance, debugColor, 0.2f);
        // --- GÖRSEL KISIM SONU ---

        if (hit.collider != null)
        {
            // Portalý çarpma noktasýna yerleþtir
            if (isBlue)
            {
                SpawnBluePortal(hit.point, hit.normal);
            }
            else
            {
                SpawnOrangePortal(hit.point, hit.normal);
            }
        }
    }

    // --- LAZER ÇÝZME FONKSÝYONLARI ---
    void DrawLaser(Vector2 startPos, Vector2 endPos, Color color)
    {
        if (lineRenderer == null) return;

        // Eðer bir önceki lazerden kalan kapatma sayacý varsa durdur
        if (laserCoroutine != null) StopCoroutine(laserCoroutine);

        lineRenderer.enabled = true;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.SetPosition(0, startPos); // Baþlangýç
        lineRenderer.SetPosition(1, endPos);    // Bitiþ

        // Belirlenen süre sonra kapatmak için sayacý baþlat
        laserCoroutine = StartCoroutine(DisableLaserAfterTime());
    }

    IEnumerator DisableLaserAfterTime()
    {
        yield return new WaitForSeconds(laserDuration);
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }


    // --- PORTAL YARATMA VE YÖNETÝM FONKSÝYONLARI ---

    void SpawnBluePortal(Vector2 position, Vector2 normal)
    {
        if (currentBluePortal != null) Destroy(currentBluePortal);
        currentBluePortal = InstantiatePortal(bluePortalPrefab, position, normal);
        UpdatePortalLinks();
    }

    void SpawnOrangePortal(Vector2 position, Vector2 normal)
    {
        if (currentOrangePortal != null) Destroy(currentOrangePortal);
        currentOrangePortal = InstantiatePortal(orangePortalPrefab, position, normal);
        UpdatePortalLinks();
    }

    GameObject InstantiatePortal(GameObject prefab, Vector2 pos, Vector2 normal)
    {
        // Portalý duvardan biraz öne çýkar
        Vector2 spawnPos = pos + (normal * 0.1f);

        // Portalýn yüzey normaline bakacak þekilde döndürülmesi
        // Vector2.up (0,1) yönü ile normal arasýndaki açýyý bulur.
        Quaternion baseRot = Quaternion.FromToRotation(Vector2.up, normal);

        // Ofset eklenerek portalýn dikey durmasý saðlanýr.
        Quaternion finalRot = baseRot * Quaternion.Euler(0, 0, rotationOffset);

        return Instantiate(prefab, spawnPos, finalRot);
    }

    void UpdatePortalLinks()
    {
        if (currentBluePortal != null && currentOrangePortal != null)
        {
            // Not: Portal scriptinin adýnýn 'Portal' olduðundan emin olun
            Portal blueScript = currentBluePortal.GetComponent<Portal>();
            Portal orangeScript = currentOrangePortal.GetComponent<Portal>();

            if (blueScript != null && orangeScript != null)
            {
                blueScript.linkedPortal = orangeScript;
                orangeScript.linkedPortal = blueScript;
            }
        }
    }

    // --- YÖN BULMA FONKSÝYONU ---

    // Yönü ana yönlere (sað, sol, yukarý, aþaðý) yuvarlar.
    Vector2 GetCardinalDirection(Vector2 direction)
    {
        float threshold = 0.1f;

        // Yönü yatay mý dikey mi baskýn olarak belirle
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            // Yatay dominant
            if (Mathf.Abs(direction.x) > threshold)
                return new Vector2(Mathf.Sign(direction.x), 0);
        }
        else
        {
            // Dikey dominant
            if (Mathf.Abs(direction.y) > threshold)
                return new Vector2(0, Mathf.Sign(direction.y));
        }

        // Hiçbir yön belirgin deðilse, son bilinen yönü kullan (daha iyi kontrol hissi için)
        return playerMovement.FacingDirection;
    }
}
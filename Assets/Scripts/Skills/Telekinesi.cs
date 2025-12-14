using UnityEngine;
using System.Collections; // IEnumerator için gerekli

public class Telekinesi : SkillBase
{
    [Header("Referanslar")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Transform firePoint;

    [Header("Görsel Efektler")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float laserDuration = 0.1f;
    private Coroutine laserCoroutine;

    [Header("Ayarlar")]
    public float pushForce = 20f; // Impulse çok güçlüdür, 100 yerine 20-30 dene
    public float pullForce = 20f;
    public float range = 50f;
    public string targetTag;    // "Kutu" vb. yazmayý unutma

    private void Start()
    {
        // 1. PlayerMovement'ý Bul
        if (playerMovement == null)
            playerMovement = GetComponentInParent<PlayerMovement>();

        // Oyuncu objesini tag ile bul (Garanti Yöntem)
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            // 2. FirePoint Transformunu Bulma:
            // FirePoint'in Player objesinin bir alt objesi olduðu varsayýlýyor.
            if (firePoint == null)
            {
                firePoint = player.transform.Find("FirePoint");
            }

            // 3. Line Renderer'ý Bul (Player'ýn içindeki "LaserEffect" objesinden)
            if (lineRenderer == null)
            {
                Transform laserObj = player.transform.Find("LaserEffect");
                if (laserObj != null)
                {
                    lineRenderer = laserObj.GetComponent<LineRenderer>();
                    if (lineRenderer != null)
                    {
                        lineRenderer.enabled = false; // Baþlangýçta kapat
                    }
                    else
                    {
                        Debug.LogError("HATA: 'LaserEffect' objesinde LineRenderer bileþeni bulunamadý!");
                    }
                }
                else
                {
                    Debug.LogError("HATA: Player'ýn içinde 'LaserEffect' adýnda bir obje yok!");
                }
            }
        }
        else
        {
            Debug.LogError("HATA: Sahnede 'Player' tagine sahip bir obje bulunamadý!");
        }

        // FirePoint hala null ise bir uyarý ver
        if (firePoint == null)
        {
            Debug.LogWarning("UYARI: FirePoint referansý atanmadý ve 'Player' objesi içinde 'FirePoint' adýnda bir alt obje bulunamadý.");
        }
    }

    // --- E TUÞU: ÝTME (PUSH) ---
    public override void UsePrimary()
    {
        // Cooldown kontrolü (SkillBase'den gelir)
        if (!IsReady())
        {
            return; // Hazýr deðilse çýkýþ yap.
        }

        Debug.Log(">> Telekinezi: ÝTME (E) <<");
        TryApplyForce(true); // true = Ýtme

        // Cooldown sýfýrla
        nextFireTime = Time.time + cooldownTime;
    }

    // --- Q TUÞU: ÇEKME (PULL) ---
    public override void UseSecondary()
    {
        // Cooldown kontrolü (SkillBase'den gelir)
        if (!IsReady())
        {
            return; // Hazýr deðilse çýkýþ yap.
        }

        Debug.Log(">> Telekinezi: ÇEKME (Q) <<");
        TryApplyForce(false); // false = Çekme

        // Cooldown sýfýrla
        nextFireTime = Time.time + cooldownTime;
    }

    // Telekinezi mantýðýný uygulayan ana fonksiyon
    void TryApplyForce(bool isPushing)
    {
        if (playerMovement == null) return;

        // Yönü al ve ana yönlere (kardinal) yuvarla (sað, sol, yukarý, aþaðý)
        Vector2 rawDirection = playerMovement.FacingDirection;
        Vector2 finalDirection = GetCardinalDirection(rawDirection);

        // Iþýn karakterin biraz önünden çýksýn ki kendi collider'ýna çarpmasýn
        Vector2 origin = (firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position) + finalDirection * 1f;

        // Raycast atýþý yap
        RaycastHit2D hit = Physics2D.Raycast(origin, finalDirection, range);

        // Debug için ýþýný çiz (Ýtme Kýrmýzý, Çekme Yeþil)
        Debug.DrawRay(origin, finalDirection * range, isPushing ? Color.red : Color.green, 0.5f);

        // --- GÖRSEL KISIM: Lazer Rengini Belirle ve Çiz ---
        Color laserColor = isPushing ? Color.red : Color.green;

        // Eðer bir þeye çarptýysa oraya kadar, çarpmadýysa menzil sonuna kadar çiz
        Vector2 endPoint = hit.collider != null ? hit.point : (origin + finalDirection * range);

        DrawLaser(origin, endPoint, laserColor);
        // --------------------------------------------------

        if (hit.collider != null)
        {
            // Tag kontrolü (Eðer targetTag boþ deðilse ve tutmuyorsa dur.)
            if (!string.IsNullOrEmpty(targetTag) && !hit.collider.CompareTag(targetTag))
                return;

            Rigidbody2D boxRb = hit.collider.GetComponent<Rigidbody2D>();

            if (boxRb != null)
            {
                // Ýtme ise yön ayný, çekme ise yön ters (-finalDirection)
                Vector2 forceDirection = isPushing ? finalDirection : -finalDirection;
                float power = isPushing ? pushForce : pullForce;

                // ForceMode2D.Impulse anlýk bir vuruþ/kuvvet hissi verir
                boxRb.AddForce(forceDirection * power, ForceMode2D.Impulse);
            }
        }
    }

    // --- LAZER ÇÝZME FONKSÝYONLARI ---
    void DrawLaser(Vector2 startPos, Vector2 endPos, Color color)
    {
        if (lineRenderer == null) return;

        // Önceki Coroutine varsa durdur
        if (laserCoroutine != null) StopCoroutine(laserCoroutine);

        lineRenderer.enabled = true;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);

        // Lazer Süresi bitince kapatacak Coroutine'i baþlat
        laserCoroutine = StartCoroutine(DisableLaserAfterTime());
    }

    // Belirtilen süre sonunda LineRenderer'ý kapatýr
    IEnumerator DisableLaserAfterTime()
    {
        yield return new WaitForSeconds(laserDuration);
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }

    // Bir vektörü ana yönlere (sað, sol, yukarý, aþaðý) yuvarlar.
    Vector2 GetCardinalDirection(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            // Yatay dominant
            return new Vector2(Mathf.Sign(direction.x), 0);
        }
        else
        {
            // Dikey dominant (veya tam ortada)
            return new Vector2(0, Mathf.Sign(direction.y));
        }
    }
}
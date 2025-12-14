using UnityEngine;

public class CollectFiles : MonoBehaviour
{

    [Header("Raycast Ayarlari")]
    public float rayDistance = 1.5f;
    public LayerMask dosyaLayer;
    private PlayerMovement playerMovement;


    [Header("E Basili Tut Ayarlari")]
    public float holdTime = 0.1f;
    private float holdTimer = 0f;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovement referansi bulunamadi!");
            return;
        }

        Vector2 direction = playerMovement.FacingDirection;
        Vector2 origin = (Vector2)transform.position + direction * 0.2f;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, rayDistance, dosyaLayer);
        Debug.DrawRay(origin, direction * rayDistance, Color.green);

        if (hit.collider != null)
        {
            GameObject parent = hit.collider.gameObject;

            int masaLayer = LayerMask.NameToLayer("Masa");
            int buyukMasaLayer = LayerMask.NameToLayer("BüyükMasa");

            if (masaLayer == -1 || buyukMasaLayer == -1)
            {
                Debug.LogError("Layer isimleri yanlis! 'Masa' veya 'BüyükMasa' kontrol edin.");
                return;
            }

            // Raycast çarptýysa logla
            Debug.Log("Raycast çarptý: " + parent.name + ", Layer: " + LayerMask.LayerToName(parent.layer));

            // Anlik E basma (Masa)
            if (Input.GetKeyDown(KeyCode.E) && parent.layer == masaLayer)
            {
                Debug.Log("Masa toplandi!");
                DestroyChildren(parent);
                holdTimer = 0f;
            }

            // Basili Tutma (BüyükMasa)
            else if (parent.layer == buyukMasaLayer)
            {
                if (Input.GetKey(KeyCode.E))
                {
                    holdTimer += Time.deltaTime;

                    if (holdTimer >= holdTime)
                    {
                        Debug.Log("BüyükMasa toplandi!");
                        DestroyChildren(parent);
                        holdTimer = 0f;
                    }
                }
                else
                {
                    holdTimer = 0f;
                }
            }
            else
            {
                holdTimer = 0f;
            }
        }
        else
        {
            holdTimer = 0f;
            // Debug.Log("Raycast hicbir seye carpmadi. Yön: " + direction);
        }
    }

    private void DestroyChildren(GameObject parent)
    {
        foreach (Transform child in parent.transform)
        {
            if (!child.gameObject.CompareTag("Sabit"))
            {
                Destroy(child.gameObject);
            }
        }
    }
}
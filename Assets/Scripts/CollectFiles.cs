using UnityEngine;
using UnityEngine.UI;

public class CollectFiles : MonoBehaviour
{
    [Header("Raycast Ayarlari")]
    public float rayDistance = 1.5f;
    public LayerMask dosyaLayer;
    private PlayerMovement playerMovement;

    [Header("E Basili Tut Ayarlari")]
    public float holdTime = 0.5f;
    private float holdTimer = 0f;

    [Header("Puan Ayarlari")]
    public int masaScore = 10;
    public int buyukMasaScore = 50;

    [Header("UI İlerleme Ayarlari")]
    public GameObject progressUIParent;
    public Slider progressBarSlider;

    private Vector3 lastHitPosition;

    private int masaLayer;
    private int buyukMasaLayer;
    private const int DEFAULT_LAYER = 0;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();

        masaLayer = LayerMask.NameToLayer("Masa");
        buyukMasaLayer = LayerMask.NameToLayer("BüyükMasa");

        if (masaLayer == -1 || buyukMasaLayer == -1)
        {
            return;
        }

        ShowProgressUI(false);
        UpdateProgressBar(0f);
    }

    void Update()
    {
        if (playerMovement == null || GameManager.Instance == null) return;

        Vector2 direction = playerMovement.FacingDirection;
        Vector2 origin = (Vector2)transform.position + direction * 0.2f;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, rayDistance, dosyaLayer);

        bool isHittingCollectable = (hit.collider != null);

        if (isHittingCollectable)
        {
            GameObject parent = hit.collider.gameObject;
            int hitLayer = parent.layer;

            lastHitPosition = parent.transform.position;

            if (hitLayer == masaLayer)
            {
                ResetHoldTimer();

                if (Input.GetKeyDown(KeyCode.E))
                {
                    CompleteCollection(parent, masaScore);
                }
            }
            else if (hitLayer == buyukMasaLayer)
            {
                if (Input.GetKey(KeyCode.E))
                {
                    ShowProgressUI(true);

                    holdTimer += Time.deltaTime;

                    UpdateProgressBar(Mathf.Clamp01(holdTimer / holdTime));

                    if (holdTimer >= holdTime)
                    {
                        CompleteCollection(parent, buyukMasaScore);
                        ResetHoldTimer();
                    }
                }
                else
                {
                    ResetHoldTimer();
                }
            }
            else
            {
                ResetHoldTimer();
            }
        }
        else
        {
            ResetHoldTimer();
        }

        MoveProgressUI();
    }

    private void CompleteCollection(GameObject parent, int score)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(score);
        }

        DestroyChildren(parent);

        parent.layer = DEFAULT_LAYER;
    }

    private void ResetHoldTimer()
    {
        holdTimer = 0f;
        ShowProgressUI(false);
        UpdateProgressBar(0f);
    }

    private void ShowProgressUI(bool show)
    {
        if (progressUIParent != null)
        {
            if (progressUIParent.activeSelf != show)
            {
                progressUIParent.SetActive(show);
            }
        }
    }

    private void UpdateProgressBar(float fillAmount)
    {
        if (progressBarSlider != null)
        {
            progressBarSlider.value = fillAmount;
        }
    }

    private void MoveProgressUI()
    {
        if (progressUIParent != null && progressUIParent.activeSelf)
        {
            progressUIParent.transform.position = lastHitPosition + new Vector3(0, 1.5f, 0);
        }
    }

    private void DestroyChildren(GameObject parent)
    {
        Transform[] children = parent.transform.GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (child.gameObject != parent && !child.gameObject.CompareTag("Sabit"))
            {
                Destroy(child.gameObject);
            }
        }
    }
}
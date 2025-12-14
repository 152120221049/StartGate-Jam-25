using UnityEditor.ShaderGraph;
using UnityEngine;


public class DoorController : MonoBehaviour
{
    [Header("Kapý Ayarlarý")]
    public Vector3 openPositionOffset = new Vector3(0, 5, 0);
    public float moveSpeed = 3f;

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private Vector3 targetPosition; // Kapýnýn hareket etmesi gereken nihai pozisyon
    private bool isOpened = false;

    // Harici script'lerin kapýnýn durumunu bilmesi için (okunur)
    public bool IsDoorOpen => isOpened;

    void Start()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + openPositionOffset;
        targetPosition = closedPosition; // Baþlangýçta kapalý
    }

    void Update()
    {
        // Kapý hedeflenen pozisyona doðru hareket etmeli
        if (transform.position != targetPosition)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            // Eðer hareket bittiyse
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                // Kapý durumu güncellenir
                isOpened = (targetPosition == openPosition);
            }
        }
    }

    // Tetikleyici (TimedInteraction2D) tarafýndan çaðrýlýr
    public void OpenDoor()
    {
        // Eðer hedef zaten açýk pozisyon deðilse, hedefi açýk yap
        if (targetPosition != openPosition)
        {
            targetPosition = openPosition;
#if UNITY_EDITOR
            Debug.Log("2D Kapý açýlmaya baþladý.");
#endif
        }
    }

    // Tetikleyici (TimedInteraction2D) tarafýndan çaðrýlýr
    public void CloseDoor()
    {
        // Eðer hedef zaten kapalý pozisyon deðilse, hedefi kapalý yap
        if (targetPosition != closedPosition)
        {
            targetPosition = closedPosition;
#if UNITY_EDITOR
            Debug.Log("2D Kapý kapanmaya baþladý.");
#endif
        }
    }
}

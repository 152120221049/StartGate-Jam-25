using UnityEngine;

public enum DoorDirection { Top, Bottom, Left, Right }

public class RoomConnector : MonoBehaviour
{
    public DoorDirection direction;
    public bool isEntrance;

    [Header("Kapı Mekaniği")]
    public GameObject doorBlocker; // Kapanınca devreye girecek duvar/kapı objesi

    private void Awake()
    {
        if(doorBlocker != null)
        {
            doorBlocker.SetActive(false); // Başlangıçta kapı engeli kapalı
        }
    }
    public void CloseDoor()
    {
        if (doorBlocker != null)
        {
            doorBlocker.SetActive(true); // Engeli görünür ve çarpılabilir yap
        }
    }
}
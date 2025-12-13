using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "NewRoom", menuName = "LevelGen/Room Data")]
public class RoomData : ScriptableObject
{
    [Header("Oda Görünümü")]
    public GameObject roomPrefab;

    [Header("Gereksinimler")]
    public List<PowerUpType> requiredPowerUp;

    [Header("Bağlantı Bilgisi")]
    // Bu odanın giriş kapısı hangi yönde? (Filtreleme için kritik)
    public DoorDirection entryDirection; 

    [Header("Ayarlar")]
    public float timeLimit = 45f;
    public int maxCollectibles = 5;
}

// Power-up çeşitlerini burada tanımlıyoruz
public enum PowerUpType
{
    None,           // Hiçbir şey
    FireGun,        // Ateş Silahı (Buz/Ahşap)
    PortalGun,      // Portal Silahı
    Teleport,       // Işınlanma (2 birim ileri)
    SizeGun,        // Büyüme/Küçülme
    Telekinesis,    // Telekinezi (İtme/Çekme)
    Acid            // Asit (Metal eritme/Yavaşlatma)   
}
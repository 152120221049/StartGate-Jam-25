using UnityEngine;

public class GlowBooster : MonoBehaviour
{
    [Header("Parlaklık Ayarı")]
    public Color baseColor = Color.yellow; // Hangi renk olsun?
    [Range(0, 10)] public float intensity = 3f; // Parlaklık gücü

    private ParticleSystem ps;
    private Material mat;

    void Start()
    {
        // 1. Önce Particle System'ı dene
        ps = GetComponent<ParticleSystem>();

        // 2. Yoksa Renderer'daki materyali al
        if (GetComponent<Renderer>() != null)
        {
            mat = GetComponent<Renderer>().material;
        }
    }

    void Update()
    {
        // Rengi ve Parlaklığı Hesapla (HDR Formülü)
        // Renk * (2 üzeri Intensity) formülüyle ışık şiddetini artırıyoruz
        Color finalColor = baseColor * Mathf.LinearToGammaSpace(Mathf.Pow(2, intensity));

        // A) Eğer bu bir Particle System ise:
        if (ps != null)
        {
            var main = ps.main;
            // Particle rengini zorla değiştir
            main.startColor = finalColor;
        }
        // B) Eğer bu bir Sprite veya Mesh ise (Materyal kullanıyorsa):
        else if (mat != null)
        {
            // Shader'ına göre rengi bas
            // Mobile/Particles/Additive shader'ı genelde "_TintColor" kullanır
            if (mat.HasProperty("_TintColor"))
                mat.SetColor("_TintColor", finalColor);
            else if (mat.HasProperty("_Color"))
                mat.SetColor("_Color", finalColor);
        }
    }
}
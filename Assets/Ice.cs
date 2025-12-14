using UnityEngine;
using System.Collections;

public class Ice : MonoBehaviour
{
    [SerializeField] private float meltTime = 1;
    private Transform Transform;
    private bool isMelting = false;
    private void Start()
    {
        Transform = transform;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("FireGun") && !isMelting)
        {
            // Erime sürecini baþlat
            StartCoroutine(Melt());
            isMelting = true;
        }
    }
    private IEnumerator Melt()
    {
        Vector3 initialScale = Transform.localScale;
        Vector3 targetScale = Vector3.zero;
        float elapsedTime = 0f;

        while (elapsedTime < meltTime)
        {
            elapsedTime += Time.deltaTime;

            float t = elapsedTime / meltTime;


            Transform.localScale = Vector3.Lerp(initialScale, targetScale, t);

            yield return null;
        }
        Transform.localScale = targetScale;
        Destroy(gameObject);

    }



}

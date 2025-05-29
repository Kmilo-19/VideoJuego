using UnityEngine;
using System.Collections;

public class TileEffect : MonoBehaviour
{
    public AudioClip destroySound;
    private bool isBeingDestroyed = false;

    public void PlayDestroyEffect()
    {
        if (isBeingDestroyed) return;
        isBeingDestroyed = true;

        // Reproduce el sonido
        AudioSource.PlayClipAtPoint(destroySound, transform.position);

        // Inicia la animación de "pop"
        StartCoroutine(PopEffect());
    }

    private IEnumerator PopEffect()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 1.3f;

        float duration = 0.1f;
        float time = 0;

        while (time < duration)
        {
            float t = time / duration;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            time += Time.deltaTime;
            yield return null;
        }

        // Aseguramos la escala final
        transform.localScale = targetScale;

        // Destruimos el objeto tras la animación
        Destroy(gameObject);
    }
}

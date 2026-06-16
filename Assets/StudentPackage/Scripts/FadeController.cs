using UnityEngine;
using Cysharp.Threading.Tasks;

public class FadeController : MonoBehaviour
{
    public CanvasGroup fadeCanvasGroup;

    public float fadeDuration = 1f;

    private static FadeController instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        fadeCanvasGroup.alpha = 0f;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public async UniTask FadeOut(float duration = -1f)
    {
        if (duration < 0f)
        {
            duration = fadeDuration;
        }

        await FadeAsync(fadeCanvasGroup, 0f, 1f, duration);
    }

    public async UniTask FadeIn(float duration = -1f)
    {
        if (duration < 0f)
        {
            duration = fadeDuration;
        }

        await FadeAsync(fadeCanvasGroup, 1f, 0f, duration);
    }

    private async UniTask FadeAsync(CanvasGroup canvasGroup, float from, float to, float duration)
    {
        canvasGroup.alpha = from;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            canvasGroup.alpha = Mathf.Lerp(from, to, t);
            await UniTask.Yield();
        }

        canvasGroup.alpha = to;
    }
}
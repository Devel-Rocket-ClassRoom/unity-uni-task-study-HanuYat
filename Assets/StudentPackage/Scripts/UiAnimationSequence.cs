using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public class UiAnimationSequence : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public GameObject box;
    public Button startButton;
    public Button resetButton;
    public Button cancelButton;
    public TMP_Text statusText;

    public float fadeDuration = 1f;

    public float moveDuration = 1f;
    public float moveDistance = 500f;

    public float scaleDuration = 1f;
    public float scaleMultiplier = 1.5f;

    public float rotateDuration = 1f;
    public float rotateAngle = 180f;

    public float resetDuration = 0.5f;

    private RectTransform boxRect;
    private Vector2 startAnchoredPos;
    private Vector3 startScale;
    private Quaternion startRotation;
    private bool isRunning;

    private CancellationTokenSource animCts;

    private async void Start()
    {
        if (canvasGroup == null)
            canvasGroup = box.GetComponent<CanvasGroup>();
        boxRect = box.GetComponent<RectTransform>();

        startScale = box.transform.localScale;
        startRotation = box.transform.rotation;

        statusText.text = "준비";
        isRunning = false;

        startButton.onClick.AddListener(OnClickStart);
        resetButton.onClick.AddListener(OnClickReset);
        cancelButton.onClick.AddListener(OnClickCancel);

        // 레이아웃 한 번 돈 뒤에 캐싱
        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
        startAnchoredPos = boxRect.anchoredPosition;
        ResetBoxToStart();
    }

    private void OnDestroy()
    {
        animCts?.Cancel();
        animCts?.Dispose();
    }

    private CancellationToken RestartCts()
    {
        animCts?.Cancel();
        animCts?.Dispose();
        animCts = CancellationTokenSource.CreateLinkedTokenSource(
            this.GetCancellationTokenOnDestroy());
        return animCts.Token;
    }

    private void ResetBoxToStart()
    {
        boxRect.anchoredPosition = startAnchoredPos;
        box.transform.localScale = startScale;
        box.transform.rotation = startRotation;
        canvasGroup.alpha = 0f;
    }

    private void OnClickStart()
    {
        if (isRunning) return;

        statusText.text = "시작";
        RunSequence(RestartCts()).Forget();
        ResetBoxToStart();
    }

    private void OnClickReset()
    {
        if (!isRunning) return;

        ResetWithRestart(RestartCts()).Forget();
    }

    private void OnClickCancel()
    {
        if (!isRunning) return;

        statusText.text = "취소";
        isRunning = false;
        CancelAndFadeOut(RestartCts()).Forget();
    }

    private async UniTask RunSequence(CancellationToken ct)
    {
        isRunning = true;
        try
        {
            statusText.text = "페이드 인...";
            await FadeIn(ct);

            statusText.text = "이동 중...";
            await MoveRight(ct);

            statusText.text = "확대 중...";
            await ScaleUp(ct);

            statusText.text = "회전 중...";
            await Rotate(ct);

            statusText.text = "완료";
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            isRunning = false;
        }
    }

    private async UniTaskVoid ResetWithRestart(CancellationToken ct)
    {
        isRunning = false;
        ResetBoxToStart();
        statusText.text = "리셋 중...";

        bool canceled = await UniTask.Delay(TimeSpan.FromSeconds(resetDuration),
                              cancellationToken: ct).SuppressCancellationThrow();
        if (canceled) return;

        await RunSequence(ct);
    }

    private async UniTaskVoid CancelAndFadeOut(CancellationToken ct)
    {
        try
        {
            await FadeOut(ct);
            ResetBoxToStart();
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async UniTask FadeIn(CancellationToken ct)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, ct);
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
        }
        canvasGroup.alpha = 1f;
    }

    private async UniTask FadeOut(CancellationToken ct)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, ct);
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
        }
        canvasGroup.alpha = 0f;
    }

    private async UniTask MoveRight(CancellationToken ct)
    {
        Vector2 from = boxRect.anchoredPosition;
        Vector2 to = from + Vector2.right * moveDistance;
        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, ct);
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            boxRect.anchoredPosition = Vector2.Lerp(from, to, t);
        }
        boxRect.anchoredPosition = to;
    }

    private async UniTask ScaleUp(CancellationToken ct)
    {
        Vector3 from = box.transform.localScale;
        Vector3 to = startScale * scaleMultiplier;
        float elapsed = 0f;
        while (elapsed < scaleDuration)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, ct);
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / scaleDuration);
            box.transform.localScale = Vector3.Lerp(from, to, t);
        }
        box.transform.localScale = to;
    }

    private async UniTask Rotate(CancellationToken ct)
    {
        Quaternion from = box.transform.rotation;
        Quaternion to = from * Quaternion.Euler(0f, 0f, rotateAngle);
        float elapsed = 0f;
        while (elapsed < rotateDuration)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, ct);
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / rotateDuration);
            box.transform.rotation = Quaternion.Slerp(from, to, t);
        }
        box.transform.rotation = to;
    }
}
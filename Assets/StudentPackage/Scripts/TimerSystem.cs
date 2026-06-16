using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using Cysharp.Threading.Tasks;

public class TimerSystem : MonoBehaviour
{
    public TMP_Text timerText;
    public TMP_Text statusText;
    public Button startButton;
    public Button stopButton;
    public Button resumeButton;
    public Button resetButton;

    public float startTime = 10f;
    private float currentTime = 0f;
    private bool isRunning = false;

    private CancellationTokenSource timerCts;

    private void Start()
    {
        currentTime = startTime;
        UpdateTimerText();
        statusText.text = "준비";
        isRunning = false;

        startButton.onClick.AddListener(OnClickStart);
        stopButton.onClick.AddListener(OnClickStop);
        resumeButton.onClick.AddListener(OnClickResume);
        resetButton.onClick.AddListener(OnClickReset);
    }

    private void OnDestroy()
    {
        timerCts?.Cancel();
        timerCts?.Dispose();
    }

    private CancellationToken RestartCts()
    {
        timerCts?.Cancel();
        timerCts?.Dispose();
        timerCts = CancellationTokenSource.CreateLinkedTokenSource(
            this.GetCancellationTokenOnDestroy());
        return timerCts.Token;
    }

    private async UniTaskVoid TimerLoop(CancellationToken ct)
    {
        isRunning = true;
        statusText.text = "진행 중";

        while (currentTime > 0f)
        {
            bool canceled = await UniTask.Delay(TimeSpan.FromSeconds(1),
                                  cancellationToken: ct).SuppressCancellationThrow();
            if (canceled) return;

            currentTime -= 1f;
            if (currentTime < 0f) currentTime = 0f;
            UpdateTimerText();
        }

        isRunning = false;
        statusText.text = "완료";
    }

    private void OnClickStart()
    {
        currentTime = startTime;
        UpdateTimerText();
        TimerLoop(RestartCts()).Forget();
    }

    private void OnClickStop()
    {
        if (!isRunning) return;

        timerCts?.Cancel();
        isRunning = false;
        statusText.text = "정지";
    }

    private void OnClickResume()
    {
        if (isRunning) return;
        if (currentTime <= 0f) return;

        TimerLoop(RestartCts()).Forget();
    }

    private void OnClickReset()
    {
        timerCts?.Cancel();
        isRunning = false;
        currentTime = startTime;
        UpdateTimerText();
        statusText.text = "준비";
    }

    private void UpdateTimerText()
    {
        int totalSeconds = Mathf.CeilToInt(currentTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}
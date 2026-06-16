using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading;
using Cysharp.Threading.Tasks;

public class AutoSaveController : MonoBehaviour
{
    public TMP_InputField inputField;
    public Button manualSaveButton;
    public TMP_Text lastSaveText;
    public TMP_Text statusText;
    public Toggle autoSaveToggle;

    private CancellationTokenSource debounceCts;
    private bool isSaving;
    private string saveData = string.Empty;

    public float saveDuration = 2f;
    public float autoSaveInterval = 10f;
    public float debounceInterval = 3f;

    private void Start()
    {
        manualSaveButton.onClick.AddListener(OnClickManualSave);
        inputField.onValueChanged.AddListener(OnInputChanged);
        autoSaveToggle.isOn = true;
        autoSaveToggle.onValueChanged.AddListener(OnAutoSaveToggleChanged);

        AutoSaveLoop().Forget();

        statusText.text = "준비";
        lastSaveText.text = string.Empty;
    }

    private void OnDestroy()
    {
        debounceCts?.Cancel();
        debounceCts?.Dispose();
    }

    private async UniTaskVoid AutoSaveLoop()
    {
        var ct = this.GetCancellationTokenOnDestroy();
        try
        {
            while (true)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(autoSaveInterval), cancellationToken: ct);
                if (autoSaveToggle.isOn)
                {
                    await SaveData("자동 저장");
                }
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Auto Save Loop Canceled!");
        }
    }

    private async UniTask SaveData(string message)
    {
        if (isSaving)
        {
            statusText.text = "저장 중... 건너뜀";
            return;
        }

        isSaving = true;
        try
        {
            statusText.text = $"{message}: 저장 중...";
            await UniTask.Delay(TimeSpan.FromSeconds(saveDuration));
            saveData = inputField.text;
            statusText.text = $"{message}: 저장 완료!";
            lastSaveText.text = $"마지막 저장 시간: {DateTime.Now:HH:mm:ss}";
        }
        catch (Exception)
        {
            statusText.text = "저장 실패!";
        }
        finally
        {
            isSaving = false;
        }
    }

    private void OnClickManualSave()
    {
        SaveData("수동 저장").Forget();
    }

    private void OnAutoSaveToggleChanged(bool value)
    {
        statusText.text = value ? "자동 저장 활성화" : "자동 저장 비활성화";
    }

    private void OnInputChanged(string text)
    {
        debounceCts?.Cancel();
        debounceCts?.Dispose();
        debounceCts = new CancellationTokenSource();

        DebounceAndSave(debounceCts.Token).Forget();
    }

    private async UniTaskVoid DebounceAndSave(CancellationToken ct)
    {
        try
        {
            statusText.text = "추가 입력 대기 중...";
            await UniTask.Delay(TimeSpan.FromSeconds(debounceInterval), cancellationToken: ct);
            await SaveData("디바운스 저장");
        }
        catch (OperationCanceledException)
        {
        }
    }
}
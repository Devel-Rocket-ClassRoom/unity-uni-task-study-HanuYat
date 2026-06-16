using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

public class SceneLoadController : MonoBehaviour
{
    public Button loadSceneButton;
    public Slider progressBar;
    public TMP_Text progressText;

    public FadeController fadeController;

    public string targetSceneName;
    public float minimumLoadTime = 2f;
    private bool isLoading = false;

    private void Start()
    {
        loadSceneButton.onClick.AddListener(OnLoadSceneClicked);
        fadeController = FindFirstObjectByType<FadeController>();
        gameObject.SetActive(false);
    }

    private void OnLoadSceneClicked()
    {
        LoadSceneWithFadeAsync().Forget();
    }

    private async UniTaskVoid LoadSceneWithFadeAsync()
    {
        progressBar.value = 0f;
        progressText.text = $"로딩 중... {0}%";

        await fadeController.FadeOut();

        isLoading = true;
        gameObject.SetActive(true);
        loadSceneButton.interactable = false;

        await LoadSceneAsync();

        await fadeController.FadeIn();

        isLoading = false;
        // gameObject.SetActive(false);
        // loadSceneButton.interactable = true;
    }

    private async UniTask LoadSceneAsync()
    {
        var progress = Progress.Create<float>(p =>
        {
            progressBar.value = p;
            progressText.text = $"로딩 중... {Mathf.RoundToInt(p * 100)}%";
            Debug.Log($"로딩 중... {Mathf.RoundToInt(p * 100)}%");
        });

        var asyncOp = SceneManager.LoadSceneAsync(targetSceneName);
        asyncOp.allowSceneActivation = false;

        float startTime = Time.time;
        while (asyncOp.progress < 0.9f)
        {
            progress.Report(Mathf.Clamp01(asyncOp.progress / 0.9f));
            await UniTask.Yield();
        }

        float elapsed = Time.time - startTime;
        if (elapsed < minimumLoadTime)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(minimumLoadTime - elapsed));
        }

        progress.Report(1f);
        asyncOp.allowSceneActivation = true;
        await asyncOp.ToUniTask();
    }
}
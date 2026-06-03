// 로딩 씬에서 Loading15 표시와 다음 씬 비동기 전환을 관리하는 스크립트
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenManager : MonoBehaviour
{
    public static string SceneToLoad = "Logo_Scene";

    [Header("Legacy UI")]
    public GameObject[] legacyUiObjects;

    [Header("Loading UI")]
    public GameObject loadingPrefab;
    public RectTransform loadingParent;
    public Vector2 loadingAnchoredPosition = new Vector2(0f, 90f);
    public Vector2 loadingSize = new Vector2(60f, 60f);
    public float loadingShowDelay = 0.15f;
    public float minimumLoadingTime = 0.5f;

    private GameObject loadingInstance;

    private void Start()
    {
        HideLegacyUi();
        StartCoroutine(ShowLoadingThenLoad());
    }

    private IEnumerator ShowLoadingThenLoad()
    {
        if (loadingShowDelay > 0f)
            yield return new WaitForSeconds(loadingShowDelay);

        EnsureLoadingInstance();
        yield return StartCoroutine(LoadSceneAsync());
    }

    private IEnumerator LoadSceneAsync()
    {
        float loadingStartedAt = Time.time;
        AsyncOperation operation = SceneManager.LoadSceneAsync(SceneToLoad);

        if (operation == null)
            yield break;

        operation.allowSceneActivation = false;

        while (operation.progress < 0.9f)
        {
            yield return null;
        }

        float elapsed = Time.time - loadingStartedAt;
        if (elapsed < minimumLoadingTime)
            yield return new WaitForSeconds(minimumLoadingTime - elapsed);

        operation.allowSceneActivation = true;
    }

    private void HideLegacyUi()
    {
        foreach (GameObject uiObject in legacyUiObjects)
        {
            if (uiObject != null)
                uiObject.SetActive(false);
        }
    }

    private void EnsureLoadingInstance()
    {
        if (loadingInstance != null || loadingPrefab == null)
            return;

        Transform parent = loadingParent != null ? loadingParent : transform;
        loadingInstance = Instantiate(loadingPrefab, parent);
        loadingInstance.SetActive(true);

        if (loadingInstance.transform is RectTransform rectTransform)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0f);
            rectTransform.anchorMax = new Vector2(0.5f, 0f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = loadingAnchoredPosition;
            rectTransform.sizeDelta = loadingSize;
            rectTransform.localScale = Vector3.one;
        }
    }
}

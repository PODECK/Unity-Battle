// 로고 페이드 후 로딩 UI를 보여주고 다음 씬을 비동기로 여는 스크립트
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LogoFade : MonoBehaviour
{
    [Header("Logo")]
    public TextMeshProUGUI logoText;
    public GameObject loadingIndicator;
    public GameObject loadingPrefab;
    public RectTransform loadingParent;
    public Vector2 loadingAnchoredPosition = new Vector2(0f, 90f);
    public Vector2 loadingSize = new Vector2(100f, 100f);
    public bool overrideLoadingColor;
    public Color loadingColor = Color.white;

    [Header("Timing")]
    public float startDelay = 1f;
    public float fadeInDuration = 2f;
    public float holdDuration = 2f;
    public float fadeOutDuration = 1f;
    public float minimumLoadingTime = 0.5f;
    public string nextSceneName = "main_map";

    private GameObject loadingInstance;

    private void Start()
    {
        EnsureLoadingInstance();
        SetAlpha(0f);
        SetLoadingVisible(false);
        StartCoroutine(FadeSequence());
    }

    private IEnumerator FadeSequence()
    {
        yield return new WaitForSeconds(startDelay);
        yield return StartCoroutine(Fade(0f, 1f, fadeInDuration));
        yield return new WaitForSeconds(holdDuration);
        yield return StartCoroutine(Fade(1f, 0f, fadeOutDuration));

        if (logoText != null)
            logoText.gameObject.SetActive(false);

        SetLoadingVisible(true);
        yield return StartCoroutine(LoadNextScene());
    }

    private IEnumerator LoadNextScene()
    {
        float loadingStartedAt = Time.time;
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(nextSceneName);

        if (loadOperation == null)
            yield break;

        loadOperation.allowSceneActivation = false;

        while (loadOperation.progress < 0.9f)
        {
            yield return null;
        }

        float elapsed = Time.time - loadingStartedAt;
        if (elapsed < minimumLoadingTime)
            yield return new WaitForSeconds(minimumLoadingTime - elapsed);

        loadOperation.allowSceneActivation = true;
    }

    private IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(Mathf.Lerp(startAlpha, endAlpha, elapsed / duration));
            yield return null;
        }

        SetAlpha(endAlpha);
    }

    private void SetAlpha(float alpha)
    {
        if (logoText == null)
            return;

        Color color = logoText.color;
        color.a = alpha;
        logoText.color = color;
    }

    private void SetLoadingVisible(bool isVisible)
    {
        EnsureLoadingInstance();

        GameObject target = loadingInstance != null ? loadingInstance : loadingIndicator;
        if (target != null)
            target.SetActive(isVisible);
    }

    private void EnsureLoadingInstance()
    {
        if (loadingInstance != null || loadingIndicator != null || loadingPrefab == null)
            return;

        Transform parent = loadingParent != null ? loadingParent : transform;
        loadingInstance = Instantiate(loadingPrefab, parent);

        if (loadingInstance.transform is RectTransform rectTransform)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0f);
            rectTransform.anchorMax = new Vector2(0.5f, 0f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = loadingAnchoredPosition;
            rectTransform.sizeDelta = loadingSize;
            rectTransform.localScale = Vector3.one;
        }

        ApplyLoadingColor(loadingInstance);
    }

    private void ApplyLoadingColor(GameObject target)
    {
        if (!overrideLoadingColor || target == null)
            return;

        Graphic[] graphics = target.GetComponentsInChildren<Graphic>(true);
        foreach (Graphic graphic in graphics)
        {
            graphic.color = loadingColor;
        }
    }
}

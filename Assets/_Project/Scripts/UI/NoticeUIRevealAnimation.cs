// NoticeUI의 장식선과 안내 문구 등장 연출을 제어하는 컴포넌트
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NoticeUIRevealAnimation : MonoBehaviour
{
    [SerializeField] private Graphic diamond;
    [SerializeField] private Graphic leftLine;
    [SerializeField] private Graphic rightLine;
    [SerializeField] private TextMeshProUGUI mainText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float diamondFadeDuration = 0.25f;
    [SerializeField] private float lineExtendDuration = 0.45f;
    [SerializeField] private float textFadeDuration = 0.35f;
    [SerializeField] private float stepDelay = 0.08f;

    private Coroutine revealCoroutine;
    private Color diamondColor;
    private Color leftLineColor;
    private Color rightLineColor;
    private Color textColor;

    private void Awake()
    {
        EnsureCanvasGroup();
        CacheColors();
    }

    private void OnEnable()
    {
        EnsureCanvasGroup();
        canvasGroup.alpha = 1f;
        CacheColors();
        ResetVisuals();

        if (revealCoroutine != null)
        {
            StopCoroutine(revealCoroutine);
        }

        revealCoroutine = StartCoroutine(RevealRoutine());
    }

    private void OnDisable()
    {
        if (revealCoroutine != null)
        {
            StopCoroutine(revealCoroutine);
            revealCoroutine = null;
        }
    }

    public void FadeOutAndDestroy(float delay, float duration)
    {
        StartCoroutine(FadeOutAndDestroyRoutine(delay, duration));
    }

    private IEnumerator RevealRoutine()
    {
        yield return FadeGraphic(diamond, diamondColor, 0f, 1f, diamondFadeDuration);
        yield return new WaitForSeconds(stepDelay);
        yield return ExtendLines();
        yield return new WaitForSeconds(stepDelay);
        yield return FadeText(0f, 1f, textFadeDuration);
        revealCoroutine = null;
    }

    private IEnumerator ExtendLines()
    {
        float elapsed = 0f;

        while (elapsed < lineExtendDuration)
        {
            elapsed += Time.deltaTime;
            float t = EaseOutCubic(Mathf.Clamp01(elapsed / lineExtendDuration));
            SetLineProgress(t);
            yield return null;
        }

        SetLineProgress(1f);
    }

    private IEnumerator FadeGraphic(Graphic graphic, Color baseColor, float from, float to, float duration)
    {
        if (graphic == null)
        {
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = EaseOutCubic(Mathf.Clamp01(elapsed / duration));
            SetGraphicAlpha(graphic, baseColor, Mathf.Lerp(from, to, t));
            yield return null;
        }

        SetGraphicAlpha(graphic, baseColor, to);
    }

    private IEnumerator FadeText(float from, float to, float duration)
    {
        if (mainText == null)
        {
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = EaseOutCubic(Mathf.Clamp01(elapsed / duration));
            SetGraphicAlpha(mainText, textColor, Mathf.Lerp(from, to, t));
            yield return null;
        }

        SetGraphicAlpha(mainText, textColor, to);
    }

    private IEnumerator FadeOutAndDestroyRoutine(float delay, float duration)
    {
        yield return new WaitForSeconds(delay);

        if (revealCoroutine != null)
        {
            StopCoroutine(revealCoroutine);
            revealCoroutine = null;
        }

        EnsureCanvasGroup();
        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = EaseOutCubic(Mathf.Clamp01(elapsed / duration));
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        Destroy(gameObject);
    }

    private void ResetVisuals()
    {
        SetGraphicAlpha(diamond, diamondColor, 0f);
        SetGraphicAlpha(leftLine, leftLineColor, 1f);
        SetGraphicAlpha(rightLine, rightLineColor, 1f);
        SetGraphicAlpha(mainText, textColor, 0f);
        SetLineProgress(0f);
    }

    private void SetLineProgress(float progress)
    {
        SetLineScale(leftLine, progress);
        SetLineScale(rightLine, progress);
    }

    private static void SetLineScale(Graphic line, float progress)
    {
        if (line == null)
        {
            return;
        }

        Vector3 scale = line.rectTransform.localScale;
        scale.x = progress;
        line.rectTransform.localScale = scale;
    }

    private void CacheColors()
    {
        if (diamond != null) diamondColor = diamond.color;
        if (leftLine != null) leftLineColor = leftLine.color;
        if (rightLine != null) rightLineColor = rightLine.color;
        if (mainText != null) textColor = mainText.color;
    }

    private void EnsureCanvasGroup()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    private static void SetGraphicAlpha(Graphic graphic, Color baseColor, float alpha)
    {
        if (graphic == null)
        {
            return;
        }

        baseColor.a = alpha;
        graphic.color = baseColor;
    }

    private static float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }
}

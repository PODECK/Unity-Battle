using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;

public class LoadingTrigger : MonoBehaviour
{
    public string targetObjectName = "TSI_Stone_Block_20B (7)";
    public string nextSceneName = "Logo_Scene";
    public string loadingSceneName = "LoadingScene";
    public float interactionDistance = 3.0f;
    public GameObject interactionPrompt;
    public float uiScale = 0.45f;
    public float irisCloseDuration = 0.9f;
    public float blackHoldDuration = 0.2f;

    private Transform targetTransform;
    private InputAction interactAction;
    private bool isPlayerNearby = false;
    private bool isTransitioning = false;
    private SpriteRenderer[] promptSprites;
    private float currentPromptRise = 0f;
    private Coroutine promptFadeCoroutine;
    private RawImage irisImage;
    private Texture2D irisTexture;
    private Color32[] irisPixels;
    private const int IrisTextureSize = 256;

    void Start()
    {
        interactAction = InputSystem.actions.FindAction("Player/Interact");
        GameObject target = GameObject.Find(targetObjectName);
        if (target != null) targetTransform = target.transform;

        if (interactionPrompt != null)
        {
            promptSprites = interactionPrompt.GetComponentsInChildren<SpriteRenderer>(true);
            interactionPrompt.transform.localScale = Vector3.one * uiScale;
            SetPromptAlpha(0f);
            interactionPrompt.SetActive(true);
        }
    }

    void OnEnable() { if (interactAction != null) interactAction.Enable(); }
    void OnDisable() { if (interactAction != null) interactAction.Disable(); }

    void Update()
    {
        if (targetTransform == null) return;

        float dist = Vector3.Distance(transform.position, targetTransform.position);
        bool nearby = dist <= interactionDistance;

        if (nearby != isPlayerNearby)
        {
            isPlayerNearby = nearby;
            UpdatePromptVisibility();
        }

        if (!isTransitioning && isPlayerNearby && interactAction != null && interactAction.WasPressedThisFrame())
        {
            StartCoroutine(LoadWithIrisClose());
        }

        if (interactionPrompt != null && currentPromptRise > 0.01f && Camera.main != null)
        {
            interactionPrompt.transform.rotation = Camera.main.transform.rotation;
            float riseOffset = (1f - currentPromptRise) * -0.4f;
            // Clear height above player's head (2.8f instead of 2.2f)
            interactionPrompt.transform.position = transform.position + Vector3.up * 2.8f + Vector3.up * riseOffset;
        }
    }

    void UpdatePromptVisibility()
    {
        if (interactionPrompt == null) return;
        StartPromptFade(isPlayerNearby ? 1f : 0f);
    }

    void StartPromptFade(float target)
    {
        if (promptFadeCoroutine != null) StopCoroutine(promptFadeCoroutine);
        promptFadeCoroutine = StartCoroutine(FadePromptRoutine(target));
    }

    IEnumerator FadePromptRoutine(float target)
    {
        float startRise = currentPromptRise;
        float elapsed = 0f;
        float duration = 0.5f; // Slower motion (0.3f -> 0.5f)

        if (target > 0.5f) SetPromptAlpha(1f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            float bounceT;
            if (target > 0.5f) // Appearing
            {
                float s = 2.5f;
                float it = t - 1;
                bounceT = (it * it * ((s + 1) * it + s) + 1);
            }
            else // Disappearing
            {
                float s = 2.5f;
                bounceT = t * t * ((s + 1) * t - s);
            }
            
            currentPromptRise = Mathf.Lerp(startRise, target, bounceT);
            yield return null;
        }
        currentPromptRise = target;
        if (target < 0.5f) SetPromptAlpha(0f);
    }

    void SetPromptAlpha(float alpha)
    {
        if (promptSprites == null) return;
        foreach (var sr in promptSprites)
        {
            if (sr == null) continue;
            Color c = sr.color;
            if (sr.gameObject.name == "Prompt_BG") c.a = alpha * 0.6f;
            else c.a = alpha;
            sr.color = c;
        }
    }

    IEnumerator LoadWithIrisClose()
    {
        isTransitioning = true;
        if (interactAction != null) interactAction.Disable();
        StartPromptFade(0f);
        EnsureIrisOverlay();

        float elapsed = 0f;
        while (elapsed < irisCloseDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / irisCloseDuration);
            float easedT = t * t * (3f - 2f * t);
            DrawIris(1f - easedT);
            yield return null;
        }

        DrawIris(0f);
        yield return new WaitForSeconds(blackHoldDuration);

        LoadingScreenManager.SceneToLoad = nextSceneName;
        SceneManager.LoadScene(loadingSceneName);
    }

    void EnsureIrisOverlay()
    {
        if (irisImage != null)
            return;

        GameObject canvasObject = new GameObject("IrisCloseCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5000;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(960f, 600f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject imageObject = new GameObject("IrisCloseOverlay", typeof(RectTransform), typeof(RawImage));
        imageObject.transform.SetParent(canvasObject.transform, false);

        RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        irisTexture = new Texture2D(IrisTextureSize, IrisTextureSize, TextureFormat.RGBA32, false);
        irisTexture.wrapMode = TextureWrapMode.Clamp;
        irisTexture.filterMode = FilterMode.Bilinear;
        irisPixels = new Color32[IrisTextureSize * IrisTextureSize];

        irisImage = imageObject.GetComponent<RawImage>();
        irisImage.texture = irisTexture;
        irisImage.raycastTarget = false;
        DrawIris(1f);
    }

    void DrawIris(float radius)
    {
        if (irisTexture == null || irisPixels == null)
            return;

        if (radius <= 0f)
        {
            for (int i = 0; i < irisPixels.Length; i++)
            {
                irisPixels[i] = new Color32(0, 0, 0, 255);
            }

            irisTexture.SetPixels32(irisPixels);
            irisTexture.Apply(false);
            return;
        }

        float center = (IrisTextureSize - 1) * 0.5f;
        float maxRadius = center * 1.45f;
        float currentRadius = Mathf.Clamp01(radius) * maxRadius;
        float feather = Mathf.Max(2f, IrisTextureSize * 0.035f);

        for (int y = 0; y < IrisTextureSize; y++)
        {
            for (int x = 0; x < IrisTextureSize; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                byte alpha = (byte)(Mathf.Clamp01((distance - currentRadius) / feather) * 255f);
                irisPixels[y * IrisTextureSize + x] = new Color32(0, 0, 0, alpha);
            }
        }

        irisTexture.SetPixels32(irisPixels);
        irisTexture.Apply(false);
    }
}

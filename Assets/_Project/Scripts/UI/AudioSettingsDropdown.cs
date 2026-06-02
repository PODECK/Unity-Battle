// main_map 우측 상단 오디오 설정 드롭다운을 생성하고 볼륨을 적용하는 컨트롤러
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

public class AudioSettingsDropdown : MonoBehaviour
{
    public static float BgmVolume { get; private set; } = 1f;
    public static float SfxVolume { get; private set; } = 1f;

    [Header("Layout")]
    [SerializeField] private Vector2 buttonSize = new Vector2(44f, 44f);
    [SerializeField] private Vector2 panelSize = new Vector2(280f, 150f);
    [SerializeField] private Vector2 screenOffset = new Vector2(-24f, -24f);
    [SerializeField] private float panelTopOffset = 10f;
    [SerializeField] private float panelPadding = 16f;
    [SerializeField] private float rowHeight = 38f;

    [Header("Style")]
    [SerializeField] private Color buttonColor = new Color(0f, 0f, 0f, 0.62f);
    [SerializeField] private Color buttonTextColor = Color.white;
    [SerializeField] private Color panelColor = new Color(0f, 0f, 0f, 0.78f);
    [SerializeField] private Color labelColor = Color.white;
    [SerializeField] private Color sliderBackgroundColor = new Color(1f, 1f, 1f, 0.22f);
    [SerializeField] private Color sliderFillColor = new Color(0.16f, 0.76f, 0.92f, 1f);
    [SerializeField] private Color sliderHandleColor = Color.white;
    [SerializeField] private Vector2 sliderHandleSize = new Vector2(10f, 12f);
    [SerializeField] private int fontSize = 18;

    [Header("Audio")]
    [SerializeField] private float defaultBgmVolume = 0.8f;
    [SerializeField] private float defaultSfxVolume = 0.8f;
    [SerializeField] private string[] bgmNameKeywords = { "bgm", "music", "ambient", "waterfall", "water" };
    [SerializeField] private string[] sfxNameKeywords = { "sfx", "footstep", "chest", "pickup", "enter" };

    [Header("Input")]
    [SerializeField] private bool toggleWithEscape = true;
    [SerializeField] private bool unlockCursorWhileOpen = true;

    private const string BgmPrefsKey = "podeck_bgm_volume";
    private const string SfxPrefsKey = "podeck_sfx_volume";

    private readonly List<SourceState> bgmSources = new List<SourceState>();
    private readonly List<SourceState> sfxSources = new List<SourceState>();

    private RectTransform panel;
    private float bgmVolume;
    private float sfxVolume;

    private void Awake()
    {
        bgmVolume = PlayerPrefs.GetFloat(BgmPrefsKey, defaultBgmVolume);
        sfxVolume = PlayerPrefs.GetFloat(SfxPrefsKey, defaultSfxVolume);
        BgmVolume = bgmVolume;
        SfxVolume = sfxVolume;
    }

    private void Start()
    {
        CacheAudioSources();
        BuildUi();
        ApplyVolumes();
    }

    private void Update()
    {
        if (!toggleWithEscape)
            return;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePanel();
        }
#endif
    }

    private void CacheAudioSources()
    {
        bgmSources.Clear();
        sfxSources.Clear();

        AudioSource[] sources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource source in sources)
        {
            if (source == null)
                continue;

            SourceState state = new SourceState(source, source.volume);
            if (IsBgmSource(source))
                bgmSources.Add(state);
            else
                sfxSources.Add(state);
        }
    }

    private bool IsBgmSource(AudioSource source)
    {
        string sourceName = source.gameObject.name.ToLowerInvariant();

        foreach (string keyword in bgmNameKeywords)
        {
            if (!string.IsNullOrWhiteSpace(keyword) && sourceName.Contains(keyword.ToLowerInvariant()))
                return true;
        }

        foreach (string keyword in sfxNameKeywords)
        {
            if (!string.IsNullOrWhiteSpace(keyword) && sourceName.Contains(keyword.ToLowerInvariant()))
                return false;
        }

        return source.loop && source.playOnAwake;
    }

    private void BuildUi()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            canvas = CreateCanvas();

        EnsureEventSystem();

        RectTransform button = CreatePanel("SettingsButton", canvas.transform, buttonSize, buttonColor);
        button.anchorMin = Vector2.one;
        button.anchorMax = Vector2.one;
        button.pivot = Vector2.one;
        button.anchoredPosition = screenOffset;

        Button buttonComponent = button.gameObject.AddComponent<Button>();
        buttonComponent.onClick.AddListener(TogglePanel);
        CreateText("Icon", button, "⚙", buttonTextColor, 24, TextAlignmentOptions.Center).rectTransform.sizeDelta = buttonSize;

        panel = CreatePanel("SettingsPanel", canvas.transform, panelSize, panelColor);
        panel.anchorMin = Vector2.one;
        panel.anchorMax = Vector2.one;
        panel.pivot = Vector2.one;
        panel.anchoredPosition = new Vector2(screenOffset.x, screenOffset.y - buttonSize.y - panelTopOffset);
        panel.gameObject.SetActive(false);

        CreateSliderRow("BGMRow", "BGM", bgmVolume, 0, OnBgmChanged);
        CreateSliderRow("SFXRow", "SFX", sfxVolume, 1, OnSfxChanged);
    }

    private void EnsureEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() != null)
            return;

#if ENABLE_INPUT_SYSTEM
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
#else
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
#endif
    }

    private Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("AudioSettingsCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 4000;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(960f, 600f);
        scaler.matchWidthOrHeight = 0.5f;

        return canvas;
    }

    private void CreateSliderRow(string name, string label, float value, int rowIndex, UnityEngine.Events.UnityAction<float> onChanged)
    {
        RectTransform row = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        row.SetParent(panel, false);
        row.anchorMin = new Vector2(0f, 1f);
        row.anchorMax = new Vector2(1f, 1f);
        row.pivot = new Vector2(0.5f, 1f);
        row.anchoredPosition = new Vector2(0f, -panelPadding - rowIndex * rowHeight);
        row.sizeDelta = new Vector2(-panelPadding * 2f, rowHeight);

        TextMeshProUGUI labelText = CreateText(label + "Label", row, label, labelColor, fontSize, TextAlignmentOptions.Left);
        labelText.rectTransform.anchorMin = new Vector2(0f, 0f);
        labelText.rectTransform.anchorMax = new Vector2(0f, 1f);
        labelText.rectTransform.pivot = new Vector2(0f, 0.5f);
        labelText.rectTransform.anchoredPosition = Vector2.zero;
        labelText.rectTransform.sizeDelta = new Vector2(64f, 0f);

        Slider slider = CreateSlider(label + "Slider", row, value);
        RectTransform sliderRect = slider.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0f, 0.5f);
        sliderRect.anchorMax = new Vector2(1f, 0.5f);
        sliderRect.pivot = new Vector2(0.5f, 0.5f);
        sliderRect.offsetMin = new Vector2(76f, -11f);
        sliderRect.offsetMax = new Vector2(0f, 11f);
        slider.onValueChanged.AddListener(onChanged);
    }

    private Slider CreateSlider(string name, Transform parent, float value)
    {
        RectTransform root = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Slider)).GetComponent<RectTransform>();
        root.SetParent(parent, false);

        Image rootImage = root.GetComponent<Image>();
        rootImage.color = new Color(0f, 0f, 0f, 0f);

        RectTransform background = CreatePanel("Background", root, Vector2.zero, sliderBackgroundColor);
        background.anchorMin = new Vector2(0f, 0.35f);
        background.anchorMax = new Vector2(1f, 0.65f);
        background.offsetMin = Vector2.zero;
        background.offsetMax = Vector2.zero;

        RectTransform fillArea = new GameObject("Fill Area", typeof(RectTransform)).GetComponent<RectTransform>();
        fillArea.SetParent(root, false);
        fillArea.anchorMin = Vector2.zero;
        fillArea.anchorMax = Vector2.one;
        fillArea.offsetMin = Vector2.zero;
        fillArea.offsetMax = Vector2.zero;

        RectTransform fill = CreatePanel("Fill", fillArea, Vector2.zero, sliderFillColor);
        fill.anchorMin = new Vector2(0f, 0.35f);
        fill.anchorMax = new Vector2(1f, 0.65f);
        fill.offsetMin = Vector2.zero;
        fill.offsetMax = Vector2.zero;

        RectTransform handleArea = new GameObject("Handle Slide Area", typeof(RectTransform)).GetComponent<RectTransform>();
        handleArea.SetParent(root, false);
        handleArea.anchorMin = Vector2.zero;
        handleArea.anchorMax = Vector2.one;
        handleArea.offsetMin = new Vector2(9f, 0f);
        handleArea.offsetMax = new Vector2(-9f, 0f);

        RectTransform handle = CreatePanel("Handle", handleArea, sliderHandleSize, sliderHandleColor);
        handle.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, sliderHandleSize.x);
        handle.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sliderHandleSize.y);

        Slider slider = root.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = Mathf.Clamp01(value);
        slider.fillRect = fill;
        slider.handleRect = handle;
        slider.targetGraphic = handle.GetComponent<Image>();
        slider.direction = Slider.Direction.LeftToRight;
        slider.SetValueWithoutNotify(Mathf.Clamp01(value));

        return slider;
    }

    private RectTransform CreatePanel(string name, Transform parent, Vector2 size, Color color)
    {
        RectTransform rectTransform = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
        rectTransform.SetParent(parent, false);
        rectTransform.sizeDelta = size;

        Image image = rectTransform.GetComponent<Image>();
        image.color = color;

        return rectTransform;
    }

    private TextMeshProUGUI CreateText(string name, Transform parent, string text, Color color, int size, TextAlignmentOptions alignment)
    {
        TextMeshProUGUI textComponent = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        textComponent.transform.SetParent(parent, false);
        textComponent.text = text;
        textComponent.color = color;
        textComponent.fontSize = size;
        textComponent.alignment = alignment;
        textComponent.raycastTarget = false;
        return textComponent;
    }

    private void TogglePanel()
    {
        if (panel != null)
            SetPanelOpen(!panel.gameObject.activeSelf);
    }

    private void SetPanelOpen(bool isOpen)
    {
        if (panel == null)
            return;

        panel.gameObject.SetActive(isOpen);

        if (!unlockCursorWhileOpen)
            return;

        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isOpen;

        StarterAssets.StarterAssetsInputs[] inputHandlers = FindObjectsByType<StarterAssets.StarterAssetsInputs>(FindObjectsSortMode.None);
        foreach (StarterAssets.StarterAssetsInputs inputHandler in inputHandlers)
        {
            inputHandler.cursorLocked = !isOpen;
            inputHandler.cursorInputForLook = !isOpen;
        }
    }

    private void OnBgmChanged(float value)
    {
        bgmVolume = value;
        BgmVolume = bgmVolume;
        PlayerPrefs.SetFloat(BgmPrefsKey, bgmVolume);
        ApplyVolumes();
    }

    private void OnSfxChanged(float value)
    {
        sfxVolume = value;
        SfxVolume = sfxVolume;
        PlayerPrefs.SetFloat(SfxPrefsKey, sfxVolume);
        ApplyVolumes();
    }

    private void ApplyVolumes()
    {
        ApplyVolumeGroup(bgmSources, bgmVolume);
        ApplyVolumeGroup(sfxSources, sfxVolume);
    }

    private void ApplyVolumeGroup(List<SourceState> sources, float groupVolume)
    {
        foreach (SourceState state in sources)
        {
            if (state.Source != null)
                state.Source.volume = state.BaseVolume * groupVolume;
        }
    }

    private readonly struct SourceState
    {
        public readonly AudioSource Source;
        public readonly float BaseVolume;

        public SourceState(AudioSource source, float baseVolume)
        {
            Source = source;
            BaseVolume = baseVolume;
        }
    }
}

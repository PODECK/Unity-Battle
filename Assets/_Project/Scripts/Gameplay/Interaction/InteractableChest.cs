// 플레이어의 상자 상호작용과 보상 연출을 제어하는 컴포넌트
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

public class InteractableChest : MonoBehaviour
{
    [Header("References")]
    public GameObject interactionPrompt;
    public GameObject cardPack;
    public Animator animator;
    public string openAnimationState = "Animated PBR Chest _Opening_UnCommon";
    public Light internalLight;
    public AudioSource audioSource;
    public AudioClip chestOpenClip;
    public AudioClip cardPickupClip;
    
    [Header("Settings")]
    public float interactionDistance = 4.0f;
    public float scaleDuration = 0.8f;
    public float floatAmplitude = 0.15f;
    public float spinSpeed = 60f;
    public float suckDuration = 0.6f;
    public float promptFadeDuration = 0.5f;
    public float lightFadeDuration = 0.5f;
    public float baseUIHeight = 1.35f;
    public float cardUIOffset = 0.55f;
    public float promptRisingAmount = 0.5f;
    public float uiScale = 0.45f;
    public float internalLightRange = 3.5f;
    public float internalLightIntensity = 2.0f;
    public float particleScale = 1.0f;
    [Range(0f, 1f)] public float chestOpenVolume = 0.75f;
    [Range(0f, 1f)] public float cardPickupVolume = 0.75f;
    public bool rememberCollectedState = true;
    public string chestPersistenceId = "";
    public Vector3 collisionBoxCenter = new Vector3(0f, 0.6f, 0f);
    public Vector3 collisionBoxSize = new Vector3(1.5f, 1.2f, 1.2f);

    private bool isPlayerNearby = false;
    private bool isOpened = false;
    private bool isCollected = false;
    private bool isSucking = false;
    
    private InputAction interactAction;
    private Vector3 originalPos;
    private Transform playerTransform;
    private SpriteRenderer[] promptSprites;
    private Coroutine promptFadeCoroutine;
    private float currentPromptAlpha = 0f;
    private float currentPromptRise = 0f;
    private float targetLightIntensity = 0f;
    private ParticleSystem[] allParticles;
    private Vector3[] particleOriginalScales;
    private string persistenceKey;

    void Awake()
    {
        EnsureSolidCollider();
        EnsureAudioSource();
        persistenceKey = BuildPersistenceKey();

        originalPos = transform.position;
        if (interactionPrompt != null)
        {
            promptSprites = interactionPrompt.GetComponentsInChildren<SpriteRenderer>(true);
            interactionPrompt.transform.localScale = Vector3.one * uiScale;
            SetPromptAlpha(0f);
            interactionPrompt.SetActive(true);
        }
        if (cardPack != null) cardPack.SetActive(false);
        if (internalLight != null) 
        {
            internalLight.range = internalLightRange;
            targetLightIntensity = internalLightIntensity;
            internalLight.intensity = 0f;
            internalLight.enabled = false;
        }
        
        if (animator == null) animator = GetComponentInChildren<Animator>();
        allParticles = GetComponentsInChildren<ParticleSystem>(true);
        CacheParticleScales();
        ApplyParticleScale();
        StopAllParticles();

        if (HasSavedCollection())
        {
            RestoreCollectedState();
        }
    }

    void EnsureSolidCollider()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        foreach (Collider col in colliders)
        {
            if (col != null && !col.isTrigger)
            {
                return;
            }
        }

        BoxCollider box = gameObject.AddComponent<BoxCollider>();
        box.isTrigger = false;
        box.center = collisionBoxCenter;
        box.size = collisionBoxSize;
    }

    void EnsureAudioSource()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 12f;
    }

    void PlaySfx(AudioClip clip, float volume)
    {
        if (clip == null) return;

        EnsureAudioSource();
        audioSource.PlayOneShot(clip, volume * AudioSettingsDropdown.SfxVolume);
    }

    string BuildPersistenceKey()
    {
        if (!string.IsNullOrWhiteSpace(chestPersistenceId))
        {
            return $"PODECK.Chest.{chestPersistenceId}";
        }

        string scenePart = SceneManager.GetActiveScene().path;
        string objectPath = GetTransformPath(transform);
        return $"PODECK.Chest.{scenePart}.{objectPath}";
    }

    string GetTransformPath(Transform target)
    {
        string path = $"{target.name}[{target.GetSiblingIndex()}]";
        Transform parent = target.parent;
        while (parent != null)
        {
            path = $"{parent.name}[{parent.GetSiblingIndex()}]/{path}";
            parent = parent.parent;
        }
        return path;
    }

    bool HasSavedCollection()
    {
        return rememberCollectedState && PlayerPrefs.GetInt(persistenceKey, 0) == 1;
    }

    void SaveCollectedState()
    {
        if (!rememberCollectedState) return;

        PlayerPrefs.SetInt(persistenceKey, 1);
        PlayerPrefs.Save();
    }

    void RestoreCollectedState()
    {
        isOpened = true;
        isCollected = true;
        isSucking = false;

        if (cardPack != null) cardPack.SetActive(false);
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.enabled = true;
            animator.Play(openAnimationState, 0, 1f);
            animator.Update(0f);
        }
        if (internalLight != null)
        {
            internalLight.intensity = 0f;
            internalLight.enabled = false;
        }
        StopAllParticles();
        SetPromptAlpha(0f);
    }

    void Start()
    {
        interactAction = InputSystem.actions?.FindAction("Player/Interact");
        interactAction?.Enable();
        FindPlayer();
    }

    void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    void CacheParticleScales()
    {
        if (allParticles == null)
        {
            particleOriginalScales = null;
            return;
        }

        particleOriginalScales = new Vector3[allParticles.Length];
        for (int i = 0; i < allParticles.Length; i++)
        {
            particleOriginalScales[i] = allParticles[i] != null ? allParticles[i].transform.localScale : Vector3.one;
        }
    }

    void ApplyParticleScale()
    {
        if (allParticles == null || particleOriginalScales == null)
        {
            return;
        }

        for (int i = 0; i < allParticles.Length; i++)
        {
            if (allParticles[i] == null)
            {
                continue;
            }

            allParticles[i].transform.localScale = particleOriginalScales[i] * particleScale;
        }
    }

    void StopAllParticles()
    {
        if (allParticles == null)
        {
            return;
        }

        foreach (ParticleSystem ps in allParticles)
        {
            if (ps == null)
            {
                continue;
            }

            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    void OnEnable() { if (interactAction != null) interactAction.Enable(); }
    void OnDisable() { if (interactAction != null) interactAction.Disable(); }

    void Update()
    {
        if (isSucking) return;

        if (playerTransform == null) FindPlayer();

        if (playerTransform != null)
        {
            float dist = Vector3.Distance(transform.position, playerTransform.position);
            bool nearby = dist <= interactionDistance;

            if (nearby != isPlayerNearby)
            {
                isPlayerNearby = nearby;
                UpdatePromptVisibility();
            }
        }

        if (isPlayerNearby && IsInteractPressed())
        {
            if (!isOpened)
            {
                StartCoroutine(OpenChestSequence());
            }
            else if (!isCollected && cardPack.activeSelf)
            {
                StartCoroutine(CollectCardSequence());
            }
        }

        if (interactionPrompt != null && currentPromptAlpha > 0.01f && Camera.main != null)
        {
            // Pure Billboard: Match camera rotation so it stays "flat" on screen
            interactionPrompt.transform.rotation = Camera.main.transform.rotation;
            
            float riseOffset = (1f - currentPromptRise) * -promptRisingAmount;
            Vector3 targetBasePos;
            if (isOpened && !isCollected && cardPack != null && cardPack.activeSelf)
            {
                targetBasePos = cardPack.transform.position + Vector3.up * cardUIOffset;
            }
            else
            {
                targetBasePos = transform.position + Vector3.up * baseUIHeight;
            }
            interactionPrompt.transform.position = targetBasePos + Vector3.up * riseOffset;
        }
    }

    private bool IsInteractPressed()
    {
        return (interactAction != null && interactAction.WasPressedThisFrame())
            || (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame);
    }

    void UpdatePromptVisibility()
    {
        if (interactionPrompt == null) return;
        
        bool shouldShow = isPlayerNearby && !isCollected && !isSucking;
        if (isOpened && !isCollected && (cardPack == null || !cardPack.activeSelf)) shouldShow = false;

        StartPromptFade(shouldShow ? 1f : 0f);
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
        
        // Appear immediately if target is 1
        if (target > 0.5f) SetPromptAlpha(1f);

        while (elapsed < promptFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / promptFadeDuration;
            
            float bounceT;
            if (target > 0.5f) // Appearing: Back Out (Dramatic)
            {
                float s = 2.5f; 
                float it = t - 1;
                bounceT = (it * it * ((s + 1) * it + s) + 1);
            }
            else // Disappearing: Back In (Dramatic)
            {
                float s = 2.5f;
                bounceT = t * t * ((s + 1) * t - s);
            }

            currentPromptRise = Mathf.Lerp(startRise, target, bounceT);
            yield return null;
        }
        currentPromptRise = target;
        // Disappear immediately if target is 0
        if (target < 0.5f) SetPromptAlpha(0f);
    }

    void SetPromptAlpha(float alpha)
    {
        currentPromptAlpha = alpha;
        if (promptSprites == null) return;
        foreach (var sr in promptSprites)
        {
            if (sr == null) continue;
            Color c = sr.color;
            if (sr.gameObject.name == "Prompt_BG")
                c.a = alpha * 0.6f;
            else
                c.a = alpha;
            sr.color = c;
        }
    }

    private IEnumerator OpenChestSequence()
    {
        isOpened = true;
        UpdatePromptVisibility();

        float shakeDuration = 0.5f;
        float shakeAmount = 0.05f;
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            transform.position = originalPos + Random.insideUnitSphere * shakeAmount;
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = originalPos;

        // 2. Open Lid via Animator
        if (animator != null) 
        {
            animator.enabled = true;
            animator.Play(openAnimationState, 0, 0f);
        }
        PlaySfx(chestOpenClip, chestOpenVolume);

        yield return new WaitForSeconds(1.3f);

        if (internalLight != null) 
        {
            internalLight.enabled = true;
            StartCoroutine(FadeLightRoutine(targetLightIntensity));
        }
        
        // Play FX if any
        foreach (var ps in allParticles) ps.Play();

        if (cardPack != null)
        {
            cardPack.SetActive(true);
            cardPack.transform.localScale = Vector3.zero;
            
            elapsed = 0f;
            while (elapsed < scaleDuration)
            {
                float t = elapsed / scaleDuration;
                float s = Mathf.Sin(t * Mathf.PI); 
                float scale = Mathf.Lerp(0f, 1.0f, t) + s * 0.4f;
                cardPack.transform.localScale = Vector3.one * scale;
                elapsed += Time.deltaTime;
                yield return null;
            }
            cardPack.transform.localScale = Vector3.one;
            
            UpdatePromptVisibility();
            StartCoroutine(CardPackFloatAndSpin());
        }
    }

    private IEnumerator CardPackFloatAndSpin()
    {
        Vector3 packStartPos = cardPack.transform.position;
        while (!isSucking && !isCollected)
        {
            cardPack.transform.position = packStartPos + new Vector3(0, Mathf.Sin(Time.time * 2f) * floatAmplitude, 0);
            cardPack.transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private IEnumerator CollectCardSequence()
    {
        isSucking = true;
        isCollected = true;
        SaveCollectedState();
        UpdatePromptVisibility();
        PlaySfx(cardPickupClip, cardPickupVolume);
        
        if (internalLight != null) StartCoroutine(FadeLightRoutine(0f));
        
        // Stop all particles
        foreach (var ps in allParticles) ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        Vector3 startPos = cardPack.transform.position;
        float elapsed = 0f;
        
        while (elapsed < suckDuration)
        {
            if (playerTransform != null)
            {
                Vector3 targetPos = playerTransform.position + Vector3.up * 1.2f;
                float t = elapsed / suckDuration;
                t = t * t * t; 
                
                cardPack.transform.position = Vector3.Lerp(startPos, targetPos, t);
                cardPack.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        cardPack.SetActive(false);
        isSucking = false;
    }

    IEnumerator FadeLightRoutine(float target)
    {
        float startIntensity = internalLight.intensity;
        float elapsed = 0f;
        while (elapsed < lightFadeDuration)
        {
            elapsed += Time.deltaTime;
            internalLight.intensity = Mathf.Lerp(startIntensity, target, elapsed / lightFadeDuration);
            yield return null;
        }
        internalLight.intensity = target;
        if (target <= 0.01f) internalLight.enabled = false;
    }




}

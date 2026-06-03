// 소리가 들리기 시작할 때 자연스럽게 페이드인하는 컴포넌트
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class WaterfallAudioFadeIn : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float targetVolume = 0.35f;
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private bool resetWhenOutOfRange = true;

    private Transform listenerTransform;
    private float fadeTime;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        audioSource.loop = true;
        audioSource.playOnAwake = true;
        audioSource.volume = 0f;

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    private void Update()
    {
        if (audioSource == null)
        {
            return;
        }

        Transform listener = GetListenerTransform();
        if (listener == null)
        {
            FadeToTarget();
            return;
        }

        float distance = Vector3.Distance(transform.position, listener.position);
        if (distance <= audioSource.maxDistance)
        {
            FadeToTarget();
        }
        else if (resetWhenOutOfRange)
        {
            fadeTime = 0f;
            audioSource.volume = 0f;
        }
    }

    private Transform GetListenerTransform()
    {
        if (listenerTransform != null)
        {
            return listenerTransform;
        }

        AudioListener listener = FindAnyObjectByType<AudioListener>();
        if (listener != null)
        {
            listenerTransform = listener.transform;
        }

        return listenerTransform;
    }

    private void FadeToTarget()
    {
        float scaledTargetVolume = targetVolume * AudioSettingsDropdown.BgmVolume;
        if (audioSource.volume >= scaledTargetVolume)
        {
            audioSource.volume = scaledTargetVolume;
            return;
        }

        float duration = Mathf.Max(0.01f, fadeDuration);
        fadeTime += Time.deltaTime;
        audioSource.volume = Mathf.Lerp(0f, scaledTargetVolume, fadeTime / duration);
    }
}

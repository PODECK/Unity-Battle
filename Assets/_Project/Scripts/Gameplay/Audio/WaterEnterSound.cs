// 플레이어가 물 영역에 들어갈 때 물소리를 재생하는 트리거 컴포넌트
using UnityEngine;

public class WaterEnterSound : MonoBehaviour
{
    public static bool IsPlayerInWater => totalPlayerInsideCount > 0;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip enterClip;
    [SerializeField] private AudioClip loopClip;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float enterVolume = 1f;
    [SerializeField] private float loopVolume = 0.35f;
    [SerializeField] private bool playLoopWhileInside = false;

    private int playerInsideCount;
    private static int totalPlayerInsideCount;

    private void Awake()
    {
        EnsureAudioSource();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        playerInsideCount++;
        totalPlayerInsideCount++;
        EnsureAudioSource();

        if (enterClip != null)
        {
            audioSource.PlayOneShot(enterClip, enterVolume * AudioSettingsDropdown.SfxVolume);
        }

        if (playLoopWhileInside && loopClip != null && !audioSource.isPlaying)
        {
            audioSource.clip = loopClip;
            audioSource.loop = true;
            audioSource.volume = loopVolume * AudioSettingsDropdown.BgmVolume;
            audioSource.Play();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        playerInsideCount = Mathf.Max(0, playerInsideCount - 1);
        totalPlayerInsideCount = Mathf.Max(0, totalPlayerInsideCount - 1);
        if (playerInsideCount == 0 && playLoopWhileInside && audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    private void OnDisable()
    {
        if (playerInsideCount <= 0)
        {
            return;
        }

        totalPlayerInsideCount = Mathf.Max(0, totalPlayerInsideCount - playerInsideCount);
        playerInsideCount = 0;
    }

    private void EnsureAudioSource()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
    }
}

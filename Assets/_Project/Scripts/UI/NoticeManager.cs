using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class NoticeManager : MonoBehaviour
{
    [SerializeField] private GameObject noticePrefab;
    [SerializeField] private float delay = 10f;
    [SerializeField] private float displayDuration = 5f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    private void Start()
    {
        Debug.Log("[NoticeManager] Started. Waiting for " + delay + " seconds...");
        StartCoroutine(ShowNoticeAfterDelay());
    }

    private IEnumerator ShowNoticeAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        Debug.Log("[NoticeManager] Delay finished. Attempting to show notice.");

        if (noticePrefab != null)
        {
            GameObject noticeInstance = Instantiate(noticePrefab);
            Debug.Log("[NoticeManager] Notice instantiated: " + noticeInstance.name);
            
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas != null)
            {
                noticeInstance.transform.SetParent(canvas.transform, false);
                Debug.Log("[NoticeManager] Notice parented to Canvas: " + canvas.name);
            }
            else
            {
                Debug.LogWarning("[NoticeManager] No Canvas found to parent the notice!");
            }
            
            NoticeUIRevealAnimation revealAnimation = noticeInstance.GetComponent<NoticeUIRevealAnimation>();
            if (revealAnimation != null)
            {
                revealAnimation.FadeOutAndDestroy(displayDuration, fadeOutDuration);
            }
            else
            {
                Destroy(noticeInstance, displayDuration);
            }
        }
        else
        {
            Debug.LogError("[NoticeManager] noticePrefab is not assigned!");
        }
    }
}

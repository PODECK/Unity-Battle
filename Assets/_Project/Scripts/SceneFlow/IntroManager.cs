using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class IntroManager : MonoBehaviour
{
    [SerializeField] private CinemachineCamera introCam;
    [SerializeField] private CinemachineCamera thirdPersonCam;
    [SerializeField] private float introDuration = 3f;

    [SerializeField] private GameObject playerController;

    void Start()
    {
        // 인트로 카메라 활성화
        introCam.Priority = 20;
        thirdPersonCam.Priority = 10;

        // 플레이어 조작 비활성화
        if (playerController != null)
            playerController.SetActive(false);

        // 3초 후 전환
        StartCoroutine(EndIntro());
    }

    IEnumerator EndIntro()
    {
        yield return new WaitForSeconds(introDuration);

        // 3인칭으로 전환
        introCam.Priority = 5;
        thirdPersonCam.Priority = 20;

        // 블렌딩 시간 대기 (1초)
        yield return new WaitForSeconds(1f);

        // 플레이어 조작 활성화
        if (playerController != null)
            playerController.SetActive(true);
    }
}
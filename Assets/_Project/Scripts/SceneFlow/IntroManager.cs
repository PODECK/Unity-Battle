// 인트로 카메라에서 플레이어 추적 카메라로 전환하고 조작 활성화를 관리하는 컴포넌트
using System.Collections;
using StarterAssets;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class IntroManager : MonoBehaviour
{
    [SerializeField] private CinemachineCamera introCam;
    [SerializeField] private CinemachineCamera thirdPersonCam;
    [SerializeField] private float introDuration = 3f;
    [SerializeField] private GameObject playerController;

    private ThirdPersonController thirdPersonController;
    private PlayerInput playerInput;

    private void Start()
    {
        ResolveSceneReferences();

        if (introCam == null || thirdPersonCam == null)
        {
            Debug.LogWarning("IntroManager camera references are missing.");
            return;
        }

        CachePlayerComponents();

        introCam.Priority = 20;
        thirdPersonCam.Priority = 10;
        SetPlayerControlEnabled(false);

        StartCoroutine(EndIntro());
    }

    private IEnumerator EndIntro()
    {
        yield return new WaitForSecondsRealtime(introDuration);

        introCam.Priority = 5;
        thirdPersonCam.Priority = 20;

        yield return new WaitForSecondsRealtime(1f);

        SetPlayerControlEnabled(true);
    }

    private void CachePlayerComponents()
    {
        if (playerController == null)
            return;

        thirdPersonController = playerController.GetComponent<ThirdPersonController>();
        playerInput = playerController.GetComponent<PlayerInput>();
    }

    private void ResolveSceneReferences()
    {
        CinemachineCamera[] cameras = FindObjectsByType<CinemachineCamera>(FindObjectsInactive.Include);

        foreach (CinemachineCamera camera in cameras)
        {
            if (introCam == null && camera.gameObject.name == "CM_Intro")
                introCam = camera;

            if (thirdPersonCam == null && camera.gameObject.name == "PlayerFollowCamera")
                thirdPersonCam = camera;
        }

        if (playerController == null)
            playerController = GameObject.FindWithTag("Player");

        Transform trackingTarget = thirdPersonCam != null ? thirdPersonCam.Follow : null;
        if (trackingTarget == null)
            trackingTarget = FindSceneTransform("CinemachineTarget");

        if (trackingTarget == null)
            trackingTarget = FindSceneTransform("PlayerCameraRoot");

        if (thirdPersonCam != null && trackingTarget != null)
            thirdPersonCam.Follow = trackingTarget;
    }

    private static Transform FindSceneTransform(string objectName)
    {
        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in objects)
        {
            if (obj.name == objectName && obj.scene.IsValid())
                return obj.transform;
        }

        return null;
    }

    private void SetPlayerControlEnabled(bool enabled)
    {
        if (thirdPersonController != null)
            thirdPersonController.enabled = enabled;

        if (playerInput != null)
            playerInput.enabled = enabled;
    }
}

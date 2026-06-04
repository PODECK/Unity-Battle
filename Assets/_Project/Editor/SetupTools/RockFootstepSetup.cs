// Third Person 캐릭터의 기본 발소리를 rock.mp3로 강제 연결하는 에디터 도구
using StarterAssets;
using UnityEditor;
using UnityEngine;

public static class RockFootstepSetup
{
    private const string RockClipPath = "Assets/ThirdParty/Footsteps Mini Sound Pack/rock.mp3";
    private const string PlayerPrefabPath = "Assets/ThirdParty/StarterAssets/ThirdPersonController/Prefabs/PlayerArmature.prefab";

    [MenuItem("Tools/PODECK/Force Rock Footstep")]
    public static void ForceRockFootstep()
    {
        AudioClip rockClip = AssetDatabase.LoadAssetAtPath<AudioClip>(RockClipPath);
        int sceneCount = 0;

        foreach (ThirdPersonController controller in Object.FindObjectsByType<ThirdPersonController>(FindObjectsInactive.Include))
        {
            Apply(controller, rockClip);
            EditorUtility.SetDirty(controller);
            sceneCount++;
        }

        bool prefabUpdated = ApplyToPrefab(rockClip);

        Debug.Log($"Rock footstep setup completed. Scene controllers: {sceneCount}, Prefab updated: {prefabUpdated}");
        EditorUtility.DisplayDialog("완료", $"씬 캐릭터: {sceneCount}\n프리팹 갱신: {prefabUpdated}", "확인");
    }

    private static bool ApplyToPrefab(AudioClip rockClip)
    {
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
        if (prefabRoot == null)
        {
            return false;
        }

        try
        {
            ThirdPersonController controller = prefabRoot.GetComponent<ThirdPersonController>();
            if (controller == null)
            {
                return false;
            }

            Apply(controller, rockClip);
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, PlayerPrefabPath);
            return true;
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
    }

    private static void Apply(ThirdPersonController controller, AudioClip rockClip)
    {
        SerializedObject serializedController = new(controller);
        SerializedProperty clips = serializedController.FindProperty("FootstepAudioClips");
        clips.arraySize = rockClip != null ? 1 : 0;
        if (rockClip != null)
        {
            clips.GetArrayElementAtIndex(0).objectReferenceValue = rockClip;
        }

        serializedController.FindProperty("FootstepOutputBoost").floatValue = 1.6f;
        serializedController.FindProperty("AudioFoley").objectReferenceValue = null;
        serializedController.ApplyModifiedPropertiesWithoutUndo();

        if (controller.AudioFootsteps != null)
        {
            controller.AudioFootsteps.playOnAwake = false;
            controller.AudioFootsteps.volume = 1f;
            controller.AudioFootsteps.spatialBlend = 0f;
            controller.AudioFootsteps.dopplerLevel = 0f;
            EditorUtility.SetDirty(controller.AudioFootsteps);
        }
    }
}

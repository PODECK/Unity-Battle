// 씬의 폭포 오디오에 페이드인 컴포넌트를 일괄 적용하는 에디터 도구
using UnityEditor;
using UnityEngine;

public class WaterfallAudioFadeSetup : EditorWindow
{
    private string waterfallNameKeyword = "Waterfall_Audio";
    private float targetVolume = 0.35f;
    private float fadeDuration = 1.5f;
    private float maxDistance = 80f;

    [MenuItem("Tools/PODECK/Setup Waterfall Audio Fade In")]
    public static void ShowWindow()
    {
        GetWindow<WaterfallAudioFadeSetup>("Waterfall Fade");
    }

    private void OnGUI()
    {
        GUILayout.Label("폭포 오디오 페이드인 설정", EditorStyles.boldLabel);
        waterfallNameKeyword = EditorGUILayout.TextField("폭포 오디오 이름", waterfallNameKeyword);
        targetVolume = EditorGUILayout.Slider("목표 볼륨", targetVolume, 0f, 1f);
        fadeDuration = EditorGUILayout.FloatField("페이드 시간", fadeDuration);
        maxDistance = EditorGUILayout.FloatField("최대 거리", maxDistance);

        GUILayout.Space(10);

        if (GUILayout.Button("활성 씬에 적용", GUILayout.Height(36)))
        {
            ApplyToActiveScene();
        }
    }

    private void ApplyToActiveScene()
    {
        GameObject[] objects = FindObjectsByType<GameObject>(FindObjectsInactive.Include);
        int applied = 0;

        foreach (GameObject obj in objects)
        {
            if (!obj.scene.IsValid() || !obj.name.Contains(waterfallNameKeyword))
            {
                continue;
            }

            AudioSource audioSource = obj.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                continue;
            }

            audioSource.loop = true;
            audioSource.playOnAwake = true;
            audioSource.spatialBlend = 1f;
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            audioSource.maxDistance = maxDistance;
            audioSource.volume = 0f;

            WaterfallAudioFadeIn fadeIn = obj.GetComponent<WaterfallAudioFadeIn>();
            if (fadeIn == null)
            {
                fadeIn = obj.AddComponent<WaterfallAudioFadeIn>();
            }

            SerializedObject serializedFadeIn = new(fadeIn);
            serializedFadeIn.FindProperty("audioSource").objectReferenceValue = audioSource;
            serializedFadeIn.FindProperty("targetVolume").floatValue = targetVolume;
            serializedFadeIn.FindProperty("fadeDuration").floatValue = fadeDuration;
            serializedFadeIn.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(obj);
            applied++;
        }

        Debug.Log($"Waterfall audio fade setup completed. Applied: {applied}");
        EditorUtility.DisplayDialog("완료", $"적용: {applied}", "확인");
    }
}

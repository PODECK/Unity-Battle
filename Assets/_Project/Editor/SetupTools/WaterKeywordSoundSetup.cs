// 이름에 Water가 포함된 씬 오브젝트에 물 진입 사운드 트리거를 일괄 적용하는 에디터 도구
using UnityEditor;
using UnityEngine;

public class WaterKeywordSoundSetup : EditorWindow
{
    private const string DefaultLoopClipPath = "Assets/_Project/Audio/Ambient/Free_Nature_Ambient/WAV_lake_ambient_loop.wav";

    private string keyword = "Water";
    private string excludeKeyword = "Waterfall";
    private float triggerDepth = 2f;
    private float waterVolume = 0.35f;
    private bool includePlaneChildren = true;
    private AudioClip enterClip;
    private AudioClip loopClip;
    private bool playLoopWhileInside;

    [MenuItem("Tools/PODECK/Setup Water Enter Sounds")]
    public static void ShowWindow()
    {
        GetWindow<WaterKeywordSoundSetup>("Water Sounds");
    }

    private void OnEnable()
    {
        LoadDefaultLoopClip();
    }

    private void OnGUI()
    {
        GUILayout.Label("물 진입 사운드 일괄 설정", EditorStyles.boldLabel);
        keyword = EditorGUILayout.TextField("이름 키워드", keyword);
        excludeKeyword = EditorGUILayout.TextField("제외 키워드", excludeKeyword);
        triggerDepth = EditorGUILayout.FloatField("트리거 깊이", triggerDepth);
        waterVolume = EditorGUILayout.Slider("전체 물 볼륨", waterVolume, 0f, 1f);
        includePlaneChildren = EditorGUILayout.Toggle("Water 하위 Plane 포함", includePlaneChildren);
        enterClip = (AudioClip)EditorGUILayout.ObjectField("진입 효과음", enterClip, typeof(AudioClip), false);
        loopClip = (AudioClip)EditorGUILayout.ObjectField("내부 반복음", loopClip, typeof(AudioClip), false);
        playLoopWhileInside = EditorGUILayout.Toggle("안에 있을 때 반복 재생", playLoopWhileInside);

        GUILayout.Space(10);

        if (GUILayout.Button("기본 호수 반복음 불러오기"))
        {
            LoadDefaultLoopClip();
        }

        if (GUILayout.Button("활성 씬에 적용", GUILayout.Height(36)))
        {
            ApplyToActiveScene();
        }

        if (GUILayout.Button("적용된 물 사운드 제거"))
        {
            RemoveWaterSoundsFromActiveScene();
        }
    }

    private void ApplyToActiveScene()
    {
        GameObject[] objects = FindObjectsByType<GameObject>(FindObjectsInactive.Include);
        int applied = 0;
        int skipped = 0;

        foreach (GameObject obj in objects)
        {
            if (!MatchesKeyword(obj))
            {
                continue;
            }

            if (MatchesExcludeKeyword(obj))
            {
                skipped++;
                continue;
            }

            if (SetupObject(obj))
            {
                applied++;
            }
            else
            {
                skipped++;
            }
        }

        Debug.Log($"Water enter sound setup completed. Applied: {applied}, Skipped: {skipped}");
        EditorUtility.DisplayDialog("완료", $"적용: {applied}\n건너뜀: {skipped}", "확인");
    }

    private void RemoveWaterSoundsFromActiveScene()
    {
        if (!EditorUtility.DisplayDialog("확인", "WaterEnterSound가 붙은 물 사운드 설정을 제거합니다.", "예", "아니오"))
        {
            return;
        }

        GameObject[] objects = FindObjectsByType<GameObject>(FindObjectsInactive.Include);
        int removed = 0;

        foreach (GameObject obj in objects)
        {
            WaterEnterSound sound = obj.GetComponent<WaterEnterSound>();
            if (sound == null)
            {
                continue;
            }

            AudioSource audioSource = obj.GetComponent<AudioSource>();
            if (audioSource != null && IsWaterSetupAudioSource(audioSource))
            {
                DestroyImmediate(audioSource);
            }

            BoxCollider boxCollider = obj.GetComponent<BoxCollider>();
            if (boxCollider != null && boxCollider.isTrigger)
            {
                DestroyImmediate(boxCollider);
            }

            DestroyImmediate(sound);
            EditorUtility.SetDirty(obj);
            removed++;
        }

        Debug.Log($"Water enter sound setup removed. Removed: {removed}");
        EditorUtility.DisplayDialog("완료", $"제거: {removed}", "확인");
    }

    private bool IsWaterSetupAudioSource(AudioSource audioSource)
    {
        if (audioSource.clip != null)
        {
            return audioSource.clip == loopClip || AssetDatabase.GetAssetPath(audioSource.clip) == DefaultLoopClipPath;
        }

        return !audioSource.playOnAwake && audioSource.spatialBlend > 0.99f;
    }

    private bool MatchesKeyword(GameObject obj)
    {
        if (!obj.scene.IsValid() || string.IsNullOrWhiteSpace(keyword))
        {
            return false;
        }

        if (!obj.name.Contains(keyword))
        {
            string assetPath = GetPrefabAssetPath(obj);
            if (assetPath.Contains(keyword))
            {
                return true;
            }

            return includePlaneChildren && IsPlaneUnderKeywordParent(obj);
        }

        return true;
    }

    private bool MatchesExcludeKeyword(GameObject obj)
    {
        if (string.IsNullOrWhiteSpace(excludeKeyword))
        {
            return false;
        }

        return obj.name.Contains(excludeKeyword) || GetPrefabAssetPath(obj).Contains(excludeKeyword);
    }

    private bool IsPlaneUnderKeywordParent(GameObject obj)
    {
        if (!obj.name.Contains("Plane"))
        {
            return false;
        }

        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            bool parentExcluded = !string.IsNullOrWhiteSpace(excludeKeyword) && parent.name.Contains(excludeKeyword);
            if (parent.name.Contains(keyword) && !parentExcluded)
            {
                return true;
            }

            parent = parent.parent;
        }

        return false;
    }

    private string GetPrefabAssetPath(GameObject obj)
    {
        GameObject prefabSource = PrefabUtility.GetCorrespondingObjectFromSource(obj);
        if (prefabSource == null)
        {
            return string.Empty;
        }

        return AssetDatabase.GetAssetPath(prefabSource);
    }

    private void LoadDefaultLoopClip()
    {
        loopClip = AssetDatabase.LoadAssetAtPath<AudioClip>(DefaultLoopClipPath);
        playLoopWhileInside = loopClip != null;
    }

    private bool SetupObject(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer == null)
        {
            return false;
        }

        BoxCollider trigger = obj.GetComponent<BoxCollider>();
        if (trigger == null)
        {
            trigger = obj.AddComponent<BoxCollider>();
        }

        trigger.isTrigger = true;
        ApplyTriggerBounds(obj, renderer, trigger);

        AudioSource audioSource = obj.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = obj.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 12f;
        audioSource.volume = waterVolume;

        WaterEnterSound sound = obj.GetComponent<WaterEnterSound>();
        if (sound == null)
        {
            sound = obj.AddComponent<WaterEnterSound>();
        }

        SerializedObject serializedSound = new(sound);
        serializedSound.FindProperty("audioSource").objectReferenceValue = audioSource;
        serializedSound.FindProperty("enterClip").objectReferenceValue = enterClip;
        serializedSound.FindProperty("loopClip").objectReferenceValue = loopClip;
        serializedSound.FindProperty("enterVolume").floatValue = waterVolume;
        serializedSound.FindProperty("loopVolume").floatValue = waterVolume;
        serializedSound.FindProperty("playLoopWhileInside").boolValue = playLoopWhileInside;
        serializedSound.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(obj);
        return true;
    }

    private void ApplyTriggerBounds(GameObject obj, Renderer renderer, BoxCollider trigger)
    {
        Bounds bounds = renderer.bounds;
        Vector3 scale = obj.transform.lossyScale;
        float sx = Mathf.Abs(scale.x) > 0.0001f ? Mathf.Abs(scale.x) : 1f;
        float sy = Mathf.Abs(scale.y) > 0.0001f ? Mathf.Abs(scale.y) : 1f;
        float sz = Mathf.Abs(scale.z) > 0.0001f ? Mathf.Abs(scale.z) : 1f;

        trigger.size = new Vector3(bounds.size.x / sx, triggerDepth / sy, bounds.size.z / sz);

        Vector3 localCenter = obj.transform.InverseTransformPoint(bounds.center);
        trigger.center = new Vector3(localCenter.x, localCenter.y - (trigger.size.y * 0.5f), localCenter.z);
    }
}

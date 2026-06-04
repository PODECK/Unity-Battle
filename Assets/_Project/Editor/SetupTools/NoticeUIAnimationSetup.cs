// NoticeUI 프리팹에 등장 연출 컴포넌트를 자동 연결하는 에디터 도구
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class NoticeUIAnimationSetup
{
    private const string PrefabPath = "Assets/_Project/Prefabs/NoticeUI.prefab";

    [MenuItem("Tools/PODECK/Setup NoticeUI Reveal Animation")]
    public static void Setup()
    {
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(PrefabPath);

        try
        {
            NoticeUIRevealAnimation animation = prefabRoot.GetComponent<NoticeUIRevealAnimation>();
            if (animation == null)
            {
                animation = prefabRoot.AddComponent<NoticeUIRevealAnimation>();
            }

            SerializedObject serializedAnimation = new(animation);
            serializedAnimation.FindProperty("diamond").objectReferenceValue = FindGraphic(prefabRoot, "Divider/Diamond");
            serializedAnimation.FindProperty("leftLine").objectReferenceValue = FindGraphic(prefabRoot, "Divider/LeftLine");
            serializedAnimation.FindProperty("rightLine").objectReferenceValue = FindGraphic(prefabRoot, "Divider/RightLine");
            serializedAnimation.FindProperty("mainText").objectReferenceValue = prefabRoot.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
            serializedAnimation.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPath);
            AssetDatabase.SaveAssets();
            Debug.Log("NoticeUI reveal animation setup completed.");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
    }

    private static Graphic FindGraphic(GameObject root, string path)
    {
        return root.transform.Find(path)?.GetComponent<Graphic>();
    }
}

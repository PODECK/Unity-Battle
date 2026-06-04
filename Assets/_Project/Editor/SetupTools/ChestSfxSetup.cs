// 상자 열림과 카드팩 획득 효과음을 InteractableChest에 일괄 연결하는 에디터 도구
using UnityEditor;
using UnityEngine;

public static class ChestSfxSetup
{
    private const string ChestOpenClipPath = "Assets/_Project/Audio/SFX/SFX_WoodenChest_Open.wav";
    private const string CardPickupClipPath = "Assets/_Project/Audio/SFX/SFX_Item_Pickup_Short.wav";

    [MenuItem("Tools/PODECK/Setup Chest SFX")]
    public static void SetupChestSfx()
    {
        AudioClip chestOpenClip = AssetDatabase.LoadAssetAtPath<AudioClip>(ChestOpenClipPath);
        AudioClip cardPickupClip = AssetDatabase.LoadAssetAtPath<AudioClip>(CardPickupClipPath);

        if (chestOpenClip == null || cardPickupClip == null)
        {
            Debug.LogError($"Chest SFX setup failed. Missing clip: {ChestOpenClipPath} or {CardPickupClipPath}");
            return;
        }

        InteractableChest[] chests = Object.FindObjectsByType<InteractableChest>(FindObjectsInactive.Include);

        int updatedCount = 0;
        foreach (InteractableChest chest in chests)
        {
            if (chest == null) continue;

            Undo.RecordObject(chest.gameObject, "Setup Chest SFX");
            Undo.RecordObject(chest, "Setup Chest SFX");

            AudioSource source = chest.GetComponent<AudioSource>();
            if (source == null)
            {
                source = Undo.AddComponent<AudioSource>(chest.gameObject);
            }

            source.playOnAwake = false;
            source.spatialBlend = 1f;
            source.rolloffMode = AudioRolloffMode.Logarithmic;
            source.minDistance = 1f;
            source.maxDistance = 12f;

            chest.audioSource = source;
            chest.chestOpenClip = chestOpenClip;
            chest.cardPickupClip = cardPickupClip;
            chest.chestOpenVolume = 0.75f;
            chest.cardPickupVolume = 0.75f;

            EditorUtility.SetDirty(source);
            EditorUtility.SetDirty(chest);
            EditorUtility.SetDirty(chest.gameObject);
            updatedCount++;
        }

        Debug.Log($"Chest SFX setup completed. Updated chests: {updatedCount}");
    }
}

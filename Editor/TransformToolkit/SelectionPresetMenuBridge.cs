using UnityEditor;
using UnityEngine;

namespace Wrj.TransformToolkit
{
    public static class SelectionPresetMenuBridge
{
    // --- Renumber ---
    [MenuItem("Edit/Renumber")]
    [MenuItem("GameObject/Renumber", false, 0)]
    private static void Renumber()
        => RunPreset<RenumberSelectionPreset>();

    [MenuItem("Edit/Renumber", true)]
    [MenuItem("GameObject/Renumber", true)]
    private static bool Renumber_Validate()
        => Selection.gameObjects != null && Selection.gameObjects.Length > 1;

    // --- Reorder Hierarchy To Selection (if you want it as its own menu item) ---
    [MenuItem("Edit/Reorder Hierarchy To Selection")]
    [MenuItem("GameObject/Reorder Hierarchy To Selection", false, 0)]
    private static void ReorderHierarchyToSelection()
        => RunPreset<ReorderHierarchyToSelectionPreset>();

    [MenuItem("Edit/Reorder Hierarchy To Selection", true)]
    [MenuItem("GameObject/Reorder Hierarchy To Selection", true)]
    private static bool ReorderHierarchyToSelection_Validate()
        => Selection.gameObjects != null && Selection.gameObjects.Length > 1;

    // -----------------------
    // Shared dispatch
    // -----------------------
    private static void RunPreset<TPreset>() where TPreset : SelectionPreset
    {
        var preset = FindFirstPreset<TPreset>();
        if (!preset)
        {
            EditorUtility.DisplayDialog(
                "Selection Preset Missing",
                $"No {typeof(TPreset).Name} asset exists. Open Transform Toolkit and select Refresh Presets to bake it.",
                "OK");
            return;
        }

        preset.Apply(Selection.gameObjects);
    }

    private static TPreset FindFirstPreset<TPreset>() where TPreset : SelectionPreset
    {
        var guids = AssetDatabase.FindAssets($"t:{typeof(TPreset).Name}");
        if (guids == null || guids.Length == 0) return null;

        var path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<TPreset>(path);
    }
}
}

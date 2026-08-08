using UnityEngine;

namespace Wrj.TransformToolkit
{
    [CreateAssetMenu(menuName = "Transform Toolkit/Selection Presets/Selection Preset Registry",
        fileName = "SelectionPresetRegistry")]
    public sealed class SelectionPresetRegistry : ScriptableObject
    {
        public SelectionPreset multiRenamePreset;
        public SelectionPreset renumberPreset;
        public SelectionPreset reorderHierarchyToSelectionPreset;
    }
}

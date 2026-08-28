using UnityEngine;
using UnityEditor;

namespace Wrj.TransformToolkit
{
    public abstract class TransformPreset : ScriptableObject
    {
        [Tooltip("Display name for this preset in the UI. If empty, uses the preset module name.")]
        [SerializeField] private string displayName;

        [Tooltip("Whether this preset is enabled and visible in the Transform Toolkit window.")]
        [SerializeField] private bool enabledInWindow = true;

        // Per-asset UI state can live here too if you want it serialized.
        [Tooltip("Whether the preset's settings are expanded in the UI.")]
        [SerializeField] private bool isExpanded = true;

        public string DisplayName => IsDefaultDisplayName(displayName) ? GetModuleName() : displayName;
        public bool EnabledInWindow => enabledInWindow;
        public bool IsExpanded { get => isExpanded; set => isExpanded = value; }

        private static bool IsDefaultDisplayName(string value)
            => string.IsNullOrWhiteSpace(value) || value == "Preset";

        private string GetModuleName()
        {
            foreach (var attribute in GetType().GetCustomAttributes(typeof(CreateAssetMenuAttribute), false))
            {
                if (attribute is not CreateAssetMenuAttribute createAssetMenu ||
                    string.IsNullOrWhiteSpace(createAssetMenu.menuName))
                    continue;

                int separatorIndex = createAssetMenu.menuName.LastIndexOf('/');
                return separatorIndex >= 0
                    ? createAssetMenu.menuName.Substring(separatorIndex + 1)
                    : createAssetMenu.menuName;
            }

            return ObjectNames.NicifyVariableName(name);
        }

        /// <summary>Draw preset GUI. Return true if settings changed.</summary>
        public abstract bool DrawGUI(PresetContext ctx);

        /// <summary>Apply preset to targets. Window handles Undo.</summary>
        public abstract void Apply(PresetContext ctx, Transform[] targets);
    }
}

using UnityEngine;
using UnityEditor;

namespace Wrj.TransformToolkit
{
    public abstract class SelectionPreset : ScriptableObject
    {
        [Tooltip("Display name for this preset in the UI. If empty, uses the preset module name.")]
        [SerializeField] private string displayName;

        [Tooltip("Whether this preset is enabled and visible in the Transform Toolkit window.")]
        [SerializeField] private bool enabledInWindow = true;

        [Tooltip("Whether the preset's settings are expanded in the UI.")]
        [SerializeField] private bool isExpanded = true;

        public string DisplayName => IsDefaultDisplayName(displayName) ? GetModuleName() : displayName;
        public bool EnabledInWindow => enabledInWindow;
        public bool IsExpanded { get => isExpanded; set => isExpanded = value; }

        private static bool IsDefaultDisplayName(string value)
            => string.IsNullOrWhiteSpace(value) || value == "Selection Preset";

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

        public abstract bool DrawGUI();
        public abstract void Apply(GameObject[] targets);
    }
}

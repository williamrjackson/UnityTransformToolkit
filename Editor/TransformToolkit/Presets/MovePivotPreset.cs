using System;
using UnityEditor;
using UnityEngine;

namespace Wrj.TransformToolkit
{
    [CreateAssetMenu(
        menuName = "Transform Toolkit/Presets/Move Pivot",
        fileName = "MovePivotPreset")]
    public sealed class MovePivotPreset : TransformPreset
    {
        private enum PivotSource
        {
            WorldPosition,
            GameObject
        }

        [Header("Pivot")]
        [SerializeField] private PivotSource pivotSource = PivotSource.WorldPosition;
        [SerializeField] private Vector3 worldPosition = Vector3.zero;
        [SerializeField] private string sourceGlobalId;

        [NonSerialized] private GameObject _sourceCache;

        public override bool DrawGUI(PresetContext ctx)
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.HelpBox(
                "Translates the entire selection by one shared delta so its world-space pivot " +
                "(the average of the selected Transform positions) matches the target. " +
                "The objects keep their positions relative to one another.",
                MessageType.None);

            pivotSource = (PivotSource)EditorGUILayout.EnumPopup("Pivot Source", pivotSource);

            if (pivotSource == PivotSource.WorldPosition)
            {
                worldPosition = EditorGUILayout.Vector3Field("World Position", worldPosition);
            }
            else
            {
                DrawSourceField();
            }

            return EditorGUI.EndChangeCheck();
        }

        public override void Apply(PresetContext ctx, Transform[] targets)
        {
            if (targets == null || targets.Length == 0)
                return;

            if (!TryGetPivotPosition(out Vector3 pivotPosition))
            {
                EditorUtility.DisplayDialog(
                    "Move Pivot",
                    "No source GameObject is assigned, or it could not be resolved in the current scene.",
                    "OK");
                return;
            }

            Vector3 selectionPivot = ComputeSelectionPivot(targets, out int validTargetCount);
            if (validTargetCount == 0) return;

            Vector3 delta = pivotPosition - selectionPivot;
            Transform[] validTargets = new Transform[validTargetCount];
            Vector3[] originalPositions = new Vector3[validTargetCount];
            int next = 0;

            for (int i = 0; i < targets.Length; i++)
            {
                Transform target = targets[i];
                if (!target) continue;

                validTargets[next] = target;
                originalPositions[next] = target.position;
                next++;
            }

            // Parents must move before selected descendants, otherwise a selected child can
            // inherit the group delta and then receive it a second time.
            Array.Sort(validTargets, originalPositions, new HierarchyDepthComparer());

            for (int i = 0; i < validTargets.Length; i++)
                validTargets[i].position = originalPositions[i] + delta;
        }

        private void DrawSourceField()
        {
            GameObject current = ResolveSource();

            EditorGUI.BeginChangeCheck();
            GameObject picked = (GameObject)EditorGUILayout.ObjectField(
                new GUIContent("Source GameObject", "Use this GameObject's Transform position as the pivot point."),
                current,
                typeof(GameObject),
                allowSceneObjects: true);

            if (EditorGUI.EndChangeCheck())
            {
                SetSource(picked);
                EditorUtility.SetDirty(this);
            }
        }

        private bool TryGetPivotPosition(out Vector3 position)
        {
            if (pivotSource == PivotSource.WorldPosition)
            {
                position = worldPosition;
                return true;
            }

            GameObject source = ResolveSource();
            if (source)
            {
                position = source.transform.position;
                return true;
            }

            position = default;
            return false;
        }

        private static Vector3 ComputeSelectionPivot(Transform[] targets, out int validTargetCount)
        {
            Vector3 sum = Vector3.zero;
            validTargetCount = 0;

            for (int i = 0; i < targets.Length; i++)
            {
                Transform target = targets[i];
                if (!target) continue;

                sum += target.position;
                validTargetCount++;
            }

            return validTargetCount > 0 ? sum / validTargetCount : Vector3.zero;
        }

        private sealed class HierarchyDepthComparer : System.Collections.Generic.IComparer<Transform>
        {
            public int Compare(Transform a, Transform b)
            {
                return GetDepth(a).CompareTo(GetDepth(b));
            }

            private static int GetDepth(Transform transform)
            {
                int depth = 0;
                while (transform && transform.parent)
                {
                    depth++;
                    transform = transform.parent;
                }

                return depth;
            }
        }

        private void SetSource(GameObject source)
        {
            _sourceCache = source;
            sourceGlobalId = source
                ? GlobalObjectId.GetGlobalObjectIdSlow(source).ToString()
                : null;
        }

        private GameObject ResolveSource()
        {
            if (_sourceCache) return _sourceCache;
            if (string.IsNullOrEmpty(sourceGlobalId)) return null;
            if (!GlobalObjectId.TryParse(sourceGlobalId, out GlobalObjectId id)) return null;

            _sourceCache = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id) as GameObject;
            return _sourceCache;
        }
    }
}

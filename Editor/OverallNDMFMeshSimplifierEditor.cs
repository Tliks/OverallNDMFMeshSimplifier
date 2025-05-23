using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using nadena.dev.ndmf.preview;

namespace com.aoyon.OverallNDMFMeshSimplifier
{
    [CustomEditor(typeof(OverallNdmfMeshSimplifier))]
    internal class OverallNDMFMeshSimplifierEditor : Editor
    {
        private SerializedProperty _isAutoAdjust;
        private SerializedProperty _targetTriangleCount;
        private SerializedProperty _simplifierTargets;

        private OverallNdmfMeshSimplifier _component;

        private void OnEnable()
        {
            _isAutoAdjust = serializedObject.FindProperty(nameof(OverallNdmfMeshSimplifier.IsAutoAdjust));
            _targetTriangleCount = serializedObject.FindProperty(nameof(OverallNdmfMeshSimplifier.TargetTriangleCount));
            _simplifierTargets = serializedObject.FindProperty(nameof(OverallNdmfMeshSimplifier.Targets));
            
            _component = target as OverallNdmfMeshSimplifier;
            AssginTarget();            
        }

        private void AssginTarget()
        {
            Undo.RecordObject(_component, "AssginTarget");
            _component.AssginTarget();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            AutoAdjustGUI();
            EditorGUILayout.Space();
            TargetGUI();
            EditorGUILayout.Space();
            TogglePreviewGUI(OverallNDMFMeshSimplifierPreview.ToggleNode);

            serializedObject.ApplyModifiedProperties();
        }

        GUIContent[] displayTriangleOptions = new GUIContent[]
        {
            new GUIContent("PC-Poor-Medium-Good"),
            new GUIContent("PC-Excellent"),
            new GUIContent("Mobile-Poor"),
            new GUIContent("Mobile-Medium"),
            new GUIContent("Mobile-Good"),
            new GUIContent("Mobile-Excellent")
        };
        int[] triangleOptions = new int[]
        {
            70000,
            32000,
            20000,
            15000,
            10000,
            7500
        };
        private void AutoAdjustGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Target Triangle Count", GUILayout.Width(150f));
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(_targetTriangleCount, GUIContent.none);
                EditorGUILayout.IntPopup(_targetTriangleCount, displayTriangleOptions, triangleOptions, GUIContent.none, GUILayout.Width(200f));
                if (EditorGUI.EndChangeCheck() && _isAutoAdjust.boolValue)
                {
                    AdjustQuality();
                }
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Adjust", GUILayout.Width(90f))) { AdjustQuality(); }
                EditorGUI.BeginChangeCheck();
                _isAutoAdjust.boolValue = EditorGUILayout.ToggleLeft("Enable Auto Adjust", _isAutoAdjust.boolValue);
                if (EditorGUI.EndChangeCheck() && _isAutoAdjust.boolValue)
                {
                    AdjustQuality();
                }
            }
            // 未知のレンダラーに対する設定など詳細設定を追加したい
        }

        private bool _showOthers = false;
        private SerializedProperty _currentSimplifySettingTarget = null;
        private bool _showCurrentSimplifySetting = true;
        private void TargetGUI()
        {
            var (enabledTargets, disabledTargets, editorOnlyTargets) = GetTargets();

            using (new EditorGUILayout.HorizontalScope())
            {
                var current = enabledTargets.Sum(t => t.FindPropertyRelative(nameof(OverallNDMFMeshSimplifierTarget.AbsoulteTriangleCount)).intValue)
                    + disabledTargets.Sum(t => GetTotalTriangleCount(t));
                var sum = enabledTargets.Concat(disabledTargets).Sum(t => t.FindPropertyRelative(nameof(OverallNDMFMeshSimplifierTarget.TotalTriangleCount)).intValue);
                var countLabel = $"Current: {current} / {sum}";
                var labelWidth1 = 7f * countLabel.ToString().Count();
                var isOverflow = _targetTriangleCount.intValue < current;
                if (isOverflow) EditorGUILayout.LabelField(countLabel + " - Overflow!", GUIStyleHelper.redStyle, GUILayout.Width(labelWidth1));
                else EditorGUILayout.LabelField(countLabel, GUILayout.Width(labelWidth1));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Set 50%", GUILayout.Width(90f))) { SetQualityAll(0.5f); }
                if (GUILayout.Button("Set 100%", GUILayout.Width(90f))) { SetQualityAll(1.0f); }
                var iconContent = EditorGUIUtility.IconContent("AssemblyLock");
                iconContent.tooltip = "Lock value";
                EditorGUILayout.LabelField(iconContent, GUILayout.Width(18f));
                EditorGUILayout.LabelField(GUIContent.none, GUILayout.Width(18f));
            }

            SortTargets(enabledTargets);
            var labelWidth2 = enabledTargets.Any() ? IntWidth(enabledTargets.First()) + 14f : 0f;
            for (int i = 0; i < enabledTargets.Count; i++)
            {
                var enabledTarget = enabledTargets[i];
                var state = enabledTarget.FindPropertyRelative(nameof(OverallNDMFMeshSimplifierTarget.State));
                var renderer = enabledTarget.FindPropertyRelative(nameof(OverallNDMFMeshSimplifierTarget.Renderer));
                var absoulteValue = enabledTarget.FindPropertyRelative(nameof(OverallNDMFMeshSimplifierTarget.AbsoulteTriangleCount));
                var totalTriangleCount = GetTotalTriangleCount(enabledTarget);
                var fixedValue = enabledTarget.FindPropertyRelative(nameof(OverallNDMFMeshSimplifierTarget.Fixed));

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(state, GUIContent.none, GUILayout.Width(70f));
                    EditorGUILayout.ObjectField(renderer.objectReferenceValue, typeof(Renderer), false, GUILayout.MinWidth(100f)); // ReadOnly
                    EditorGUILayout.IntSlider(absoulteValue, 0, totalTriangleCount, GUIContent.none, GUILayout.MinWidth(140f));
                    if (EditorGUI.EndChangeCheck() && _isAutoAdjust.boolValue)
                    {
                        AdjustQuality(enabledTarget);
                    }
                    EditorGUILayout.LabelField(new GUIContent($"/ {totalTriangleCount}"), GUILayout.Width(labelWidth2));
                    EditorGUILayout.PropertyField(fixedValue, GUIContent.none, GUILayout.Width(18f));
                    if (GUILayout.Button(EditorGUIUtility.IconContent("Settings@2x"), GUIStyleHelper.iconButtonStyle, GUILayout.Width(16f), GUILayout.Height(16f))) 
                    { 
                        _currentSimplifySettingTarget = enabledTarget;
                        _showCurrentSimplifySetting = true;
                    }
                }
            }

            bool warnEditorOnly = editorOnlyTargets.Any(editorOnlyTarget =>
            {
                var renderer = editorOnlyTarget.FindPropertyRelative(nameof(OverallNDMFMeshSimplifierTarget.Renderer));
                return !Utils.IsEditorOnlyInHierarchy((renderer.objectReferenceValue as Renderer).gameObject);
            });
            using (new EditorGUILayout.HorizontalScope())
            {
                var foldoutstyle = warnEditorOnly ? GUIStyleHelper.foldOutYellowStyle : EditorStyles.foldout;
                _showOthers = EditorGUILayout.Foldout(_showOthers, "Others", foldoutstyle);
                if (warnEditorOnly) EditorGUILayout.LabelField(EditorGUIUtility.IconContent("Warning@2x"), GUILayout.Width(18f));
            }
            if (_showOthers)
            {
                SortTargets(disabledTargets);
                SortTargets(editorOnlyTargets);
                var width1 = disabledTargets.Any() ? IntWidth(disabledTargets.First()) : 0f;
                var width2 = editorOnlyTargets.Any() ? IntWidth(editorOnlyTargets.First()) : 0f;
                var labelWidth3 = Mathf.Max(width1, width2) * 2 + 21f;
                var otherTargets = disabledTargets.Concat(editorOnlyTargets);
                foreach (var otherTarget in otherTargets)
                {
                    var state = otherTarget.FindPropertyRelative(nameof(OverallNDMFMeshSimplifierTarget.State));
                    var renderer = otherTarget.FindPropertyRelative(nameof(OverallNDMFMeshSimplifierTarget.Renderer));
                    var totalTriangleCount = GetTotalTriangleCount(otherTarget);

                    var IsEditorOnly = state.intValue == (int)OverallNdmfMeshSimplifierTargetState.EditorOnly;
                    var suspicious = IsEditorOnly && !Utils.IsEditorOnlyInHierarchy((renderer.objectReferenceValue as Renderer).gameObject);
                    
                    var defaultBackgroundColor = GUI.backgroundColor;
                    if (suspicious) GUI.backgroundColor = Color.yellow;
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(state, GUIContent.none, GUILayout.Width(100f));
                        if (EditorGUI.EndChangeCheck() && _isAutoAdjust.boolValue)
                        {
                            AdjustQuality();
                        }
                        EditorGUILayout.ObjectField(renderer.objectReferenceValue, typeof(Renderer), false); // ReadOnly
                        var countLabel = IsEditorOnly 
                            ? $"0 / {totalTriangleCount}"
                            : $"{totalTriangleCount} / {totalTriangleCount}";
                        EditorGUILayout.LabelField(new GUIContent(countLabel), GUILayout.Width(labelWidth3));
                    }
                    GUI.backgroundColor = defaultBackgroundColor;
                }
                if (warnEditorOnly) EditorGUILayout.HelpBox("Ensure Renderer is EditorOnly", MessageType.Warning);
            }

            if (_currentSimplifySettingTarget != null)
            {
                var renderer = _currentSimplifySettingTarget.FindPropertyRelative(nameof(OverallNDMFMeshSimplifierTarget.Renderer));
                var options = _currentSimplifySettingTarget.FindPropertyRelative(nameof(OverallNDMFMeshSimplifierTarget.Options));
                
                _showCurrentSimplifySetting = EditorGUILayout.Foldout(_showCurrentSimplifySetting, $"Simplifier Options for {renderer?.objectReferenceValue?.name}");
                if (_showCurrentSimplifySetting)
                {
                    var iterator = options;
                    iterator.NextVisible(true);
                    while (iterator.NextVisible(false) && iterator.depth == 3)
                    {
                        EditorGUILayout.PropertyField(iterator);
                    }
                }
            }

            static float IntWidth(SerializedProperty prop) => 7f * GetTotalTriangleCount(prop).ToString().Count();
        }

        private void TogglePreviewGUI(TogglablePreviewNode toggleNode)
        {
            if (toggleNode.IsEnabled.Value)
            {
                if (GUILayout.Button("Disable NDMF Preview"))
                {
                    toggleNode.IsEnabled.Value = false;
                }
            }
            else
            {
                if (GUILayout.Button("Enable NDMF Preview"))
                {
                    toggleNode.IsEnabled.Value = true;
                }
            }
        }

        private (List<SerializedProperty> enabledTargets, List<SerializedProperty> disabledTargets, List<SerializedProperty> editorOnlyTargets) GetTargets()
        {
            var enabledTargets = new List<SerializedProperty>();
            var disabledTargets = new List<SerializedProperty>();
            var editorOnlyTargets = new List<SerializedProperty>();
            for (int i = 0; i < _simplifierTargets.arraySize; i++)
            {
                var simplifierTarget = _simplifierTargets.GetArrayElementAtIndex(i);
                var renderer = simplifierTarget.FindPropertyRelative(nameof(OverallNDMFMeshSimplifierTarget.Renderer));
                if (renderer.objectReferenceValue == null) continue;
                var state = simplifierTarget.FindPropertyRelative(nameof(OverallNDMFMeshSimplifierTarget.State));

                switch (state.intValue)
                {
                    case (int)OverallNdmfMeshSimplifierTargetState.Enabled:
                        enabledTargets.Add(simplifierTarget);
                        break;
                    case (int)OverallNdmfMeshSimplifierTargetState.Disabled:
                        disabledTargets.Add(simplifierTarget);
                        break;
                    case (int)OverallNdmfMeshSimplifierTargetState.EditorOnly:
                        editorOnlyTargets.Add(simplifierTarget);
                        break;
                }
            }
            return (enabledTargets, disabledTargets, editorOnlyTargets);
        }

        private static int GetTotalTriangleCount(SerializedProperty targetProp)
        {
            var renderer = targetProp.FindPropertyRelative(nameof(OverallNDMFMeshSimplifierTarget.Renderer)).objectReferenceValue as Renderer;
            if (renderer == null) return 0;

            if (OverallNDMFMeshSimplifierPreview.TryGetTotalTriangleCount(renderer, out var triCount))
            {
                return triCount;
            }
            else
            {
                return targetProp.FindPropertyRelative(nameof(OverallNDMFMeshSimplifierTarget.TotalTriangleCount)).intValue;
            }
        }

        private void SortTargets(List<SerializedProperty> targets)
        {
            targets.Sort((a, b) =>
            {
                var aTotalTriangleCount = a.FindPropertyRelative(nameof(OverallNDMFMeshSimplifierTarget.TotalTriangleCount)).intValue;
                var bTotalTriangleCount = b.FindPropertyRelative(nameof(OverallNDMFMeshSimplifierTarget.TotalTriangleCount)).intValue;
                return bTotalTriangleCount - aTotalTriangleCount;
            });
        }

        private void AdjustQuality(SerializedProperty fixedProperty = null)
        {
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            var (enabledTargets, disabledTargets, editorOnlyTargets) = GetTargets();

            var adjustableTargets = new List<SerializedProperty>();
            var unadjustablecount = 0;
            foreach (var enabledTarget in enabledTargets)
            {
                var fixedValue = enabledTarget.FindPropertyRelative(nameof(OverallNDMFMeshSimplifierTarget.Fixed)).boolValue;

                if (!fixedValue && !SerializedProperty.DataEquals(fixedProperty, enabledTarget)) adjustableTargets.Add(enabledTarget);
                else unadjustablecount += enabledTarget.FindPropertyRelative(nameof(OverallNDMFMeshSimplifierTarget.AbsoulteTriangleCount)).intValue; // 固定された対象はAbsoulteTriangleCount
            }
            unadjustablecount += disabledTargets.Sum(t => t.FindPropertyRelative(nameof(OverallNDMFMeshSimplifierTarget.TotalTriangleCount)).intValue); // TotalTriangleCount

            int actualTargetCount = _targetTriangleCount.intValue - unadjustablecount;
            if (actualTargetCount <= 0) 
            {
                Debug.LogError("Couldn't adjust quality. Unadjustable renderer is too many for Target Count. Make renderer unfixed or enabled");
                return;
            }

            var adjustableRatio = (float)actualTargetCount / adjustableTargets.Sum(t => t.FindPropertyRelative(nameof(OverallNDMFMeshSimplifierTarget.AbsoulteTriangleCount)).intValue);

            foreach (var adjustableTarget in adjustableTargets)
            {
                var absoulteValue = adjustableTarget.FindPropertyRelative(nameof(OverallNDMFMeshSimplifierTarget.AbsoulteTriangleCount));
                var totalTriangleCount = adjustableTarget.FindPropertyRelative(nameof(OverallNDMFMeshSimplifierTarget.TotalTriangleCount));
                absoulteValue.intValue = (int)(absoulteValue.intValue * adjustableRatio);
                if (absoulteValue.intValue > totalTriangleCount.intValue)
                {
                    absoulteValue.intValue = totalTriangleCount.intValue;
                }
            }
            
            var overflow = adjustableTargets.Sum(t => t.FindPropertyRelative(nameof(OverallNDMFMeshSimplifierTarget.AbsoulteTriangleCount)).intValue) - actualTargetCount;

            while (overflow > 0 && adjustableTargets.Any())
            {
                foreach (var adjustableTarget in adjustableTargets)
                {
                    var absoulteValue = adjustableTarget.FindPropertyRelative(nameof(OverallNDMFMeshSimplifierTarget.AbsoulteTriangleCount));
                    var totalTriangleCount = adjustableTarget.FindPropertyRelative(nameof(OverallNDMFMeshSimplifierTarget.TotalTriangleCount));

                    if (absoulteValue.intValue < totalTriangleCount.intValue)
                    {
                        absoulteValue.intValue++;
                        overflow--;
                        if (overflow <= 0) break;
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void SetQualityAll(float ratio)
        {
            for (int i = 0; i < _simplifierTargets.arraySize; i++)
            {
                var simplifierTarget = _simplifierTargets.GetArrayElementAtIndex(i);
                var state = simplifierTarget.FindPropertyRelative(nameof(OverallNDMFMeshSimplifierTarget.State));
                var fixedValue = simplifierTarget.FindPropertyRelative(nameof(OverallNDMFMeshSimplifierTarget.Fixed));
                var absoulteValue = simplifierTarget.FindPropertyRelative(nameof(OverallNDMFMeshSimplifierTarget.AbsoulteTriangleCount));
                var totalTriangleCount = simplifierTarget.FindPropertyRelative(nameof(OverallNDMFMeshSimplifierTarget.TotalTriangleCount));

                if (state.intValue == (int)OverallNdmfMeshSimplifierTargetState.Enabled && !fixedValue.boolValue)
                {
                    absoulteValue.intValue = (int)(totalTriangleCount.intValue * ratio);
                }
            }
        }
    }

    internal static class GUIStyleHelper
    {
        private static GUIStyle m_iconButtonStyle;
        public static GUIStyle iconButtonStyle
        {
            get
            {
                if (m_iconButtonStyle == null) InitIconButtonStyle();
                return m_iconButtonStyle;
            }
        }
        static void InitIconButtonStyle()
        {
            m_iconButtonStyle = new GUIStyle();
        }

        private static GUIStyle m_redStyle;
        public static GUIStyle redStyle
        {
            get
            {
                if (m_redStyle == null) InitRedStyle();
                return m_redStyle;
            }
        }
        static void InitRedStyle()
        {
            m_redStyle = new GUIStyle();
            m_redStyle.normal = new GUIStyleState() { textColor = Color.red };
        }

        private static GUIStyle m_foldOutYellowStyle;
        public static GUIStyle foldOutYellowStyle
        {
            get
            {
                if (m_foldOutYellowStyle == null) InitFoldOutYellowStyle();
                return m_foldOutYellowStyle;
            }
        }
        static void InitFoldOutYellowStyle()
        {
            m_foldOutYellowStyle = new GUIStyle(EditorStyles.foldout);
            m_foldOutYellowStyle.normal.textColor = Color.yellow;
            m_foldOutYellowStyle.focused.textColor = Color.yellow;
        }
    }
}
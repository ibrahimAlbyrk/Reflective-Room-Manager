using System.Linq;
using UnityEngine;
using UnityEditor;
using REFLECTIVE.Runtime.Configuration;

namespace REFLECTIVE.Editor.Configuration
{
    [CustomEditor(typeof(ReflectiveBuildConfig))]
    public class ReflectiveBuildConfigEditor : UnityEditor.Editor
    {
        private SerializedProperty _buildTypeProp;
        private SerializedProperty _autoDetectProp;

        private static readonly string[] REFLECTIVE_SYMBOLS = { "REFLECTIVE_SERVER", "REFLECTIVE_CLIENT" };

        private void OnEnable()
        {
            _buildTypeProp = serializedObject.FindProperty("_buildType");
            _autoDetectProp = serializedObject.FindProperty("_autoDetectServerPlatform");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawHeader();
            EditorGUILayout.Space(10);

            DrawBuildTypeSelection();
            EditorGUILayout.Space(5);

            DrawPlatformOverride();
            EditorGUILayout.Space(10);

            DrawCurrentStatus();
            EditorGUILayout.Space(10);

            DrawApplyButton();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Reflective Room Manager", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Build Configuration", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
        }

        private void DrawBuildTypeSelection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_buildTypeProp, new GUIContent("Build Type"));

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                ApplyBuildType((ReflectiveBuildConfig)target);
            }

            // Description based on selection
            var buildType = (ReflectiveBuildType)_buildTypeProp.enumValueIndex;
            var description = buildType switch
            {
                ReflectiveBuildType.HostClient => "Both server and client code included.\nUse for listen servers or development.",
                ReflectiveBuildType.DedicatedServer => "Server-only code included.\nUse for dedicated server builds.",
                ReflectiveBuildType.PureClient => "Client-only code included.\nUse for client builds connecting to dedicated servers.",
                _ => ""
            };

            EditorGUILayout.HelpBox(description, MessageType.Info);
            EditorGUILayout.EndVertical();
        }

        private void DrawPlatformOverride()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(_autoDetectProp, new GUIContent("Auto-Detect Server Platform"));

            if (_autoDetectProp.boolValue)
            {
                EditorGUILayout.HelpBox(
                    "When building for Dedicated Server platform, DedicatedServer build type will be used automatically.",
                    MessageType.None);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawCurrentStatus()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Current Scripting Defines", EditorStyles.boldLabel);

            var currentDefines = GetCurrentDefines();
            var serverActive = currentDefines.Contains("REFLECTIVE_SERVER");
            var clientActive = currentDefines.Contains("REFLECTIVE_CLIENT");

            EditorGUILayout.BeginHorizontal();
            DrawStatusIcon(serverActive);
            EditorGUILayout.LabelField("REFLECTIVE_SERVER", serverActive ? EditorStyles.boldLabel : EditorStyles.label);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            DrawStatusIcon(clientActive);
            EditorGUILayout.LabelField("REFLECTIVE_CLIENT", clientActive ? EditorStyles.boldLabel : EditorStyles.label);
            EditorGUILayout.EndHorizontal();

            // Check if out of sync
            var config = (ReflectiveBuildConfig)target;
            var expectedDefines = config.GetDefineSymbols();
            var isInSync = expectedDefines.All(d => currentDefines.Contains(d)) &&
                           REFLECTIVE_SYMBOLS.Where(s => !expectedDefines.Contains(s)).All(s => !currentDefines.Contains(s));

            if (!isInSync)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox("Scripting defines are out of sync with config!", MessageType.Warning);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawStatusIcon(bool active)
        {
            var color = GUI.color;
            GUI.color = active ? Color.green : Color.gray;
            EditorGUILayout.LabelField(active ? "●" : "○", GUILayout.Width(20));
            GUI.color = color;
        }

        private void DrawApplyButton()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Apply Build Configuration", GUILayout.Height(30)))
            {
                ApplyBuildType((ReflectiveBuildConfig)target);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox(
                "Click 'Apply' to update scripting define symbols.\nThis will trigger a recompilation.",
                MessageType.None);
        }

        private static string[] GetCurrentDefines()
        {
            var buildTarget = EditorUserBuildSettings.selectedBuildTargetGroup;
            PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget, out var defines);
            return defines;
        }

        public static void ApplyBuildType(ReflectiveBuildConfig config)
        {
            var buildTarget = EditorUserBuildSettings.selectedBuildTargetGroup;
            PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget, out var currentDefines);

            // Remove existing reflective symbols
            var filteredDefines = currentDefines.Where(d => !REFLECTIVE_SYMBOLS.Contains(d)).ToList();

            // Add new symbols based on build type
            var newSymbols = config.GetDefineSymbols();
            filteredDefines.AddRange(newSymbols);

            // Apply
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget, filteredDefines.ToArray());

            Debug.Log($"[ReflectiveBuildConfig] Applied {config.BuildType} build type. Defines: {string.Join(", ", newSymbols)}");
        }

        [MenuItem("Reflective/Open Build Config")]
        public static void OpenBuildConfig()
        {
            var config = ReflectiveBuildConfig.GetOrCreateConfig();
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
        }

        [MenuItem("Reflective/Apply Build Config")]
        public static void ApplyBuildConfigMenu()
        {
            var config = ReflectiveBuildConfig.GetOrCreateConfig();
            ApplyBuildType(config);
        }
    }
}

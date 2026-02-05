using UnityEngine;
using UnityEditor;
using REFLECTIVE.Runtime.Configuration;

namespace REFLECTIVE.Editor.Configuration
{
    /// <summary>
    /// Automatically creates build config on first import and shows setup dialog.
    /// </summary>
    [InitializeOnLoad]
    public static class ReflectiveBuildConfigInitializer
    {
        private const string INITIALIZED_KEY = "ReflectiveRM_BuildConfig_Initialized";

        static ReflectiveBuildConfigInitializer()
        {
            EditorApplication.delayCall += CheckFirstTimeSetup;
        }

        private static void CheckFirstTimeSetup()
        {
            // Skip if already initialized
            if (SessionState.GetBool(INITIALIZED_KEY, false))
                return;

            SessionState.SetBool(INITIALIZED_KEY, true);

            // Check if config exists
            var config = ReflectiveBuildConfig.Instance;
            if (config != null)
            {
                // Config exists, ensure defines are applied
                ReflectiveBuildConfigEditor.ApplyBuildType(config);
                return;
            }

            // First time setup - show dialog
            var result = EditorUtility.DisplayDialogComplex(
                "Reflective Room Manager Setup",
                "Welcome to Reflective Room Manager!\n\n" +
                "A build configuration is required. Which build type would you like to use?\n\n" +
                "• Host-Client: Both server and client code (recommended for development)\n" +
                "• Dedicated Server: Server-only code\n" +
                "• Pure Client: Client-only code\n\n" +
                "You can change this later in the config asset.",
                "Host-Client (Recommended)",
                "Dedicated Server",
                "Pure Client");

            var buildType = result switch
            {
                0 => ReflectiveBuildType.HostClient,
                1 => ReflectiveBuildType.DedicatedServer,
                2 => ReflectiveBuildType.PureClient,
                _ => ReflectiveBuildType.HostClient
            };

            CreateConfigWithBuildType(buildType);
        }

        private static void CreateConfigWithBuildType(ReflectiveBuildType buildType)
        {
            // Ensure Resources folder exists
            var resourcesPath = "Assets/ReflectiveRoomManager/Resources";
            if (!AssetDatabase.IsValidFolder(resourcesPath))
            {
                var parentPath = "Assets/ReflectiveRoomManager";
                if (!AssetDatabase.IsValidFolder(parentPath))
                {
                    AssetDatabase.CreateFolder("Assets", "ReflectiveRoomManager");
                }
                AssetDatabase.CreateFolder(parentPath, "Resources");
            }

            // Create config
            var config = ScriptableObject.CreateInstance<ReflectiveBuildConfig>();

            // Set build type via serialized object
            var so = new SerializedObject(config);
            so.FindProperty("_buildType").enumValueIndex = (int)buildType;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Save asset
            var assetPath = $"{resourcesPath}/ReflectiveBuildConfig.asset";
            AssetDatabase.CreateAsset(config, assetPath);
            AssetDatabase.SaveAssets();

            // Apply defines
            ReflectiveBuildConfigEditor.ApplyBuildType(config);

            // Select the config
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);

            Debug.Log($"[ReflectiveRoomManager] Created build config with {buildType} build type at {assetPath}");

            EditorUtility.DisplayDialog(
                "Setup Complete",
                $"Build configuration created with {buildType} build type.\n\n" +
                "The config asset has been selected in the Project window.\n" +
                "You can change the build type at any time from the inspector.",
                "OK");
        }

        [MenuItem("Reflective/Reset Build Config")]
        public static void ResetBuildConfig()
        {
            var config = ReflectiveBuildConfig.Instance;
            if (config != null)
            {
                var path = AssetDatabase.GetAssetPath(config);
                AssetDatabase.DeleteAsset(path);
            }

            SessionState.SetBool(INITIALIZED_KEY, false);
            CheckFirstTimeSetup();
        }
    }
}

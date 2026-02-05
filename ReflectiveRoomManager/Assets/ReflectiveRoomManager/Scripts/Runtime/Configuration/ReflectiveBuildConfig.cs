using UnityEngine;

namespace REFLECTIVE.Runtime.Configuration
{
    /// <summary>
    /// Build type for Reflective Room Manager.
    /// Determines which code paths are included in the build.
    /// </summary>
    public enum ReflectiveBuildType
    {
        /// <summary>
        /// Server + Client code. Use for listen servers or development.
        /// </summary>
        HostClient = 0,

        /// <summary>
        /// Server-only code. Use for dedicated server builds.
        /// </summary>
        DedicatedServer = 1,

        /// <summary>
        /// Client-only code. Use for client builds that connect to dedicated servers.
        /// </summary>
        PureClient = 2
    }

    /// <summary>
    /// Configuration asset for Reflective Room Manager build settings.
    /// Create via: Assets > Create > Reflective > Build Config
    /// </summary>
    [CreateAssetMenu(fileName = "ReflectiveBuildConfig", menuName = "Reflective/Build Config", order = 0)]
    public class ReflectiveBuildConfig : ScriptableObject
    {
        private const string CONFIG_PATH = "ReflectiveBuildConfig";

        [Header("Build Configuration")]
        [Tooltip("Select the build type for your project.\n\n" +
                 "• Host-Client: Both server and client code (default)\n" +
                 "• Dedicated Server: Server-only code\n" +
                 "• Pure Client: Client-only code")]
        [SerializeField]
        private ReflectiveBuildType _buildType = ReflectiveBuildType.HostClient;

        [Header("Platform Override")]
        [Tooltip("When enabled, Dedicated Server platform automatically uses DedicatedServer build type.")]
        [SerializeField]
        private bool _autoDetectServerPlatform = true;

        /// <summary>
        /// Current build type setting.
        /// </summary>
        public ReflectiveBuildType BuildType => _buildType;

        /// <summary>
        /// Whether to auto-detect server platform.
        /// </summary>
        public bool AutoDetectServerPlatform => _autoDetectServerPlatform;

        /// <summary>
        /// Scripting define symbols for current build type.
        /// </summary>
        public string[] GetDefineSymbols()
        {
            return _buildType switch
            {
                ReflectiveBuildType.DedicatedServer => new[] { "REFLECTIVE_SERVER" },
                ReflectiveBuildType.PureClient => new[] { "REFLECTIVE_CLIENT" },
                ReflectiveBuildType.HostClient => new[] { "REFLECTIVE_SERVER", "REFLECTIVE_CLIENT" },
                _ => new[] { "REFLECTIVE_SERVER", "REFLECTIVE_CLIENT" }
            };
        }

        private static ReflectiveBuildConfig _instance;

        /// <summary>
        /// Gets the config instance from Resources folder.
        /// </summary>
        public static ReflectiveBuildConfig Instance
        {
            get
            {
                if (_instance == null)
                    _instance = Resources.Load<ReflectiveBuildConfig>(CONFIG_PATH);

                return _instance;
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor-only: Finds or creates the config asset.
        /// </summary>
        public static ReflectiveBuildConfig GetOrCreateConfig()
        {
            var config = Instance;

            if (config == null)
            {
                config = CreateInstance<ReflectiveBuildConfig>();

                var resourcesPath = "Assets/ReflectiveRoomManager/Resources";
                if (!System.IO.Directory.Exists(resourcesPath))
                    System.IO.Directory.CreateDirectory(resourcesPath);

                UnityEditor.AssetDatabase.CreateAsset(config, $"{resourcesPath}/{CONFIG_PATH}.asset");
                UnityEditor.AssetDatabase.SaveAssets();

                Debug.Log($"[ReflectiveBuildConfig] Created config at {resourcesPath}/{CONFIG_PATH}.asset");
            }

            return config;
        }
#endif
    }
}

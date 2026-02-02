using Mirror;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace REFLECTIVE.Editor.NETWORK.Manager
{
    using Utilities;
    using Runtime.NETWORK.Manager;

    [CustomEditor(typeof(ReflectiveNetworkManager), true)]
    public class ReflectiveNetworkManagerEditor : ReflectiveEditorBase
    {
        private NetworkManager _networkManager;
        private ReflectiveNetworkManager _reflectiveNetworkManager;

        private DefaultAsset _spawnablePrefabsFolderAsset;

        private ListView _spawnListView;
        private SerializedProperty _spawnListProperty;

        protected override string GetTitle() => "REFLECTIVE NETWORK MANAGER";

        protected override void BuildInspectorUI(VisualElement root)
        {
            _networkManager = target as NetworkManager;
            _reflectiveNetworkManager = _networkManager as ReflectiveNetworkManager;

            if (!string.IsNullOrEmpty(_reflectiveNetworkManager.SpawnablePrefabsPath))
            {
                _spawnablePrefabsFolderAsset =
                    AssetDatabase.LoadAssetAtPath<DefaultAsset>(_reflectiveNetworkManager.SpawnablePrefabsPath);
            }

            AddDefaultProperties(root);

            root.Add(CreateSectionHeader("SPAWNABLE PREFABS"));

            var spawnBody = CreateSectionBody();

            _spawnListProperty = serializedObject.FindProperty("spawnPrefabs");

            var autoSearchProp = serializedObject.FindProperty("IsAutoPrefabSearcher");
            var autoSearchField = new PropertyField(autoSearchProp);
            autoSearchField.Bind(serializedObject);
            spawnBody.Add(autoSearchField);

            var manualSection = new VisualElement();

            var folderSearchProp = serializedObject.FindProperty("IsFolderSearch");
            var folderSearchField = new PropertyField(folderSearchProp);
            folderSearchField.Bind(serializedObject);
            manualSection.Add(folderSearchField);

            var folderSection = new VisualElement();
            folderSection.AddToClassList("folder-section");

            var folderField = new ObjectField("Folder")
            {
                objectType = typeof(DefaultAsset),
                value = _spawnablePrefabsFolderAsset
            };

            folderField.RegisterValueChangedCallback(evt =>
            {
                var asset = evt.newValue as DefaultAsset;

                if (asset != null)
                {
                    var assetPath = AssetDatabase.GetAssetPath(asset);

                    if (AssetDatabase.IsValidFolder(assetPath))
                    {
                        _spawnablePrefabsFolderAsset = asset;
                        _reflectiveNetworkManager.SpawnablePrefabsPath = assetPath;
                        LoadPrefabsFromFolder();
                        RefreshSpawnList();
                    }
                    else
                    {
                        Debug.LogWarning("This is not folder. You can only select folder!");
                        folderField.SetValueWithoutNotify(_spawnablePrefabsFolderAsset);
                    }
                }

                serializedObject.ApplyModifiedProperties();
            });

            folderSection.Add(folderField);
            manualSection.Add(folderSection);

            BuildSpawnListView();
            manualSection.Add(_spawnListView);

            var refreshBtn = CreateButton("Refresh List", () =>
            {
                LoadPrefabsFromFolder();
                RefreshSpawnList();
                EditorUtility.SetDirty(target);
            });

            var populateBtn = CreateButton("Populate Spawnable Prefabs", () =>
            {
                ScanForNetworkIdentities();
                RefreshSpawnList();
            });

            manualSection.Add(refreshBtn);
            manualSection.Add(populateBtn);

            spawnBody.Add(manualSection);
            root.Add(spawnBody);

            UpdateVisibility(manualSection, folderSection, refreshBtn, populateBtn);

            autoSearchField.RegisterValueChangeCallback(_ =>
            {
                serializedObject.ApplyModifiedProperties();
                UpdateVisibility(manualSection, folderSection, refreshBtn, populateBtn);
            });

            folderSearchField.RegisterValueChangeCallback(_ =>
            {
                serializedObject.ApplyModifiedProperties();
                UpdateVisibility(manualSection, folderSection, refreshBtn, populateBtn);
            });

            if (_reflectiveNetworkManager.IsFolderSearch &&
                !string.IsNullOrEmpty(_reflectiveNetworkManager.SpawnablePrefabsPath))
            {
                LoadPrefabsFromFolder();
            }
        }

        private void UpdateVisibility(VisualElement manualSection, VisualElement folderSection,
            Button refreshBtn, Button populateBtn)
        {
            var isAuto = _reflectiveNetworkManager.IsAutoPrefabSearcher;
            var isFolder = _reflectiveNetworkManager.IsFolderSearch;

            manualSection.style.display = isAuto ? DisplayStyle.None : DisplayStyle.Flex;
            folderSection.style.display = isFolder ? DisplayStyle.Flex : DisplayStyle.None;
            refreshBtn.style.display = isFolder ? DisplayStyle.Flex : DisplayStyle.None;
            populateBtn.style.display = isFolder ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private void BuildSpawnListView()
        {
            _spawnListView = new ListView
            {
                reorderable = true,
                showAddRemoveFooter = true,
                showBorder = true,
                showFoldoutHeader = true,
                headerTitle = "Registered Spawnable Prefabs",
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight
            };

            _spawnListView.AddToClassList("spawn-list");

            _spawnListView.makeItem = () =>
            {
                var field = new ObjectField { objectType = typeof(GameObject) };
                return field;
            };

            _spawnListView.bindItem = (element, index) =>
            {
                serializedObject.Update();

                if (index >= _spawnListProperty.arraySize) return;

                var field = (ObjectField)element;
                var prop = _spawnListProperty.GetArrayElementAtIndex(index);
                var go = (GameObject)prop.objectReferenceValue;

                field.SetValueWithoutNotify(go);

                field.RegisterValueChangedCallback(evt =>
                {
                    var newGo = evt.newValue as GameObject;

                    if (newGo != null && !newGo.GetComponent<NetworkIdentity>())
                    {
                        Debug.LogError(
                            $"Prefab {newGo} cannot be added as spawnable as it doesn't have a NetworkIdentity.");
                        field.SetValueWithoutNotify(go);
                        return;
                    }

                    serializedObject.Update();
                    if (index < _spawnListProperty.arraySize)
                    {
                        _spawnListProperty.GetArrayElementAtIndex(index).objectReferenceValue = newGo;
                        serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(target);
                    }
                });
            };

            _spawnListView.itemsSource = _networkManager.spawnPrefabs;

            _spawnListView.itemsAdded += _ =>
            {
                EditorUtility.SetDirty(target);
            };

            _spawnListView.itemsRemoved += _ =>
            {
                EditorUtility.SetDirty(target);
            };

            _spawnListView.itemIndexChanged += (_, _) =>
            {
                EditorUtility.SetDirty(target);
            };
        }

        private void RefreshSpawnList()
        {
            serializedObject.Update();

            if (_spawnListView != null)
            {
                _spawnListView.itemsSource = _networkManager.spawnPrefabs;
                _spawnListView.Rebuild();
            }
        }

        private void LoadPrefabsFromFolder()
        {
            if (string.IsNullOrEmpty(_reflectiveNetworkManager.SpawnablePrefabsPath)) return;

            var identities = new List<GameObject>();

            try
            {
                var assetPaths = IterateOverProject("t:prefab", _reflectiveNetworkManager.SpawnablePrefabsPath)
                    .ToArray();

                foreach (var assetPath in assetPaths)
                {
                    var ni = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

                    if (!ni) continue;

                    if (_networkManager.playerPrefab == ni.gameObject) continue;

                    identities.Add(ni.gameObject);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();

                Undo.RecordObject(_networkManager, "NetworkManager: populated prefabs");

                _networkManager.spawnPrefabs = new List<GameObject>(identities);
                _networkManager.spawnPrefabs = _networkManager.spawnPrefabs.OrderBy(go => go.name).ToList();

                EditorUtility.SetDirty(target);

                Resources.UnloadUnusedAssets();
            }
        }

        private void ScanForNetworkIdentities()
        {
            const int batchSize = 50;

            var identities = new List<GameObject>();
            var cancelled = false;

            try
            {
                var paths = EditorHelper.IterateOverProject("t:prefab").ToArray();
                var count = 0;

                for (var i = 0; i < paths.Length; i += batchSize)
                {
                    var batchEnd = Mathf.Min(i + batchSize, paths.Length);

                    for (var j = i; j < batchEnd; j++)
                    {
                        var path = paths[j];

                        if (path.Contains("Mirror/Tests/") ||
                            path.Contains("Mirror/Examples/"))
                        {
                            continue;
                        }

                        if (EditorUtility.DisplayCancelableProgressBar("Searching for NetworkIdentities..",
                                $"Scanned {count}/{paths.Length} prefabs. Found {identities.Count} new ones",
                                count / (float)paths.Length))
                        {
                            cancelled = true;
                            break;
                        }

                        count++;

                        var ni = AssetDatabase.LoadAssetAtPath<NetworkIdentity>(path);

                        if (!ni) continue;

                        if (!_networkManager.spawnPrefabs.Contains(ni.gameObject))
                        {
                            identities.Add(ni.gameObject);
                        }
                    }

                    if (cancelled) break;

                    Resources.UnloadUnusedAssets();
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();

                if (!cancelled)
                {
                    Undo.RecordObject(_networkManager, "NetworkManager: populated prefabs");

                    _networkManager.spawnPrefabs.AddRange(identities);
                    _networkManager.spawnPrefabs = _networkManager.spawnPrefabs.OrderBy(go => go.name).ToList();

                    EditorUtility.SetDirty(target);
                }

                Resources.UnloadUnusedAssets();
            }
        }

        private static IEnumerable<string> IterateOverProject(string filter, string folder)
        {
            return AssetDatabase.FindAssets(filter, new[] { folder }).Select(AssetDatabase.GUIDToAssetPath);
        }
    }
}

using Mirror;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

namespace REFLECTIVE.Editor.NETWORK.Manager
{
    using Utilities;
    
    using Runtime.NETWORK.Manager;
    
    [CustomEditor(typeof(ReflectiveNetworkManager), true)]
    public class ReflectiveNetworkManagerEditor : UnityEditor.Editor
    {
        private SerializedProperty _spawnListProperty;
        private ReorderableList _spawnList;
        
        protected NetworkManager m_networkManager;
        protected ReflectiveNetworkManager m_reflectiveNetworkManager;

        private DefaultAsset _spawnablePrefabsFolderAsset;

        private void OnEnable()
        {
            Init();

            if (!string.IsNullOrEmpty(m_reflectiveNetworkManager.SpawnablePrefabsPath))
            {
                _spawnablePrefabsFolderAsset =
                    AssetDatabase.LoadAssetAtPath<DefaultAsset>(m_reflectiveNetworkManager.SpawnablePrefabsPath);
                
                if (m_reflectiveNetworkManager.IsFolderSearch)
                {
                    LoadFindedPrefabsFromFolder();   
                }
            }
        }
        
        protected void Init()
        {
            m_networkManager = target as NetworkManager;
            m_reflectiveNetworkManager = m_networkManager as ReflectiveNetworkManager;
            
            CreateSpawnList();
        }

        private void CreateSpawnList()
        {
            _spawnListProperty = serializedObject.FindProperty("spawnPrefabs");
                
            _spawnList = new ReorderableList(serializedObject, _spawnListProperty)
            {
                drawHeaderCallback = DrawHeader,
                drawElementCallback = DrawChild,
                onReorderCallback = Changed,
                onRemoveCallback = RemoveButton,
                onChangedCallback = Changed,
                onAddCallback = AddButton,
                // this uses a 16x16 icon. other sizes make it stretch.
                elementHeight = 16 
            };
        }

        public override void OnInspectorGUI()
        {
            CustomEditorUtilities.DrawReflectionTitle("REFLECTIVE NETWORK MANAGER");
            
            CustomEditorUtilities.DrawDefaultInspector(serializedObject);
            
            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Spawnable Prefabs Settings",new GUIStyle(EditorStyles.boldLabel));

            EditorGUI.BeginChangeCheck();
            
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("IsAutoPrefabSearcher"));

            if (!m_reflectiveNetworkManager.IsAutoPrefabSearcher)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("IsFolderSearch"));

                if (m_reflectiveNetworkManager.IsFolderSearch)
                {
                    FindPrefabsFromFolder();
                }

                _spawnList.DoLayoutList();

                if(m_reflectiveNetworkManager.IsFolderSearch)
                {
                    if (GUILayout.Button("Refresh List"))
                    {
                        LoadFindedPrefabsFromFolder();
                        EditorUtility.SetDirty(target);
                    }
                }
                else
                {
                    if (GUILayout.Button("Populate Spawnable Prefabs"))
                    {
                        ScanForNetworkIdentities();
                    }
                }   
            }
            
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void FindPrefabsFromFolder()
        {
            EditorGUI.BeginChangeCheck();
            
            _spawnablePrefabsFolderAsset = (DefaultAsset)EditorGUILayout.ObjectField("Folder: ", _spawnablePrefabsFolderAsset, typeof(DefaultAsset), true);

            if (EditorGUI.EndChangeCheck())
            {
                if (_spawnablePrefabsFolderAsset != null)
                {
                    var assetPath = AssetDatabase.GetAssetPath(_spawnablePrefabsFolderAsset);

                    if (AssetDatabase.IsValidFolder(assetPath))
                    {
                        m_reflectiveNetworkManager.SpawnablePrefabsPath = assetPath;
                        
                        LoadFindedPrefabsFromFolder();
                    }
                    else
                    {
                        Debug.LogWarning("This is not folder. You can only select folder!");
                        _spawnablePrefabsFolderAsset = null;
                    }
                }
                
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void LoadFindedPrefabsFromFolder()
        {
            if (string.IsNullOrEmpty(m_reflectiveNetworkManager.SpawnablePrefabsPath)) return;
            
            var identities = new List<GameObject>();

            try
            {
                var assetPaths = IterateOverProject("t:prefab", m_reflectiveNetworkManager.SpawnablePrefabsPath).ToArray();
                
                foreach (var assetPath in assetPaths)
                {
                    var ni = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    
                    if (ni == null) continue;
                    
                    if (m_networkManager.playerPrefab == ni.gameObject) continue;
                    
                    identities.Add(ni.gameObject);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                
                // RecordObject is needed for "*" to show up in Scene.
                // however, this only saves List.Count without the entries.
                Undo.RecordObject(m_networkManager, "NetworkManager: populated prefabs");

                // set the entries
                m_networkManager.spawnPrefabs = new List<GameObject>(identities);

                // sort alphabetically for better UX
                m_networkManager.spawnPrefabs = m_networkManager.spawnPrefabs.OrderBy(go => go.name).ToList();

                // SetDirty is required to save the individual entries properly.
                EditorUtility.SetDirty(target);
                
                // Loading assets might use a lot of memory, so try to unload them after
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

                        // ignore test & example prefabs.
                        // users sometimes keep the folders in their projects.
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

                        if (!ni)
                        {
                            continue;
                        }

                        if (!m_networkManager.spawnPrefabs.Contains(ni.gameObject))
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
                    // RecordObject is needed for "*" to show up in Scene.
                    // however, this only saves List.Count without the entries.
                    Undo.RecordObject(m_networkManager, "NetworkManager: populated prefabs");

                    // add the entries
                    m_networkManager.spawnPrefabs.AddRange(identities);

                    // sort alphabetically for better UX
                    m_networkManager.spawnPrefabs = m_networkManager.spawnPrefabs.OrderBy(go => go.name).ToList();

                    // SetDirty is required to save the individual entries properly.
                    EditorUtility.SetDirty(target);
                }
                // Loading assets might use a lot of memory, so try to unload them after
                Resources.UnloadUnusedAssets();
            }
        }

        private static void DrawHeader(Rect headerRect)
        {
            GUI.Label(headerRect, "Registered Spawnable Prefabs:");
        }

        internal void DrawChild(Rect r, int index, bool isActive, bool isFocused)
        {
            var prefab = _spawnListProperty.GetArrayElementAtIndex(index);
            var go = (GameObject)prefab.objectReferenceValue;

            GUIContent label;
            
            if (go == null)
            {
                label = new GUIContent("Empty", "Drag a prefab with a NetworkIdentity here");
            }
            else
            {
                var identity = go.GetComponent<NetworkIdentity>();
                label = new GUIContent(go.name, identity != null ? $"AssetId: [{identity.assetId}]" : "No Network Identity");
            }

            var newGameObject = (GameObject)EditorGUI.ObjectField(r, label, go, typeof(GameObject), false);

            if (newGameObject != go)
            {
                if (newGameObject != null && !newGameObject.GetComponent<NetworkIdentity>())
                {
                    Debug.LogError($"Prefab {newGameObject} cannot be added as spawnable as it doesn't have a NetworkIdentity.");
                    return;
                }
                prefab.objectReferenceValue = newGameObject;
            }
        }

        internal void Changed(ReorderableList list)
        {
            EditorUtility.SetDirty(target);
        }

        internal void AddButton(ReorderableList list)
        {
            _spawnListProperty.arraySize += 1;
            list.index = _spawnListProperty.arraySize - 1;

            var obj = _spawnListProperty.GetArrayElementAtIndex(_spawnListProperty.arraySize - 1);
            obj.objectReferenceValue = null;

            _spawnList.index = _spawnList.count - 1;

            Changed(list);
        }

        internal void RemoveButton(ReorderableList list)
        {
            _spawnListProperty.DeleteArrayElementAtIndex(_spawnList.index);
            if (list.index >= _spawnListProperty.arraySize)
            {
                list.index = _spawnListProperty.arraySize - 1;
            }
        }
        
        private static IEnumerable<string> IterateOverProject(string filter, string folder)
        {
            return AssetDatabase.FindAssets(filter, new[]{folder}).Select(AssetDatabase.GUIDToAssetPath);
        }
    }
}
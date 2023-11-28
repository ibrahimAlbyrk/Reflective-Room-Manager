using Mirror;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace REFLECTIVE.Editor.NETWORK.Room.Manager
{
    using Runtime.NETWORK.Room;
    using Runtime.NETWORK.Room.Loader;
    using Runtime.NETWORK.Room.Service;

    [CustomEditor(typeof(RoomManagerBase), true)]
    public class RoomManagerBaseEditor : UnityEditor.Editor
    {
        private GUIStyle roomInfoStyle;
        private GUIStyle foldoutStyle;
        private GUIStyle buttonStyle;
        
        private bool showRoomList;
        private bool[] roomFoldouts;

        private int searchType;
        
        private string searchFilter = "";

        private Vector2 scrollPosition = new(0, 0);

        public override void OnInspectorGUI()
        {
            var roomManager = (RoomManagerBase)target;

            if (!roomManager.gameObject.TryGetComponent(out SceneInterestManagement _))
            {
                if(roomManager.RoomLoaderType == RoomLoaderType.AdditiveScene)
                {
                    EditorGUILayout.HelpBox("interest manager is not attached! If scene interest management is not added while additive room mode is selected, players may interfere with each other.", MessageType.Warning);
                }
            }

            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("You cannot make changes during runtime", MessageType.Info);
                
                GUI.enabled = false;
            }
            
            DrawDefaultInspector(); // Draw the default inspector
            
            GUI.enabled = true;

            SetStyles();
            
            var rooms = roomManager.GetRooms().ToArray();

            if (roomFoldouts == null || roomFoldouts.Length != rooms.Length)
                roomFoldouts = new bool[rooms.Length];

            GUILayout.Space(10);
            
            // Room list visibility toggle
            showRoomList = EditorGUILayout.Foldout(showRoomList, $"Room List ({rooms.Length})", foldoutStyle);

            if (showRoomList)
            {
                // Search bar for filtering rooms
                GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
                
                GUILayout.Label("Search:", GUILayout.ExpandWidth(false));
                
                searchFilter = GUILayout.TextField(searchFilter, GUI.skin.FindStyle("ToolbarSearchTextField"));
                
                GUILayout.Label("Search Type:", GUILayout.ExpandWidth(false));
                
                var options = new[] {"name", "Client ID"};
                
                searchType = EditorGUILayout.Popup(searchType, options, GUILayout.Width(75));

                if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSearchCancelButton")))
                {
                    searchFilter = "";
                    GUI.FocusControl(null);
                }

                GUILayout.EndHorizontal();

                scrollPosition = GUILayout.BeginScrollView(scrollPosition);

                for (var i = 0; i < rooms.Length; i++)
                {
                    var room = rooms[i];
                    
                    switch (searchType)
                    {
                        case 0 when !string.IsNullOrEmpty(searchFilter):
                            if(!room.RoomName.Contains(searchFilter))
                                continue;
                            
                            break;
                        case 1 when !string.IsNullOrEmpty(searchFilter):
                            if(int.TryParse(searchFilter, out var id))
                                if(room.Connections.All(conn => conn.connectionId != id))
                                    continue;
                            break;
                    }

                    // Add a background color for room info
                    GUI.backgroundColor = new Color32(70, 70, 70, 255);
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    GUI.backgroundColor = Color.white;

                    GUILayout.BeginHorizontal();
                    
                    roomFoldouts[i] = EditorGUILayout.Foldout(roomFoldouts[i], $"Room Name: {room.RoomName}");

                    var removeButtonStyle = new GUIStyle(EditorStyles.toolbarButton)
                    {
                        fixedWidth = 100
                    };
                    
                    GUILayout.Label($"{room.CurrentPlayers}/{room.MaxPlayers}", roomInfoStyle);
                    
                    if (GUILayout.Button("Remove", removeButtonStyle))
                        RoomServer.RemoveRoom(room.RoomName, true);
                    
                    GUILayout.EndHorizontal();
                    
                    if (roomFoldouts[i])
                    {
                        GUILayout.Label($"Some Infos (soon)", EditorStyles.label);
                    }

                    EditorGUILayout.EndVertical();
                }

                // Ends the scrolling view
                GUILayout.EndScrollView();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Remove All Rooms", buttonStyle))
                RoomServer.RemoveAllRoom(true);
        }

        private void SetStyles()
        {
            foldoutStyle = new GUIStyle(EditorStyles.foldoutHeader)
            {
                fontSize = 15,
            };
            
            roomInfoStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleRight
            };
            
            buttonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold
            };
        }
    }
}
using Mirror;
using System.Linq;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.GUI
{
    using Service;
    using Structs;
    
    [AddComponentMenu("REFLECTIVE/Network Room Manager HUD")]
    public class REFLECTIVE_RoomManagerHUD : MonoBehaviour
    {
        private static string _roomNameField = "Room Name";
        private static string _maxPlayers = "Max Player";

        private static bool _isServer;

        private static bool _showingRoomList;

        private static Vector2 _scrollPosition;
        
        private static GUIStyle backgroundStyle;
        
        protected virtual void OnGUI()
        {
            if (!NetworkClient.active && !NetworkServer.active) return;

            _isServer = !NetworkClient.isConnected && NetworkServer.active;
            
            var roomManager = REFLECTIVE_BaseRoomManager.Singleton;
            
            if (!roomManager) return;

            if (!_isServer)
            {
                var currentRoom = roomManager.GetRoomOfClient();
                
                if (!string.IsNullOrEmpty(currentRoom.Name))
                {
                    ShowCurrentRoom(currentRoom);
                    return;
                }   
            }
            
            if (_showingRoomList)
            {
                ShowRoomList();
                return;
            }
            
            ShowRoomButtons();
        }

        private static void ShowRoomButtons()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 230f, 30, 200f, 100f));
            
            GUILayout.BeginVertical();
            
            _roomNameField = GUILayout.TextField(_roomNameField,
                GUILayout.MinWidth(20));
            _maxPlayers = GUILayout.TextField(_maxPlayers,
                GUILayout.MinWidth(2));
            
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Create Room"))
            {
                var roomInfo = new REFLECTIVE_RoomInfo
                {
                    Name = _roomNameField,
                    SceneName = "Game_Scene",
                    MaxPlayers = int.TryParse(_maxPlayers, out var result) ? result : 2
                };
                
                if(_isServer)
                    REFLECTIVE_RoomServer.CreateRoom(roomInfo);
                else
                    REFLECTIVE_RoomClient.CreateRoom(roomInfo);
            }

            if (!_isServer)
            {
                if (GUILayout.Button("Join Room"))
                {
                    REFLECTIVE_RoomClient.JoinRoom(_roomNameField);
                }
                
                GUILayout.EndHorizontal();
                
                if (GUILayout.Button("Show Rooms"))
                {
                    _showingRoomList = true;
                }
            }
            else
            {
                if (GUILayout.Button("Show Rooms"))
                {
                    _showingRoomList = true;
                }
                
                GUILayout.EndHorizontal();
            }
            
            GUILayout.EndVertical();
            
            GUILayout.EndArea();
        }

        private static void ShowCurrentRoom(REFLECTIVE_RoomInfo roomInfo)
        {
            GUILayout.BeginArea(new Rect(Screen.width - 230f, 30, 200f, 200f));
            
            GUILayout.Label($"Room Name : {roomInfo.Name}");
            GUILayout.Label($"Max Player Count : {roomInfo.MaxPlayers}");
            GUILayout.Label($"Current Player Count : {roomInfo.CurrentPlayers}");
            
            if (GUILayout.Button("Exit Room"))
            {
                REFLECTIVE_RoomClient.ExitRoom();
            }
            
            GUILayout.EndArea();
        }

        private static void ShowRoomList()
        {
            backgroundStyle ??= new GUIStyle
            {
                normal =
                {
                    background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.5f))
                }
            };
            
            GUILayout.BeginArea(new Rect(Screen.width - 230f, 30, 200f, Screen.height - 30));

            GUILayout.BeginVertical();
            
            if (GUILayout.Button("Close Rooms"))
                _showingRoomList = false;

            //TODO: room list for client
            if (_isServer)
            {
                var rooms = REFLECTIVE_BaseRoomManager.Singleton.GetRooms().ToList();
            
                var height = Mathf.Min(rooms.Count * 25f, Screen.height - 25);
            
                UnityEngine.GUI.Box(new Rect(0, 25, 200f, height), "", backgroundStyle);
                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            
                foreach (var room in rooms.Where(room => GUILayout.Button($"{room.RoomName} - {room.CurrentPlayers}/{room.MaxPlayers}")))
                {
                    REFLECTIVE_RoomServer.RemoveRoom(room.RoomName, forced:true);
                }
            }
            else
            {
                var rooms = REFLECTIVE_BaseRoomManager.Singleton.GetRoomInfos().ToList();
            
                var height = Mathf.Min(rooms.Count * 25f, Screen.height - 25);
            
                UnityEngine.GUI.Box(new Rect(0, 25, 200f, height), "", backgroundStyle);
                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            
                foreach (var room in rooms.Where(room => GUILayout.Button($"{room.Name} - {room.CurrentPlayers}/{room.MaxPlayers}")))
                {
                    REFLECTIVE_RoomClient.JoinRoom(room.Name);
                } 
            }
            
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            
            GUILayout.EndArea();
        }
        
        private static Texture2D MakeTex(int width, int height, Color color)
        {
            var pix = new Color[width * height];
   
            for (var i = 0; i < pix.Length; ++i) 
            {
                pix[i] = color;
            }
            
            var result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
   
            return result;
        }
    }
}
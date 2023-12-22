using System.Collections.Generic;
using Mirror;
using System.Linq;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.GUI
{
    using Service;
    using Structs;
    
    [AddComponentMenu("REFLECTIVE/Network Room Manager HUD")]
    public class RoomManagerHUD : MonoBehaviour
    {
        [SerializeField] private float offsetX;
        [SerializeField] private float offsetY;
        
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
            
            var roomManager = RoomManagerBase.Instance;
            
            if (!roomManager) return;

            if (!_isServer)
            {
                var currentRoom = roomManager.GetRoomOfClient();
                
                if (!string.IsNullOrEmpty(currentRoom.RoomName))
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

        private void ShowRoomButtons()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 230f + offsetX, 30 + offsetY, 200f, 100f));
            
            GUILayout.BeginVertical();
            
            _roomNameField = GUILayout.TextField(_roomNameField,
                GUILayout.MinWidth(20));
            _maxPlayers = GUILayout.TextField(_maxPlayers,
                GUILayout.MinWidth(2));
            
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Create Room"))
            {
                var roomInfo = new RoomInfo
                {
                    RoomName = _roomNameField,
                    SceneName = RoomManagerBase.Instance.RoomScene,
                    MaxPlayers = int.TryParse(_maxPlayers, out var result) ? result : 2,
                };
                
                if(_isServer)
                    RoomServer.CreateRoom(roomInfo);
                else
                    RoomClient.CreateRoom(roomInfo.RoomName, roomInfo.SceneName, roomInfo.MaxPlayers);
            }

            if (!_isServer)
            {
                if (GUILayout.Button("Join Room"))
                {
                    RoomClient.JoinRoom(_roomNameField);
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

        private void ShowCurrentRoom(RoomInfo roomInfo)
        {
            GUILayout.BeginArea(new Rect(Screen.width - 230f + offsetX, 30 + offsetY, 200f, 200f));
            
            GUILayout.Label($"Room Name : {roomInfo.RoomName}");
            GUILayout.Label($"Max Player Count : {roomInfo.MaxPlayers}");
            GUILayout.Label($"Current Player Count : {roomInfo.CurrentPlayers}");
            
            if (GUILayout.Button("Exit Room"))
            {
                RoomClient.ExitRoom();
            }
            
            GUILayout.EndArea();
        }

        private void ShowRoomList()
        {
            backgroundStyle ??= new GUIStyle
            {
                normal =
                {
                    background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.5f))
                }
            };
            
            GUILayout.BeginArea(new Rect(Screen.width - 230f + offsetX, 30 + offsetY, 200f, Screen.height - 30));

            GUILayout.BeginVertical();
            
            if (GUILayout.Button("Close Rooms"))
                _showingRoomList = false;
            
            if (_isServer)
            {
                var rooms = RoomManagerBase.Instance.GetRooms().ToList();
            
                var height = Mathf.Min(rooms.Count * 25f, Screen.height - 25);
            
                UnityEngine.GUI.Box(new Rect(0, 25, 200f, height), "", backgroundStyle);
                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            
                foreach (var room in rooms.Where(room => GUILayout.Button($"{room.RoomName} - {room.CurrentPlayers}/{room.MaxPlayers}")))
                {
                    RoomServer.RemoveRoom(room.RoomName, forced:true);
                }
            }
            else
            {
                var rooms = RoomManagerBase.Instance.GetRoomInfos().ToList();
                
                var height = Mathf.Min(rooms.Count * 25f, Screen.height - 25);
            
                UnityEngine.GUI.Box(new Rect(0, 25, 200f, height), "", backgroundStyle);
                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            
                foreach (var room in rooms.Where(room => GUILayout.Button($"{room.RoomName} - {room.CurrentPlayers}/{room.MaxPlayers}")))
                {
                    RoomClient.JoinRoom(room.RoomName);
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
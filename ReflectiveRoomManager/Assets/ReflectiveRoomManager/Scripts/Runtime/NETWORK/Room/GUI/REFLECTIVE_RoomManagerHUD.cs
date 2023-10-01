using Mirror;
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
            }

            GUILayout.EndHorizontal();
            
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
    }
}
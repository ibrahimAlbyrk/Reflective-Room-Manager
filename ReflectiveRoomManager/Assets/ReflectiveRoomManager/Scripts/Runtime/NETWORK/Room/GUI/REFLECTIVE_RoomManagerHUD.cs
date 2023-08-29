using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.GUI
{
    using Structs;
    
    [AddComponentMenu("REFLECTIVE/Network Room Manager HUD")]
    public class REFLECTIVE_RoomManagerHUD : MonoBehaviour
    {
        private static string _roomNameField = "Room Name";
        private static string _maxPlayers = "Max Player";
        
        protected virtual void OnGUI()
        {
            if (!NetworkClient.isConnected || !NetworkClient.active) return;

            var roomManager = REFLECTIVE_BaseRoomManager.Instance;

            if (!roomManager) return;
            
            if (roomManager.IsStarted)
            {
                var currentRoom = roomManager.GetRoomOfPlayer(NetworkClient.connection);

                if (string.IsNullOrEmpty(currentRoom.RoomName)) return;
            
                ShowCurrentRoom(currentRoom);
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
                    MaxPlayers = int.TryParse(_maxPlayers, out var result) ? result : 2 
                };
                
                REFLECTIVE_BaseRoomManager.RequestCreateRoom(roomInfo);
            }
            
            if (GUILayout.Button("Join Room"))
            {
                REFLECTIVE_BaseRoomManager.RequestJoinRoom(_roomNameField);
            }
            
            GUILayout.EndHorizontal();
            
            GUILayout.EndVertical();
            
            GUILayout.EndArea();
        }

        private static void ShowCurrentRoom(REFLECTIVE_Room room)
        {
            GUILayout.BeginArea(new Rect(Screen.width - 590, 140, 520, 500));
            
            GUILayout.Label($"Room Name : {room.RoomName}");
            
            foreach (var player in room.Connections) 
            {
                GUILayout.Label($"Player: {player}"); //replace this with actual player display
            }
            
            GUILayout.EndArea();
        }
    }
}
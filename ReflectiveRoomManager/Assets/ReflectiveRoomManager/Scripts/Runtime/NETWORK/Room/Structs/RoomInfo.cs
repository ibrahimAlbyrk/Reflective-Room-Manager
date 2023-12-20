using System.Collections.Generic;

namespace REFLECTIVE.Runtime.NETWORK.Room.Structs
{
    [System.Serializable]
    public struct RoomInfo
    {
        public uint ID;
        
        public string RoomName;
        public string SceneName;
        
        public int MaxPlayers;
        public int CurrentPlayers;

        public Dictionary<string, string> CustomData;

        public RoomInfo(string roomName)
        {
            ID = 0;
            
            RoomName = roomName;
            SceneName = string.Empty;
            
            MaxPlayers = 0;
            
            CurrentPlayers = 0;

            CustomData = new Dictionary<string, string>();
        }

        public RoomInfo(string roomName, string sceneName, int maxPlayers)
        {
            ID = 0;
            
            RoomName = roomName;
            SceneName = sceneName;
            
            MaxPlayers = maxPlayers;
            
            CurrentPlayers = 0;
            
            CustomData = new Dictionary<string, string>();
        }
        
        public RoomInfo(string roomName, string sceneName, int maxPlayers, Dictionary<string, string> customData)
        {
            ID = 0;
            
            RoomName = roomName;
            SceneName = sceneName;
            
            MaxPlayers = maxPlayers;
            
            CurrentPlayers = 0;
            
            CustomData = customData;
        }
    }
}
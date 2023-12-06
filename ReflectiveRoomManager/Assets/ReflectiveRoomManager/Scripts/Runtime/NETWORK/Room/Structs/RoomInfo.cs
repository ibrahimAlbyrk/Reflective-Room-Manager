using System.Collections.Generic;

namespace REFLECTIVE.Runtime.NETWORK.Room.Structs
{
    [System.Serializable]
    public struct RoomInfo
    {
        public string RoomName;
        public string SceneName;
        
        public int MaxPlayers;
        public int CurrentPlayers;

        public List<int> ConnectionIds;

        public List<string> CustomDataKeys;
        public List<string> CustomDataValues;
        
        public RoomInfo(string roomName)
        {
            RoomName = roomName;
            SceneName = string.Empty;
            
            MaxPlayers = 0;
            
            CurrentPlayers = 0;
            
            ConnectionIds = new List<int>();
            
            CustomDataKeys = new List<string>();
            CustomDataValues = new List<string>();
        }

        public RoomInfo(string roomName, string sceneName, int maxPlayers)
        {
            RoomName = roomName;
            SceneName = sceneName;
            
            MaxPlayers = maxPlayers;
            
            CurrentPlayers = 0;
            
            ConnectionIds = new List<int>();
            
            CustomDataKeys = new List<string>();
            CustomDataValues = new List<string>();
        }
    }
}
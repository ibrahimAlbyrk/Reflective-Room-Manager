using System.Collections.Generic;
using System.Linq;

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

        public Dictionary<string, string> CustomData;

        public RoomInfo(string roomName)
        {
            RoomName = roomName;
            SceneName = string.Empty;
            
            MaxPlayers = 0;
            
            CurrentPlayers = 0;
            
            ConnectionIds = new List<int>();

            CustomData = new Dictionary<string, string>();
        }

        public RoomInfo(string roomName, string sceneName, int maxPlayers)
        {
            RoomName = roomName;
            SceneName = sceneName;
            
            MaxPlayers = maxPlayers;
            
            CurrentPlayers = 0;
            
            ConnectionIds = new List<int>();
            
            CustomData = new Dictionary<string, string>();
        }
        
        public RoomInfo(string roomName, string sceneName, int maxPlayers, Dictionary<string, string> customData)
        {
            RoomName = roomName;
            SceneName = sceneName;
            
            MaxPlayers = maxPlayers;
            
            CurrentPlayers = 0;
            
            ConnectionIds = new List<int>();
            
            CustomData = customData;
        }
    }
}
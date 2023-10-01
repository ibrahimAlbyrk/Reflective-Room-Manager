using System.Collections.Generic;

namespace REFLECTIVE.Runtime.NETWORK.Room.Structs
{
    [System.Serializable]
    public struct REFLECTIVE_RoomInfo
    {
        public string Name;
        public string SceneName;
        
        public int MaxPlayers;
        public int CurrentPlayers;

        public List<int> ConnectionIds;
        
        public REFLECTIVE_RoomInfo(string name, int maxPlayers, int currentPlayers, List<int> connectionIds = default)
        {
            Name = name;
            SceneName = default;
            MaxPlayers = maxPlayers;
            CurrentPlayers = currentPlayers;

            ConnectionIds = connectionIds;
        }
    }
}
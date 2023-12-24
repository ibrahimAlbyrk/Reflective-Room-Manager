using System.Collections.Generic;

namespace REFLECTIVE.Runtime.NETWORK.Room.Structs
{
    [System.Serializable]
    public struct RoomInfo
    {
        public uint ID;

        public bool IsPrivate;
        
        public string RoomName;
        public string SceneName;
        
        public int MaxPlayers;
        public int CurrentPlayers;

        public Dictionary<string, string> CustomData;
    }
}
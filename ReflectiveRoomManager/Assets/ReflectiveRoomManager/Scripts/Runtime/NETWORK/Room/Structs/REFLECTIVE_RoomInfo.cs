namespace REFLECTIVE.Runtime.NETWORK.Room.Structs
{
    [System.Serializable]
    public struct REFLECTIVE_RoomInfo
    {
        public string Name;
        public string SceneName;
        
        public int MaxPlayers;
        public int CurrentPlayers;

        public REFLECTIVE_RoomInfo(string name, int maxPlayers, int currentPlayers)
        {
            Name = name;
            SceneName = default;
            MaxPlayers = maxPlayers;
            CurrentPlayers = currentPlayers;
        }
    }
}
namespace REFLECTIVE.Runtime.NETWORK.Room.Structs
{
    [System.Serializable]
    public struct REFLECTIVE_RoomListInfo
    {
        public string Name;
        public int MaxPlayer;
        public int CurrentPlayer;

        public REFLECTIVE_RoomListInfo(string name, int maxPlayer, int currentPlayer)
        {
            Name = name;
            MaxPlayer = maxPlayer;
            CurrentPlayer = currentPlayer;
        }
    }
}
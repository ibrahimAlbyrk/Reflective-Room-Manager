using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.Data
{
    using Loader;
    
    [System.Serializable]
    public struct RoomData
    {
        [Tooltip("Number of players to be determined when creating a server room")]
        public int MaxPlayerCount;
        
        [Tooltip("Maximum number of players a client can specify for a room")]
        public int PlayerCountPerRoom;
        
        [Tooltip("determines what type of loading the room will have")]
        public RoomLoaderType RoomLoaderType;

        public RoomData(int maxPlayerCount, int playerCountPerRoom, RoomLoaderType roomLoaderType)
        {
            MaxPlayerCount = maxPlayerCount;
            PlayerCountPerRoom = playerCountPerRoom;
            RoomLoaderType = roomLoaderType;
        }
    }
}
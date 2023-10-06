using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.Data
{
    using Loader;
    
    [System.Serializable]
    public struct RoomData_SO
    {
        [Tooltip("Number of players to be determined when creating a server room")]
        public int DefaultMaxPlayerCount;
        
        [Tooltip("Maximum number of players a client can specify")]
        public int MaxPlayerCount;
        
        [Tooltip("determines what type of loading the room will have")]
        public RoomLoaderType RoomLoaderType;
    }
}
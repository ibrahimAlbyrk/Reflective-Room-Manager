using System.Collections.Generic;
using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.State.Messages
{
    /// <summary>
    /// Sent from server to client for periodic state sync (e.g., countdown timer).
    /// </summary>
    public struct RoomStateSyncMessage : NetworkMessage
    {
        public uint RoomID;
        public byte StateTypeID;
        public float StateElapsedTime;
        public Dictionary<string, string> StateData;

        public RoomStateSyncMessage(uint roomID, byte stateTypeID, float stateElapsedTime, Dictionary<string, string> stateData = null)
        {
            RoomID = roomID;
            StateTypeID = stateTypeID;
            StateElapsedTime = stateElapsedTime;
            StateData = stateData ?? new Dictionary<string, string>();
        }
    }
}

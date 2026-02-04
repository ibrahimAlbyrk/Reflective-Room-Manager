using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.State.Messages
{
    /// <summary>
    /// Sent from server to client when room state changes.
    /// </summary>
    public struct RoomStateChangeMessage : NetworkMessage
    {
        public uint RoomID;
        public RoomStateData StateData;

        public RoomStateChangeMessage(uint roomID, RoomStateData stateData)
        {
            RoomID = roomID;
            StateData = stateData;
        }
    }
}

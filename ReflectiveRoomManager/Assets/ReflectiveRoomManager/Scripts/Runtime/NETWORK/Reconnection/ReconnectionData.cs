using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Reconnection
{
    using Room;

    public class ReconnectionData
    {
        public string PlayerId { get; }
        public Room Room { get; }
        public float DisconnectTime { get; }
        public object GameState { get; }
        public GameObject PlayerObject { get; }
        public uint RoomID { get; }

        public ReconnectionData(string playerId, Room room, float disconnectTime, object gameState, GameObject playerObject, uint roomID)
        {
            PlayerId = playerId;
            Room = room;
            DisconnectTime = disconnectTime;
            GameState = gameState;
            PlayerObject = playerObject;
            RoomID = roomID;
        }
    }
}

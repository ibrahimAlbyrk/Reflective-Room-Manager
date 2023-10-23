using System;
using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Events
{
    using Structs;
    
    public class RoomEventManager
    {
        /// <summary>Called on the server when the room is created</summary>
        public event Action<RoomInfo> OnServerCreatedRoom;
        
        /// <summary>Called on the server when the client enters the room</summary>
        public event Action<NetworkConnection> OnServerJoinedRoom;
        
        /// <summary>Called on the server when the client leaves the room</summary>
        public event Action<NetworkConnection> OnServerExitedRoom;
        
        /// <summary>Called on the server when the client's connection is lost</summary>
        public event Action<NetworkConnection> OnServerDisconnectedRoom;
        
        public void Invoke_OnServerCreatedRoom(RoomInfo roomInfo) =>
            OnServerCreatedRoom?.Invoke(roomInfo);

        public void Invoke_OnServerJoinedClient(NetworkConnection conn) =>
            OnServerJoinedRoom?.Invoke(conn);

        public void Invoke_OnServerExitedClient(NetworkConnection conn) =>
            OnServerExitedRoom?.Invoke(conn);

        public void Invoke_OnServerDisconnectedClient(NetworkConnection conn) =>
            OnServerDisconnectedRoom?.Invoke(conn);
    }
}
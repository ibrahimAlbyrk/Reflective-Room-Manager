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
        public event Action<NetworkConnection, uint> OnServerJoinedRoom;
        
        /// <summary>Called on the server when the client leaves the room</summary>
        public event Action<NetworkConnection> OnServerExitedRoom;
        
        /// <summary>Called on the server when the client's connection is lost</summary>
        public event Action<NetworkConnection> OnServerDisconnectedRoom;
        
        internal void Invoke_OnServerCreatedRoom(RoomInfo roomInfo)
        {
            OnServerCreatedRoom?.Invoke(roomInfo);
        }

        internal void Invoke_OnServerJoinedClient(NetworkConnection conn, uint roomID)
        {
            OnServerJoinedRoom?.Invoke(conn, roomID);
        }

        internal void Invoke_OnServerExitedClient(NetworkConnection conn)
        {
            OnServerExitedRoom?.Invoke(conn);
        }

        internal void Invoke_OnServerDisconnectedClient(NetworkConnection conn)
        {
            OnServerDisconnectedRoom?.Invoke(conn);
        }

        /// <summary>Called on the server when room custom data is updated</summary>
        public event Action<Room> OnServerRoomDataUpdated;

        internal void Invoke_OnServerRoomDataUpdated(Room room)
        {
            OnServerRoomDataUpdated?.Invoke(room);
        }

        /// <summary>Called on the server when a room is about to be removed</summary>
        public event Action<uint> OnServerRoomRemoving;

        internal void Invoke_OnServerRoomRemoving(uint roomId)
        {
            OnServerRoomRemoving?.Invoke(roomId);
        }

        /// <summary>Called on the server when graceful shutdown has been initiated</summary>
        public event Action<float> OnServerShutdownStarted;

        internal void Invoke_OnServerShutdownStarted(float secondsRemaining)
        {
            OnServerShutdownStarted?.Invoke(secondsRemaining);
        }

        /// <summary>Called on the server when a player reconnects to a room</summary>
        public event Action<string, NetworkConnection, uint> OnPlayerReconnected;

        /// <summary>Called on the server when a player disconnects with grace period started</summary>
        public event Action<string, uint> OnPlayerDisconnecting;

        /// <summary>Called on the server when a player's reconnection grace period expired</summary>
        public event Action<string, uint> OnPlayerReconnectionExpired;

        internal void Invoke_OnPlayerReconnected(string playerId, NetworkConnection conn, uint roomID)
        {
            OnPlayerReconnected?.Invoke(playerId, conn, roomID);
        }

        internal void Invoke_OnPlayerDisconnecting(string playerId, uint roomID)
        {
            OnPlayerDisconnecting?.Invoke(playerId, roomID);
        }

        internal void Invoke_OnPlayerReconnectionExpired(string playerId, uint roomID)
        {
            OnPlayerReconnectionExpired?.Invoke(playerId, roomID);
        }
    }
}
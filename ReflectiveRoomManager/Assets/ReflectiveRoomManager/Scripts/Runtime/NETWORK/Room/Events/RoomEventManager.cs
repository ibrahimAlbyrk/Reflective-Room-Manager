using System;
using Mirror;
using UnityEngine;

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
        
        internal void Invoke_OnServerCreatedRoom(RoomInfo roomInfo)
        {
            Debug.Log("OnServerCreatedRoom");
            OnServerCreatedRoom?.Invoke(roomInfo);
        }

        internal void Invoke_OnServerJoinedClient(NetworkConnection conn)
        {
            Debug.Log("OnServerJoinedRoom");
            OnServerJoinedRoom?.Invoke(conn);
        }

        internal void Invoke_OnServerExitedClient(NetworkConnection conn)
        {
            Debug.Log("OnServerExitedRoom");
            OnServerExitedRoom?.Invoke(conn);
        }

        internal void Invoke_OnServerDisconnectedClient(NetworkConnection conn)
        {
            Debug.Log("OnServerDisconnectedClient");
            OnServerDisconnectedRoom?.Invoke(conn);
        }
    }
}
using System;
using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Handlers
{
    using Structs;
    using Connection.Manager;
    
    internal static class RoomConnectionHandler
    {
        internal static void OnServerCreateRoom(Action<RoomInfo, NetworkConnectionToClient> callback)
        {
            ReflectiveConnectionManager.roomConnections.OnServerCreateRoom.AddListener(callback);
        }
        
        internal static void OnServerJoinRoom(Action<NetworkConnectionToClient, string> callback)
        {
            ReflectiveConnectionManager.roomConnections.OnServerJoinRoom.AddListener(callback);
        }
        
        internal static void OnServerExitRoom(Action<NetworkConnectionToClient, bool>  callback)
        {
            ReflectiveConnectionManager.roomConnections.OnServerExitRoom.AddListener(callback);
        }
        
        internal static void OnClientRoomListAdd(Action<RoomInfo> callback)
        {
            ReflectiveConnectionManager.roomConnections.OnClientRoomListAdd.AddListener(callback);
        }
        
        internal static void OnClientRoomListUpdate(Action<RoomInfo> callback)
        {
            ReflectiveConnectionManager.roomConnections.OnClientRoomListUpdate.AddListener(callback);
        }
        
        internal static void OnClientRoomListRemove(Action<RoomInfo> callback)
        {
            ReflectiveConnectionManager.roomConnections.OnClientRoomListRemove.AddListener(callback);
        }
        
        internal static void OnClientRoomIDMessage(Action<uint> callback)
        {
            ReflectiveConnectionManager.roomConnections.OnClientRoomIDMessage.AddListener(callback);
        }
    }
}
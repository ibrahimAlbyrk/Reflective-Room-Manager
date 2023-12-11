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
            ReflectiveConnectionManager.roomConnections.OnServerCreateRoom_AddListener(callback);
        }
        
        internal static void OnServerJoinRoom(Action<NetworkConnectionToClient, string> callback)
        {
            ReflectiveConnectionManager.roomConnections.OnServerJoinRoom_AddListener(callback);
        }
        
        internal static void OnServerExitRoom(Action<NetworkConnectionToClient, bool>  callback)
        {
            ReflectiveConnectionManager.roomConnections.OnServerExitRoom_AddListener(callback);
        }
        
        internal static void OnClientRoomListAdd(Action<RoomInfo> callback)
        {
            ReflectiveConnectionManager.roomConnections.OnClientRoomListAdd_AddListener(callback);
        }
        
        internal static void OnClientRoomListUpdate(Action<RoomInfo> callback)
        {
            ReflectiveConnectionManager.roomConnections.OnClientRoomListUpdate_AddListener(callback);
        }
        
        internal static void OnClientRoomListRemove(Action<RoomInfo> callback)
        {
            ReflectiveConnectionManager.roomConnections.OnClientRoomListRemove_AddListener(callback);
        }
        
        internal static void OnClientConnectionMessage(Action<int> callback)
        {
            ReflectiveConnectionManager.roomConnections.OnClientConnectionMessage_AddListener(callback);
        }
    }
}
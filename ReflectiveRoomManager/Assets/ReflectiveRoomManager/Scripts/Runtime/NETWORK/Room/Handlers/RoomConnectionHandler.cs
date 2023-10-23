using System;
using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Handlers
{
    using Structs;
    using Connection.Manager;
    
    public class RoomConnectionHandler
    {
        public void OnServerCreateRoom(Action<NetworkConnectionToClient, RoomInfo> callback)
        {
            ConnectionManager.roomConnections.OnServerCreateRoom += callback;
        }
        
        public void OnServerJoinRoom(Action<NetworkConnectionToClient, string> callback)
        {
            ConnectionManager.roomConnections.OnServerJoinRoom += callback;
        }
        
        public void OnServerExitRoom(Action<NetworkConnectionToClient, bool>  callback)
        {
            ConnectionManager.roomConnections.OnServerExitRoom += callback;
        }
        
        public void OnClientRoomListAdd(Action<RoomInfo> callback)
        {
            ConnectionManager.roomConnections.OnClientRoomListAdd += callback;
        }
        
        public void OnClientRoomListUpdate(Action<RoomInfo> callback)
        {
            ConnectionManager.roomConnections.OnClientRoomListUpdate += callback;
        }
        
        public void OnClientRoomListRemove(Action<RoomInfo> callback)
        {
            ConnectionManager.roomConnections.OnClientRoomListRemove += callback;
        }
        
        public void OnClientConnectionMessage(Action<int> callback)
        {
            ConnectionManager.roomConnections.OnClientConnectionMessage += callback;
        }
    }
}
using Mirror;
using System.Collections.Generic;

namespace REFLECTIVE.Runtime.NETWORK.Room
{
    using Reconnection.Messages;

    public abstract partial class RoomManagerBase
    {
        private readonly HashSet<NetworkConnectionToClient> _deferredDisconnects = new();

        protected virtual void OnStartServer()
        {
            _connectionManager.RoomConnections.AddRegistersForServer();

            if (_enableReconnection)
                NetworkServer.RegisterHandler<PlayerIdentityMessage>(OnPlayerIdentityMessageReceived);
        }

        protected virtual void OnStopServer()
        {
            _reconnectionService?.ClearAll();
            RemoveAllRoom(forced:true);
        }

        protected virtual void OnStartClient()
        {
            _connectionManager.RoomConnections.AddRegistersForClient();
        }

        protected virtual void OnStopClient()
        {
        }

        protected virtual void OnServerConnect(NetworkConnection conn)
        {
            SendUpdateRoomListForClient(conn);
        }

        protected virtual void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            if (_reconnectionService != null && _reconnectionService.isActiveAndEnabled)
            {
                var room = GetRoomByConnection(conn);
                var playerId = _reconnectionService.GetPlayerId(conn);

                if (room != null && playerId != null && _reconnectionService.HandleDisconnect(conn, room, playerId))
                {
                    _deferredDisconnects.Add(conn);
                    return;
                }
            }

            ExitRoom(conn, true);
        }

        public bool ShouldSkipBaseDisconnect(NetworkConnectionToClient conn)
        {
            return _deferredDisconnects.Remove(conn);
        }

        protected virtual void OnClientConnect()
        {
        }

        protected virtual void OnClientDisconnect()
        {
        }
    }
}
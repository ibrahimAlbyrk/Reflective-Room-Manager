using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room
{
    using Structs;
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
            if (_shutdownCoroutine != null)
            {
                StopCoroutine(_shutdownCoroutine);
                _shutdownCoroutine = null;
            }

            _isShuttingDown = false;

            _rateLimiter?.Clear();
            _cleanupService?.Clear();
            _reconnectionService?.ClearAll();
            RemoveAllRoom(forced:true);
        }

        protected virtual void OnStartClient()
        {
            var useSceneManagement = RoomLoaderType != Loader.RoomLoaderType.NoneScene;
            _connectionManager.RoomConnections.AddRegistersForClient(useSceneManagement);
        }

        protected virtual void OnStopClient()
        {
        }

        protected virtual void OnServerConnect(NetworkConnection conn)
        {
            if (_isShuttingDown)
            {
                conn.Disconnect();
                return;
            }

            SendUpdateRoomListForClient(conn);
        }

        protected virtual void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            _rateLimiter?.RemoveConnection(conn);

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

        public void GracefulShutdown(float warningSeconds = 10f)
        {
            if (_isShuttingDown) return;

            _isShuttingDown = true;

            var warningMsg = new ServerShutdownWarningMessage(warningSeconds);

            foreach (var room in m_rooms)
            {
                foreach (var conn in room.Connections)
                    conn.Send(warningMsg);
            }

            m_eventManager.Invoke_OnServerShutdownStarted(warningSeconds);

            _shutdownCoroutine = StartCoroutine(ShutdownAfterDelay(warningSeconds));
        }

        private IEnumerator ShutdownAfterDelay(float seconds)
        {
            yield return new WaitForSeconds(seconds);

            RemoveAllRoom(forced: true);
            NetworkServer.Shutdown();

            _isShuttingDown = false;
            _shutdownCoroutine = null;
        }
    }
}
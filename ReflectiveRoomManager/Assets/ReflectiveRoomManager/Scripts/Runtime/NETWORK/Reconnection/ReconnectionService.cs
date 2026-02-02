using Mirror;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace REFLECTIVE.Runtime.NETWORK.Reconnection
{
    using Room;
    using Room.Events;
    using Messages;

    public class ReconnectionService : MonoBehaviour
    {
        private readonly Dictionary<string, ReconnectionData> _pending = new();

        private IReconnectionHandler _handler;
        private IPlayerStateSerializer _stateSerializer;
        private IDisconnectedPlayerHandler _playerHandler;
        private IPlayerIdentityProvider _identityProvider;

        private RoomEventManager _eventManager;

        public void Initialize(
            IReconnectionHandler handler,
            IPlayerStateSerializer stateSerializer,
            IDisconnectedPlayerHandler playerHandler,
            IPlayerIdentityProvider identityProvider,
            RoomEventManager eventManager)
        {
            _handler = handler;
            _stateSerializer = stateSerializer;
            _playerHandler = playerHandler;
            _identityProvider = identityProvider;
            _eventManager = eventManager;
        }

        public bool HandleDisconnect(NetworkConnectionToClient conn, Room room, string playerId)
        {
            if (_handler == null || !_handler.CanReconnect(playerId, room))
                return false;

            var playerObject = conn.identity != null ? conn.identity.gameObject : null;

            object gameState = null;
            if (_stateSerializer != null && playerObject != null)
                gameState = _stateSerializer.CaptureState(conn, room, playerObject);

            if (_playerHandler != null && playerObject != null)
                _playerHandler.OnPlayerDisconnected(playerObject, room);

            // Remove connection from room but keep slot reserved
            room.RemoveConnection(conn);
            room.AddReservedSlot();

            var data = new ReconnectionData(
                playerId, room, Time.unscaledTime, gameState, playerObject, room.ID);

            _pending[playerId] = data;

            _eventManager?.Invoke_OnPlayerDisconnecting(playerId, room.ID);

            return true;
        }

        public bool TryReconnect(string playerId, NetworkConnectionToClient newConn)
        {
            if (!_pending.TryGetValue(playerId, out var data))
                return false;

            var room = data.Room;

            if (room == null || !_handler.CanReconnect(playerId, room))
            {
                CleanupPendingReconnection(playerId, data);
                return false;
            }

            // Release reserved slot and add connection back
            room.RemoveReservedSlot();
            room.AddConnection(newConn);

            if (_playerHandler != null && data.PlayerObject != null)
                _playerHandler.OnPlayerReconnected(data.PlayerObject, newConn, room);

            if (_stateSerializer != null && data.PlayerObject != null && data.GameState != null)
                _stateSerializer.RestoreState(newConn, room, data.PlayerObject, data.GameState);

            _handler.OnReconnected(playerId, newConn, room);

            _pending.Remove(playerId);

            _eventManager?.Invoke_OnPlayerReconnected(playerId, newConn, room.ID);

            // Send reconnection result to client
            newConn.Send(new ReconnectionResultMessage
            {
                Success = true,
                RoomID = room.ID
            });

            return true;
        }

        private void Update()
        {
            if (_pending.Count == 0) return;

            var gracePeriod = _handler?.GracePeriodSeconds ?? 30f;
            var expiredKeys = _pending
                .Where(kvp => Time.unscaledTime - kvp.Value.DisconnectTime >= gracePeriod)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var playerId in expiredKeys)
            {
                var data = _pending[playerId];
                CleanupPendingReconnection(playerId, data);
            }
        }

        private void CleanupPendingReconnection(string playerId, ReconnectionData data)
        {
            _pending.Remove(playerId);

            if (data.Room != null)
                data.Room.RemoveReservedSlot();

            if (_playerHandler != null && data.PlayerObject != null)
                _playerHandler.OnPlayerAbandoned(data.PlayerObject, data.Room);

            _handler?.OnGracePeriodExpired(playerId, data.Room);
            _identityProvider?.RemovePlayer(playerId);

            _eventManager?.Invoke_OnPlayerReconnectionExpired(playerId, data.RoomID);

            // Remove room if empty and not server-owned
            if (data.Room != null && data.Room.CurrentPlayers < 1 && data.Room.ReservedSlots < 1 && !data.Room.IsServer)
                RoomManagerBase.Instance.RemoveRoom(data.Room);
        }

        public void ClearAll()
        {
            foreach (var kvp in _pending.ToList())
            {
                CleanupPendingReconnection(kvp.Key, kvp.Value);
            }

            _pending.Clear();
        }

        public bool HasPendingReconnection(string playerId)
        {
            return _pending.ContainsKey(playerId);
        }

        public ReconnectionData GetPendingReconnection(string playerId)
        {
            _pending.TryGetValue(playerId, out var data);
            return data;
        }

        public string GetPlayerId(NetworkConnection conn)
        {
            if (_identityProvider is GuidPlayerIdentityProvider guidProvider)
                return guidProvider.GetPlayerIdByConnection(conn);

            return null;
        }
    }
}

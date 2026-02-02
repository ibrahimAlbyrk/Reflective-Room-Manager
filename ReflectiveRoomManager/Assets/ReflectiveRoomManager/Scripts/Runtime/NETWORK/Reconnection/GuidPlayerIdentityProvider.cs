using System;
using Mirror;
using System.Collections.Generic;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Reconnection
{
    public class GuidPlayerIdentityProvider : MonoBehaviour, IPlayerIdentityProvider
    {
        private readonly Dictionary<string, NetworkConnectionToClient> _playerIdToConnection = new();
        private readonly Dictionary<NetworkConnection, string> _connectionToPlayerId = new();

        public string GetOrAssignPlayerId(NetworkConnectionToClient conn, string clientProvidedId)
        {
            // If client provided a known ID, reuse it
            if (!string.IsNullOrEmpty(clientProvidedId) && _playerIdToConnection.ContainsKey(clientProvidedId))
            {
                // Update connection mapping for reconnection
                var oldConn = _playerIdToConnection[clientProvidedId];
                if (oldConn != null)
                    _connectionToPlayerId.Remove(oldConn);

                _playerIdToConnection[clientProvidedId] = conn;
                _connectionToPlayerId[conn] = clientProvidedId;
                return clientProvidedId;
            }

            // Generate new ID for first-time connections
            var newId = Guid.NewGuid().ToString();
            _playerIdToConnection[newId] = conn;
            _connectionToPlayerId[conn] = newId;
            return newId;
        }

        public void RemovePlayer(string playerId)
        {
            if (_playerIdToConnection.TryGetValue(playerId, out var conn))
            {
                _connectionToPlayerId.Remove(conn);
                _playerIdToConnection.Remove(playerId);
            }
        }

        public string GetPlayerIdByConnection(NetworkConnection conn)
        {
            _connectionToPlayerId.TryGetValue(conn, out var playerId);
            return playerId;
        }

        public bool HasPlayerId(string playerId)
        {
            return _playerIdToConnection.ContainsKey(playerId);
        }
    }
}

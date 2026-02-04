using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.Discovery.Handlers
{
    using Messages;

    /// <summary>
    /// Network message handlers for room discovery.
    /// </summary>
    public static class RoomDiscoveryNetworkHandlers
    {
        private static bool _serverHandlersRegistered;
        private static bool _clientHandlersRegistered;

        public static void RegisterServerHandlers()
        {
            if (_serverHandlersRegistered)
            {
                Debug.LogWarning("[RoomDiscoveryNetworkHandlers] Server handlers already registered");
                return;
            }

            NetworkServer.RegisterHandler<RoomQueryRequestMessage>(OnServerRoomQuery);
            _serverHandlersRegistered = true;
        }

        public static void RegisterClientHandlers()
        {
            if (_clientHandlersRegistered)
            {
                Debug.LogWarning("[RoomDiscoveryNetworkHandlers] Client handlers already registered");
                return;
            }

            NetworkClient.RegisterHandler<RoomQueryResponseMessage>(OnClientRoomQueryResponse);
            NetworkClient.RegisterHandler<RoomDeltaUpdateMessage>(OnClientRoomDeltaUpdate);
            _clientHandlersRegistered = true;
        }

        public static void UnregisterServerHandlers()
        {
            if (!_serverHandlersRegistered) return;

            NetworkServer.UnregisterHandler<RoomQueryRequestMessage>();
            _serverHandlersRegistered = false;
        }

        public static void UnregisterClientHandlers()
        {
            if (!_clientHandlersRegistered) return;

            NetworkClient.UnregisterHandler<RoomQueryResponseMessage>();
            NetworkClient.UnregisterHandler<RoomDeltaUpdateMessage>();
            _clientHandlersRegistered = false;
        }

        private static void OnServerRoomQuery(NetworkConnectionToClient conn, RoomQueryRequestMessage msg)
        {
            var roomManager = RoomManagerBase.Instance;
            if (roomManager == null)
            {
                Debug.LogWarning("[RoomDiscoveryNetworkHandlers] RoomManagerBase instance not found");
                return;
            }

            var discoveryService = roomManager.DiscoveryService;
            if (discoveryService == null)
            {
                Debug.LogWarning("[RoomDiscoveryNetworkHandlers] DiscoveryService not initialized. Enable room discovery in RoomManager inspector.");
                conn.Send(new RoomQueryResponseMessage(RoomQueryResponse.Empty));
                return;
            }

            var response = discoveryService.ExecuteQuery(msg.Request);
            conn.Send(new RoomQueryResponseMessage(response));
        }

        private static void OnClientRoomQueryResponse(RoomQueryResponseMessage msg)
        {
            OnClientRoomQueryResponseReceived?.Invoke(msg.Response);
        }

        private static void OnClientRoomDeltaUpdate(RoomDeltaUpdateMessage msg)
        {
            OnClientRoomDeltaUpdateReceived?.Invoke(msg.Update);
        }

        /// <summary>
        /// Broadcasts delta update to all connected clients.
        /// </summary>
        public static void BroadcastDeltaUpdate(RoomDeltaUpdate update)
        {
            if (!NetworkServer.active) return;

            var message = new RoomDeltaUpdateMessage(update);

            foreach (var conn in NetworkServer.connections.Values)
            {
                if (conn != null && conn.isReady)
                    conn.Send(message);
            }
        }

        /// <summary>
        /// Sends a room query request from the client.
        /// </summary>
        public static void SendQueryRequest(RoomQueryRequest request)
        {
            if (!NetworkClient.active)
            {
                Debug.LogWarning("[RoomDiscoveryNetworkHandlers] Cannot send query - client not active");
                return;
            }

            NetworkClient.Send(new RoomQueryRequestMessage(request));
        }

        /// <summary>
        /// Sends a room query request built from a RoomFilter.
        /// </summary>
        public static void SendQueryRequest(
            RoomFilter filter,
            RoomSortOptions sortBy = RoomSortOptions.None,
            int pageNumber = 1,
            int pageSize = 20)
        {
            var request = new RoomQueryRequest(filter.Build(), sortBy, pageNumber, pageSize);
            SendQueryRequest(request);
        }

        // Client-side events
        public delegate void RoomQueryResponseHandler(RoomQueryResponse response);
        public delegate void RoomDeltaUpdateHandler(RoomDeltaUpdate update);

        /// <summary>
        /// Fired when client receives a room query response.
        /// </summary>
        public static event RoomQueryResponseHandler OnClientRoomQueryResponseReceived;

        /// <summary>
        /// Fired when client receives a delta update.
        /// </summary>
        public static event RoomDeltaUpdateHandler OnClientRoomDeltaUpdateReceived;

        public static void ClearClientEvents()
        {
            OnClientRoomQueryResponseReceived = null;
            OnClientRoomDeltaUpdateReceived = null;
        }
    }
}

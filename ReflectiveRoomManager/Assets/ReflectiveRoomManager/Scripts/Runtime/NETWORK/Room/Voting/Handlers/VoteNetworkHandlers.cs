using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.Voting.Handlers
{
    using Messages;

    /// <summary>
    /// Network message handlers for voting system.
    /// </summary>
    public static class VoteNetworkHandlers
    {
#if REFLECTIVE_SERVER
        private static bool _serverHandlersRegistered;

        public static void RegisterServerHandlers()
        {
            if (_serverHandlersRegistered)
            {
                Debug.LogWarning("[VoteNetworkHandlers] Server handlers already registered");
                return;
            }

            NetworkServer.RegisterHandler<StartVoteRequest>(OnServerStartVote);
            NetworkServer.RegisterHandler<CastVoteRequest>(OnServerCastVote);
            NetworkServer.RegisterHandler<CancelVoteRequest>(OnServerCancelVote);
            _serverHandlersRegistered = true;
        }

        public static void UnregisterServerHandlers()
        {
            if (!_serverHandlersRegistered) return;

            NetworkServer.UnregisterHandler<StartVoteRequest>();
            NetworkServer.UnregisterHandler<CastVoteRequest>();
            NetworkServer.UnregisterHandler<CancelVoteRequest>();
            _serverHandlersRegistered = false;
        }

        private static void OnServerStartVote(NetworkConnectionToClient conn, StartVoteRequest msg)
        {
            var roomManager = RoomManagerBase.Instance;
            if (roomManager == null)
            {
                Debug.LogWarning("[VoteNetworkHandlers] RoomManagerBase instance not found");
                return;
            }

            var room = roomManager.GetRoom(msg.RoomID);
            if (room == null || room.VoteManager == null)
                return;

            // Verify connection is in the room
            if (!room.Connections.Contains(conn))
            {
                Debug.LogWarning("[VoteNetworkHandlers] Connection not in room");
                return;
            }

            // Deserialize custom data if provided
            object customData = null;
            if (!string.IsNullOrEmpty(msg.CustomDataJson))
            {
                try
                {
                    customData = JsonUtility.FromJson<object>(msg.CustomDataJson);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[VoteNetworkHandlers] Failed to deserialize custom data: {ex.Message}");
                }
            }

            room.VoteManager.StartVote(conn, msg.VoteTypeID, customData);
        }

        private static void OnServerCastVote(NetworkConnectionToClient conn, CastVoteRequest msg)
        {
            var roomManager = RoomManagerBase.Instance;
            if (roomManager == null) return;

            var room = roomManager.GetRoom(msg.RoomID);
            if (room == null || room.VoteManager == null)
                return;

            // Validate vote ID
            if (room.VoteManager.CurrentVote?.VoteID != msg.VoteID)
            {
                Debug.LogWarning("[VoteNetworkHandlers] Vote ID mismatch");
                return;
            }

            room.VoteManager.CastVote(conn, msg.OptionIndex);
        }

        private static void OnServerCancelVote(NetworkConnectionToClient conn, CancelVoteRequest msg)
        {
            var roomManager = RoomManagerBase.Instance;
            if (roomManager == null) return;

            var room = roomManager.GetRoom(msg.RoomID);
            if (room == null || room.VoteManager == null)
                return;

            room.VoteManager.CancelVote(conn);
        }

        /// <summary>
        /// Broadcasts vote started message to all clients in room.
        /// </summary>
        public static void BroadcastVoteStarted(Room room, ActiveVote vote)
        {
            if (room == null || vote == null) return;

            var initiatorConnId = (vote.Initiator as NetworkConnectionToClient)?.connectionId
                                  ?? vote.Initiator.GetHashCode();

            var message = new VoteStartedMessage
            {
                RoomID = room.ID,
                VoteID = vote.VoteID,
                VoteTypeID = vote.Type.TypeID,
                InitiatorConnectionID = (uint)initiatorConnId,
                Question = vote.Question,
                Options = vote.Options,
                Duration = vote.Duration,
                StartTime = vote.StartTime
            };

            foreach (var conn in room.Connections)
            {
                conn.Send(message);
            }
        }

        /// <summary>
        /// Broadcasts vote update message to all clients in room.
        /// </summary>
        public static void BroadcastVoteUpdate(Room room, ActiveVote vote)
        {
            if (room == null || vote == null) return;

            var message = new VoteUpdateMessage
            {
                RoomID = room.ID,
                VoteID = vote.VoteID,
                VoteCounts = vote.GetVoteCounts(),
                RemainingTime = vote.RemainingTime
            };

            foreach (var conn in room.Connections)
            {
                conn.Send(message);
            }
        }

        /// <summary>
        /// Broadcasts vote ended message to all clients in room.
        /// </summary>
        public static void BroadcastVoteEnded(Room room, ActiveVote vote, VoteResult result)
        {
            if (room == null || vote == null) return;

            var message = new VoteEndedMessage
            {
                RoomID = room.ID,
                VoteID = vote.VoteID,
                WinningOption = result.WinningOption,
                VoteCounts = result.VoteCounts,
                ParticipationRate = result.ParticipationRate,
                Passed = result.Passed,
                Reason = result.Reason
            };

            foreach (var conn in room.Connections)
            {
                conn.Send(message);
            }
        }
#endif

#if REFLECTIVE_CLIENT
        private static bool _clientHandlersRegistered;

        public static void RegisterClientHandlers()
        {
            if (_clientHandlersRegistered)
            {
                Debug.LogWarning("[VoteNetworkHandlers] Client handlers already registered");
                return;
            }

            NetworkClient.RegisterHandler<VoteStartedMessage>(OnClientVoteStarted);
            NetworkClient.RegisterHandler<VoteUpdateMessage>(OnClientVoteUpdate);
            NetworkClient.RegisterHandler<VoteEndedMessage>(OnClientVoteEnded);
            _clientHandlersRegistered = true;
        }

        public static void UnregisterClientHandlers()
        {
            if (!_clientHandlersRegistered) return;

            NetworkClient.UnregisterHandler<VoteStartedMessage>();
            NetworkClient.UnregisterHandler<VoteUpdateMessage>();
            NetworkClient.UnregisterHandler<VoteEndedMessage>();
            _clientHandlersRegistered = false;
        }

        private static void OnClientVoteStarted(VoteStartedMessage msg)
        {
            OnClientVoteStartedEvent?.Invoke(msg);
        }

        private static void OnClientVoteUpdate(VoteUpdateMessage msg)
        {
            OnClientVoteUpdatedEvent?.Invoke(msg);
        }

        private static void OnClientVoteEnded(VoteEndedMessage msg)
        {
            OnClientVoteEndedEvent?.Invoke(msg);
        }

        // Client-side events for UI binding
        public delegate void ClientVoteStartedHandler(VoteStartedMessage msg);
        public delegate void ClientVoteUpdatedHandler(VoteUpdateMessage msg);
        public delegate void ClientVoteEndedHandler(VoteEndedMessage msg);

        public static event ClientVoteStartedHandler OnClientVoteStartedEvent;
        public static event ClientVoteUpdatedHandler OnClientVoteUpdatedEvent;
        public static event ClientVoteEndedHandler OnClientVoteEndedEvent;

        public static void ClearClientEvents()
        {
            OnClientVoteStartedEvent = null;
            OnClientVoteUpdatedEvent = null;
            OnClientVoteEndedEvent = null;
        }
#endif
    }
}

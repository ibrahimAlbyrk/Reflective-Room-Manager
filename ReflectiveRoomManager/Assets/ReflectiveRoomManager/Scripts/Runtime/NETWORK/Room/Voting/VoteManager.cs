using Mirror;
using System.Collections.Generic;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.Voting
{
    using Handlers;
    using Types;
    using State;
    using State.States;

    /// <summary>
    /// Manages vote lifecycle within a room.
    /// Server-authoritative vote management.
    /// </summary>
    public class VoteManager
    {
        private readonly Room _room;
        private readonly VoteConfig _config;
        private readonly Dictionary<string, IVoteType> _voteTypes;
        private readonly VoteCooldownTracker _cooldowns;

        private ActiveVote _currentVote;

        public ActiveVote CurrentVote => _currentVote;
        public bool IsVoteActive => _currentVote != null;

        public VoteManager(Room room, VoteConfig config)
        {
            _room = room;
            _config = config ?? CreateDefaultConfig();
            _voteTypes = new Dictionary<string, IVoteType>();
            _cooldowns = new VoteCooldownTracker();

            RegisterDefaultVoteTypes();
        }

        private static VoteConfig CreateDefaultConfig()
        {
            return ScriptableObject.CreateInstance<VoteConfig>();
        }

        #region Vote Type Registration

        private void RegisterDefaultVoteTypes()
        {
            RegisterVoteType(new KickPlayerVote());
            RegisterVoteType(new MapChangeVote());
            RegisterVoteType(new SkipRoundVote());
            RegisterVoteType(new RestartMatchVote());
            RegisterVoteType(new EndMatchVote());
        }

        /// <summary>
        /// Registers a custom vote type.
        /// </summary>
        public void RegisterVoteType(IVoteType voteType)
        {
            if (voteType == null)
            {
                Debug.LogError("[VoteManager] Cannot register null vote type");
                return;
            }

            if (_voteTypes.ContainsKey(voteType.TypeID))
            {
                Debug.LogWarning($"[VoteManager] Vote type {voteType.TypeID} already registered");
                return;
            }

            _voteTypes[voteType.TypeID] = voteType;

            if (_config.EnableDebugLogs)
                Debug.Log($"[VoteManager] Registered vote type: {voteType.DisplayName}");
        }

        /// <summary>
        /// Gets registered vote type by ID.
        /// </summary>
        public IVoteType GetVoteType(string typeID)
        {
            return _voteTypes.TryGetValue(typeID, out var type) ? type : null;
        }

        #endregion

        #region Vote Lifecycle

        /// <summary>
        /// Starts a new vote. Server-only operation.
        /// </summary>
        public bool StartVote(NetworkConnection initiator, string voteTypeID, object customData = null)
        {
            if (!NetworkServer.active)
            {
                Debug.LogError("[VoteManager] StartVote can only be called on server");
                return false;
            }

            // Check if vote already active
            if (IsVoteActive)
            {
                if (_config.EnableDebugLogs)
                    Debug.LogWarning("[VoteManager] Cannot start vote - vote already active");
                return false;
            }

            // Get vote type
            if (!_voteTypes.TryGetValue(voteTypeID, out var voteType))
            {
                Debug.LogError($"[VoteManager] Unknown vote type: {voteTypeID}");
                return false;
            }

            // Check cooldown
            if (_cooldowns.IsOnCooldown(voteTypeID, initiator))
            {
                if (_config.EnableDebugLogs)
                    Debug.LogWarning($"[VoteManager] Vote type {voteTypeID} on cooldown");
                return false;
            }

            // Create context (needed by CanInitiate for KickPlayerVote target check)
            var context = new VoteContext(_room, initiator, customData);

            // Check permission
            if (!voteType.CanInitiate(initiator, _room, out var reason))
            {
                if (_config.EnableDebugLogs)
                    Debug.LogWarning($"[VoteManager] Cannot initiate vote: {reason}");
                return false;
            }

            // Get question and options
            var question = voteType.GetQuestion(context);
            var options = voteType.GetOptions(context);

            if (options == null || options.Length < 2)
            {
                Debug.LogError("[VoteManager] Vote must have at least 2 options");
                return false;
            }

            // Create active vote
            _currentVote = new ActiveVote(
                voteType,
                initiator,
                question,
                options,
                Time.time,
                voteType.Duration,
                context
            );

            // Notify vote type
            voteType.OnVoteStarted(_currentVote, _room);

            // Broadcast to clients
            NotifyVoteStarted();

            if (_config.EnableDebugLogs)
                Debug.Log($"[VoteManager] Vote started: {voteType.DisplayName} - {question}");

            return true;
        }

        /// <summary>
        /// Casts a vote for an option.
        /// </summary>
        public bool CastVote(NetworkConnection voter, int optionIndex)
        {
            if (!NetworkServer.active) return false;
            if (!IsVoteActive) return false;

            // Validate option index
            if (optionIndex < 0 || optionIndex >= _currentVote.Options.Length)
            {
                Debug.LogWarning($"[VoteManager] Invalid option index: {optionIndex}");
                return false;
            }

            // Check if player can vote
            if (!_currentVote.Type.CanVote(voter, _room, _currentVote.Context))
            {
                if (_config.EnableDebugLogs)
                    Debug.LogWarning("[VoteManager] Player cannot vote on this vote");
                return false;
            }

            // Check if already voted
            if (_currentVote.HasVoted(voter))
            {
                if (!_currentVote.Type.AllowVoteChange)
                {
                    if (_config.EnableDebugLogs)
                        Debug.LogWarning("[VoteManager] Vote change not allowed");
                    return false;
                }
            }

            // Cast vote
            _currentVote.Votes[voter] = optionIndex;

            // Broadcast update
            NotifyVoteUpdate();

            if (_config.EnableDebugLogs)
                Debug.Log($"[VoteManager] Vote cast: {(voter as NetworkConnectionToClient)?.connectionId} -> option {optionIndex}");

            // Check if all voted
            CheckVoteEnd();

            return true;
        }

        /// <summary>
        /// Changes existing vote (if allowed).
        /// </summary>
        public bool ChangeVote(NetworkConnection voter, int newOptionIndex)
        {
            if (!IsVoteActive) return false;
            if (!_currentVote.Type.AllowVoteChange) return false;

            return CastVote(voter, newOptionIndex);
        }

        /// <summary>
        /// Cancels active vote (admin/owner only).
        /// </summary>
        public bool CancelVote(NetworkConnection canceller)
        {
            if (!IsVoteActive) return false;

            // Check permission (requires admin or owner role)
            if (_room.RoleManager != null)
            {
                var role = _room.RoleManager.GetPlayerRole(canceller);
                if (role < Roles.RoomRole.Admin)
                {
                    Debug.LogWarning("[VoteManager] Only admins can cancel votes");
                    return false;
                }
            }

            // End vote as cancelled
            var result = new VoteResult
            {
                WinningOption = -1,
                VoteCounts = _currentVote.GetVoteCounts(),
                ParticipationRate = _currentVote.GetParticipationRate(GetEligibleVoters()),
                Passed = false,
                Reason = VoteEndReason.Cancelled,
                VotesByOption = _currentVote.GetVotesByOption()
            };

            ProcessVoteResult(result);
            return true;
        }

        #endregion

        #region Vote Processing

        /// <summary>
        /// Updates active vote (called every frame).
        /// </summary>
        public void Update(float deltaTime)
        {
            _cooldowns.Update(deltaTime);

            if (!IsVoteActive) return;

            // Check timer
            if (_currentVote.RemainingTime <= 0f)
            {
                CheckVoteEnd();
            }
        }

        /// <summary>
        /// Checks if vote should end and processes result.
        /// </summary>
        private void CheckVoteEnd()
        {
            if (!IsVoteActive) return;

            var eligibleVoters = GetEligibleVoters();
            var allVoted = _currentVote.TotalVotes >= eligibleVoters;
            var timerExpired = _currentVote.RemainingTime <= 0f;

            if (allVoted || timerExpired)
            {
                var result = _currentVote.CalculateResult(eligibleVoters);
                result.Reason = allVoted ? VoteEndReason.AllVoted : VoteEndReason.TimerExpired;

                ProcessVoteResult(result);
            }
        }

        /// <summary>
        /// Processes vote result and applies action if passed.
        /// </summary>
        private void ProcessVoteResult(VoteResult result)
        {
            if (!IsVoteActive) return;

            var voteType = _currentVote.Type;

            if (result.Passed)
            {
                // Apply result
                voteType.ApplyResult(result, _room);

                // Start normal cooldown
                _cooldowns.StartCooldown(voteType.TypeID, voteType.Cooldown, false);

                if (_config.EnableDebugLogs)
                    Debug.Log($"[VoteManager] Vote passed: option {result.WinningOption}");
            }
            else
            {
                // Start failed vote cooldown (longer)
                var failedCooldown = voteType.Cooldown * _config.FailedVoteCooldownMultiplier;
                _cooldowns.StartCooldown(voteType.TypeID, failedCooldown, true);

                // Per-player cooldown for initiator
                _cooldowns.StartPlayerCooldown(_currentVote.Initiator, voteType.TypeID, failedCooldown);

                if (_config.EnableDebugLogs)
                    Debug.Log($"[VoteManager] Vote failed: {result.Reason}");
            }

            // Notify vote type
            voteType.OnVoteEnded(result, _room);

            // Broadcast result
            NotifyVoteEnded(result);

            // Clear active vote
            _currentVote = null;
        }

        /// <summary>
        /// Gets number of eligible voters for current vote.
        /// </summary>
        private int GetEligibleVoters()
        {
            if (!IsVoteActive) return 0;

            var count = 0;
            foreach (var conn in _room.Connections)
            {
                if (_currentVote.Type.CanVote(conn, _room, _currentVote.Context))
                    count++;
            }
            return count;
        }

        #endregion

        #region Network Notification

        private void NotifyVoteStarted()
        {
#if REFLECTIVE_SERVER
            VoteNetworkHandlers.BroadcastVoteStarted(_room, _currentVote);
#endif
        }

        private void NotifyVoteUpdate()
        {
#if REFLECTIVE_SERVER
            VoteNetworkHandlers.BroadcastVoteUpdate(_room, _currentVote);
#endif
        }

        private void NotifyVoteEnded(VoteResult result)
        {
#if REFLECTIVE_SERVER
            VoteNetworkHandlers.BroadcastVoteEnded(_room, _currentVote, result);
#endif
        }

        #endregion

        #region Lifecycle

        /// <summary>
        /// Called when player leaves room.
        /// </summary>
        internal void OnPlayerLeft(NetworkConnection conn)
        {
            if (!IsVoteActive) return;

            // Remove vote if present
            _currentVote.Votes.Remove(conn);

            // Clear player cooldowns
            _cooldowns.ClearPlayerCooldowns(conn);

            // If initiator left, cancel vote
            if (_currentVote.Initiator == conn)
            {
                var result = new VoteResult
                {
                    WinningOption = -1,
                    VoteCounts = _currentVote.GetVoteCounts(),
                    ParticipationRate = _currentVote.GetParticipationRate(GetEligibleVoters()),
                    Passed = false,
                    Reason = VoteEndReason.InitiatorLeft,
                    VotesByOption = _currentVote.GetVotesByOption()
                };

                ProcessVoteResult(result);
            }
            else
            {
                // Check if vote should end (all remaining players voted)
                CheckVoteEnd();
            }
        }

        /// <summary>
        /// Called when room state changes.
        /// </summary>
        internal void OnRoomStateChanged(IRoomState newState)
        {
            if (!IsVoteActive) return;

            // Cancel vote if transitioning to incompatible state
            if (newState is EndedState || newState is LobbyState)
            {
                var result = new VoteResult
                {
                    WinningOption = -1,
                    VoteCounts = _currentVote.GetVoteCounts(),
                    ParticipationRate = _currentVote.GetParticipationRate(GetEligibleVoters()),
                    Passed = false,
                    Reason = VoteEndReason.StateChanged,
                    VotesByOption = _currentVote.GetVotesByOption()
                };

                ProcessVoteResult(result);
            }
        }

        /// <summary>
        /// Cleans up vote manager (on room close).
        /// </summary>
        internal void Cleanup()
        {
            _currentVote = null;
            _cooldowns.Clear();
        }

        #endregion
    }
}

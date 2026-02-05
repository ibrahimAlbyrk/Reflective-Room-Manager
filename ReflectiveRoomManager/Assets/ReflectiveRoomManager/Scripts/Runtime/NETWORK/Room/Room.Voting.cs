using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room
{
    using Voting;
    using State;

    /// <summary>
    /// Partial class extension for Room voting system support.
    /// </summary>
    public partial class Room
    {
        /// <summary>
        /// Vote manager instance (null if voting not enabled).
        /// </summary>
        public VoteManager VoteManager { get; private set; }

        /// <summary>
        /// Initializes vote manager for this room.
        /// Called during room creation.
        /// </summary>
        internal void InitializeVoteManager(VoteConfig config)
        {
            if (VoteManager != null)
            {
                Debug.LogWarning($"[Room] Vote manager already initialized for room '{Name}'");
                return;
            }

            if (config == null)
            {
                Debug.LogError($"[Room] Cannot initialize vote manager for room '{Name}': config is null");
                return;
            }

            VoteManager = new VoteManager(this, config);
        }

        /// <summary>
        /// Updates vote manager (called from RoomManagerBase.Update).
        /// </summary>
        internal void UpdateVoteManager(float deltaTime)
        {
            VoteManager?.Update(deltaTime);
        }

        /// <summary>
        /// Called when player leaves (notify vote manager).
        /// </summary>
        internal void NotifyPlayerLeftForVoting(NetworkConnection conn)
        {
            VoteManager?.OnPlayerLeft(conn);
        }

        /// <summary>
        /// Called when room state changes (notify vote manager).
        /// </summary>
        internal void NotifyStateChangedForVoting(IRoomState newState)
        {
            VoteManager?.OnRoomStateChanged(newState);
        }

        /// <summary>
        /// Cleans up vote manager (on room close).
        /// </summary>
        internal void CleanupVoteManager()
        {
            VoteManager?.Cleanup();
            VoteManager = null;
        }
    }
}

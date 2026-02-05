using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Voting
{
    /// <summary>
    /// Base interface for all vote types.
    /// Fully extensible - developers can create any vote type.
    /// </summary>
    public interface IVoteType
    {
        /// <summary>Unique identifier for this vote type (e.g., "kick_player")</summary>
        string TypeID { get; }

        /// <summary>Display name shown to players (e.g., "Kick Player")</summary>
        string DisplayName { get; }

        #region Configuration

        /// <summary>Vote duration in seconds (default: 30)</summary>
        float Duration { get; }

        /// <summary>Minimum participation rate (0-1) for vote to be valid (default: 0.5)</summary>
        float MinParticipationRate { get; }

        /// <summary>Winning threshold (0-1) for option to pass (default: 0.51 = majority)</summary>
        float WinningThreshold { get; }

        /// <summary>How to resolve ties</summary>
        TieResolutionMode TieResolution { get; }

        /// <summary>Allow players to change their vote</summary>
        bool AllowVoteChange { get; }

        /// <summary>Allow spectators to vote (default: false)</summary>
        bool AllowSpectatorVote { get; }

        /// <summary>Cooldown duration in seconds after this vote type (0 = no cooldown)</summary>
        float Cooldown { get; }

        #endregion

        #region Dynamic Content

        /// <summary>
        /// Returns the question to ask players.
        /// Called when vote starts.
        /// </summary>
        string GetQuestion(VoteContext context);

        /// <summary>
        /// Returns available options for players to choose from.
        /// </summary>
        string[] GetOptions(VoteContext context);

        #endregion

        #region Permission & Validation

        /// <summary>
        /// Checks if initiator can start this vote type.
        /// </summary>
        bool CanInitiate(NetworkConnection initiator, Room room, out string reason);

        /// <summary>
        /// Checks if player can vote on this vote type.
        /// </summary>
        bool CanVote(NetworkConnection voter, Room room, VoteContext context);

        #endregion

        #region Lifecycle Hooks

        /// <summary>
        /// Called when vote starts (server-side).
        /// </summary>
        void OnVoteStarted(ActiveVote vote, Room room);

        /// <summary>
        /// Called when vote ends (server-side).
        /// </summary>
        void OnVoteEnded(VoteResult result, Room room);

        /// <summary>
        /// Applies the winning option's action (server-side).
        /// Called only if vote passed.
        /// </summary>
        void ApplyResult(VoteResult result, Room room);

        #endregion
    }
}

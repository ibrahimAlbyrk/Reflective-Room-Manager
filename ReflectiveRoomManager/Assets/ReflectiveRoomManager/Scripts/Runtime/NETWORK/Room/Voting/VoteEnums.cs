namespace REFLECTIVE.Runtime.NETWORK.Room.Voting
{
    /// <summary>
    /// Tie resolution strategies.
    /// </summary>
    public enum TieResolutionMode : byte
    {
        /// <summary>Vote fails on tie</summary>
        Fail = 0,

        /// <summary>First option wins</summary>
        FirstOption = 1,

        /// <summary>Last option wins</summary>
        LastOption = 2,

        /// <summary>Random option wins</summary>
        Random = 3,

        /// <summary>Initiator's choice wins</summary>
        InitiatorChoice = 4
    }

    /// <summary>
    /// Reason why vote ended.
    /// </summary>
    public enum VoteEndReason : byte
    {
        /// <summary>Timer expired naturally</summary>
        TimerExpired = 0,

        /// <summary>All eligible players voted</summary>
        AllVoted = 1,

        /// <summary>Vote cancelled by admin/owner</summary>
        Cancelled = 2,

        /// <summary>Room state changed (e.g., game ended)</summary>
        StateChanged = 3,

        /// <summary>Initiator left room</summary>
        InitiatorLeft = 4
    }
}

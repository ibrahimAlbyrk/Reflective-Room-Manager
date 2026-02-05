using Mirror;
using System.Collections.Generic;

namespace REFLECTIVE.Runtime.NETWORK.Room.Voting
{
    /// <summary>
    /// Immutable result of a completed vote.
    /// </summary>
    public struct VoteResult
    {
        public int WinningOption;
        public int[] VoteCounts;
        public float ParticipationRate;
        public bool Passed;
        public VoteEndReason Reason;
        public Dictionary<int, List<NetworkConnection>> VotesByOption;
    }
}

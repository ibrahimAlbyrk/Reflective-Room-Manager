using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Voting.Messages
{
    /// <summary>
    /// Sent from server to clients when vote ends.
    /// </summary>
    public struct VoteEndedMessage : NetworkMessage
    {
        public uint RoomID;
        public string VoteID;
        public int WinningOption;
        public int[] VoteCounts;
        public float ParticipationRate;
        public bool Passed;
        public VoteEndReason Reason;
    }
}

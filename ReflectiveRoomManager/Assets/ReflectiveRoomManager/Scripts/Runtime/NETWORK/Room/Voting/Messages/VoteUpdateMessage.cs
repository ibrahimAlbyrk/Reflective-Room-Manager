using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Voting.Messages
{
    /// <summary>
    /// Sent from server to clients when vote counts update.
    /// </summary>
    public struct VoteUpdateMessage : NetworkMessage
    {
        public uint RoomID;
        public string VoteID;
        public int[] VoteCounts;
        public float RemainingTime;
    }
}

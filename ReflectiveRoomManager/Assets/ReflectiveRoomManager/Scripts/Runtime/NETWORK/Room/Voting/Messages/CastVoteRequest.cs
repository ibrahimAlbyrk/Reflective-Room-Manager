using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Voting.Messages
{
    /// <summary>
    /// Sent from client to server to cast a vote.
    /// </summary>
    public struct CastVoteRequest : NetworkMessage
    {
        public uint RoomID;
        public string VoteID;
        public int OptionIndex;
    }
}

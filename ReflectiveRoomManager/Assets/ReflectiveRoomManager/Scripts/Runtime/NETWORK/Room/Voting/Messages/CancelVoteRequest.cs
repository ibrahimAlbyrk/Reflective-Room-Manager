using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Voting.Messages
{
    /// <summary>
    /// Sent from client to server to cancel active vote (admin only).
    /// </summary>
    public struct CancelVoteRequest : NetworkMessage
    {
        public uint RoomID;
        public string VoteID;
    }
}

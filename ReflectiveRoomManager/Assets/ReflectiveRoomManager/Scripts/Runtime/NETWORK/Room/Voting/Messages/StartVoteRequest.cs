using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Voting.Messages
{
    /// <summary>
    /// Sent from client to server to start a vote.
    /// </summary>
    public struct StartVoteRequest : NetworkMessage
    {
        public uint RoomID;
        public string VoteTypeID;
        public string CustomDataJson;
    }
}

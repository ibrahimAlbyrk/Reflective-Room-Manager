using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Voting.Messages
{
    /// <summary>
    /// Sent from server to clients when vote starts.
    /// </summary>
    public struct VoteStartedMessage : NetworkMessage
    {
        public uint RoomID;
        public string VoteID;
        public string VoteTypeID;
        public uint InitiatorConnectionID;
        public string Question;
        public string[] Options;
        public float Duration;
        public float StartTime;
    }
}

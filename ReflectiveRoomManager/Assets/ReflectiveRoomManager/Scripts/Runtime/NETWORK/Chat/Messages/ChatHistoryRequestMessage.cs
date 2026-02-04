using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Chat.Messages
{
    /// <summary>
    /// Client to Server: Request message history for a channel.
    /// </summary>
    public struct ChatHistoryRequestMessage : NetworkMessage
    {
        /// <summary>Channel to retrieve history from</summary>
        public ChatChannel Channel;

        /// <summary>Maximum number of messages to retrieve</summary>
        public int Count;
    }
}

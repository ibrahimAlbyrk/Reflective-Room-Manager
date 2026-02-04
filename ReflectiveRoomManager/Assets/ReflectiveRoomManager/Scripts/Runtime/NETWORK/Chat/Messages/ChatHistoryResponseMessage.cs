using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Chat.Messages
{
    using Structs;

    /// <summary>
    /// Server to Client: History response with messages for a channel.
    /// </summary>
    public struct ChatHistoryResponseMessage : NetworkMessage
    {
        /// <summary>Channel the history is for</summary>
        public ChatChannel Channel;

        /// <summary>Array of historical messages</summary>
        public ChatMessage[] Messages;
    }
}

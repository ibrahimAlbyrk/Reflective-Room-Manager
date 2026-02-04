using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Chat.Messages
{
    using Structs;

    /// <summary>
    /// Server to Client: Broadcast a chat message to eligible clients.
    /// </summary>
    public struct ChatBroadcastMessage : NetworkMessage
    {
        /// <summary>The chat message to broadcast</summary>
        public ChatMessage Message;
    }
}

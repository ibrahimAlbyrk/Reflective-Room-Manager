using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Chat.Messages
{
    /// <summary>
    /// Client to Server: Request to send a chat message.
    /// </summary>
    public struct ChatRequestMessage : NetworkMessage
    {
        /// <summary>Target channel for the message</summary>
        public ChatChannel Channel;

        /// <summary>Message content (max 512 chars)</summary>
        public string Content;

        /// <summary>Target connection ID for whisper (0 if not whisper)</summary>
        public uint TargetConnectionID;

        /// <summary>Target player name for whisper display</summary>
        public string TargetName;
    }
}

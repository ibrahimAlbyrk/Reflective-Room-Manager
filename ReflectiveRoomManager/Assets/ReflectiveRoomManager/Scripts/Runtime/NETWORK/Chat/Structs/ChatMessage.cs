using System;

namespace REFLECTIVE.Runtime.NETWORK.Chat.Structs
{
    /// <summary>
    /// Represents a chat message with sender info, content, and metadata.
    /// </summary>
    [Serializable]
    public struct ChatMessage
    {
        /// <summary>Unique message ID (timestamp + sender hash)</summary>
        public ulong MessageID;

        /// <summary>Sender's connection ID</summary>
        public uint SenderConnectionID;

        /// <summary>Sender's display name (max 32 chars)</summary>
        public string SenderName;

        /// <summary>Message content (max 512 chars)</summary>
        public string Content;

        /// <summary>Channel this message was sent on</summary>
        public ChatChannel Channel;

        /// <summary>Unix timestamp in milliseconds</summary>
        public long Timestamp;

        /// <summary>Message flags (system, admin, censored, etc.)</summary>
        public ChatMessageFlags Flags;

        /// <summary>Target connection ID for whisper messages</summary>
        public uint TargetConnectionID;

        /// <summary>Target player name for whisper messages</summary>
        public string TargetName;

        /// <summary>Room ID for room-scoped messages</summary>
        public uint RoomID;
    }

    /// <summary>
    /// Flags indicating message metadata and filter status.
    /// </summary>
    [Flags]
    public enum ChatMessageFlags : byte
    {
        None     = 0,
        System   = 1 << 0,  // Server announcement
        Admin    = 1 << 1,  // From admin/moderator
        Censored = 1 << 2,  // Content was censored (asterisks)
        Filtered = 1 << 3   // Partial filter applied
    }
}

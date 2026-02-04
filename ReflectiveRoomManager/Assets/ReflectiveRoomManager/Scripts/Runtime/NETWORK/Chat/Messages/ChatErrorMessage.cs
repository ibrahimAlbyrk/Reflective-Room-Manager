using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Chat.Messages
{
    /// <summary>
    /// Server to Client: Error response for failed chat requests.
    /// </summary>
    public struct ChatErrorMessage : NetworkMessage
    {
        /// <summary>Error description</summary>
        public string Error;

        /// <summary>Error code for programmatic handling</summary>
        public ChatErrorCode Code;
    }

    /// <summary>
    /// Error codes for chat operations.
    /// </summary>
    public enum ChatErrorCode : byte
    {
        None = 0,
        RateLimited = 1,
        Muted = 2,
        InvalidContent = 3,
        NoPermission = 4,
        TargetNotFound = 5,
        ContentBlocked = 6
    }
}

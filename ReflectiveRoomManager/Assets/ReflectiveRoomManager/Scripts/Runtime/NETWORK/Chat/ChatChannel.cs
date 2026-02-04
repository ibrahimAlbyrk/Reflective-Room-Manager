namespace REFLECTIVE.Runtime.NETWORK.Chat
{
    /// <summary>
    /// Defines available chat channels using flags for multi-channel subscriptions.
    /// </summary>
    [System.Flags]
    public enum ChatChannel : byte
    {
        None    = 0,
        Global  = 1 << 0,  // 1 - All connected players
        Room    = 1 << 1,  // 2 - Room members only
        Team    = 1 << 2,  // 4 - Team members only
        Whisper = 1 << 3,  // 8 - Private 1-to-1
        Custom1 = 1 << 4,  // 16 - Developer extensible
        Custom2 = 1 << 5,  // 32 - Developer extensible
        Custom3 = 1 << 6,  // 64 - Developer extensible
        System  = 1 << 7   // 128 - Server announcements
    }
}

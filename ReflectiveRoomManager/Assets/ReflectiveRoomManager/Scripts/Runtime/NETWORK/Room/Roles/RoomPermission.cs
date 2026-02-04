using System;

namespace REFLECTIVE.Runtime.NETWORK.Room.Roles
{
    /// <summary>
    /// Room permission flags for bitwise operations.
    /// Use HasFlag() or bitwise &amp; for fast checks.
    /// </summary>
    [Flags]
    public enum RoomPermission
    {
        None = 0,

        // Communication
        ChatSend = 1 << 0,           // 1    - Send chat messages
        VoiceComm = 1 << 1,          // 2    - Voice communication

        // Moderation
        Kick = 1 << 2,               // 4    - Kick players
        Ban = 1 << 3,                // 8    - Ban players (permanent)
        Mute = 1 << 4,               // 16   - Mute players

        // Room Control
        ChangeSettings = 1 << 5,     // 32   - Change room settings
        StartGame = 1 << 6,          // 64   - Start game
        EndGame = 1 << 7,            // 128  - End game
        AssignTeams = 1 << 8,        // 256  - Assign players to teams
        TeamBalance = 1 << 9,        // 512  - Balance teams
        ForceSpectator = 1 << 10,    // 1024 - Force player to spectator

        // Owner-Only
        CloseRoom = 1 << 11,         // 2048 - Close room
        TransferOwnership = 1 << 12, // 4096 - Transfer ownership

        // Convenience combinations
        AllCommunication = ChatSend | VoiceComm,
        AllModeration = Kick | Ban | Mute,
        AllGameControl = StartGame | EndGame | AssignTeams | TeamBalance,
        AllOwner = CloseRoom | TransferOwnership,
        All = ~None
    }
}

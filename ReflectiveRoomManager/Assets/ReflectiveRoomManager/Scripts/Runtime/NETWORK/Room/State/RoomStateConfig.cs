using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.State
{
    /// <summary>
    /// ScriptableObject configuration for room state machine.
    /// Designer-friendly settings.
    /// </summary>
    [CreateAssetMenu(fileName = "RoomStateConfig", menuName = "REFLECTIVE/Room State Config")]
    public class RoomStateConfig : ScriptableObject
    {
        [Header("Lobby Settings")]
        [Tooltip("Minimum players required to start the game")]
        [Range(1, 64)]
        public int MinPlayersToStart = 2;

        [Tooltip("How long players have to ready up before countdown starts (seconds)")]
        public float LobbyReadyTimeout = 60f;

        [Tooltip("Countdown duration in lobby before transitioning to Starting (seconds)")]
        public float LobbyCountdownDuration = 5f;

        [Tooltip("Require all players to be ready before starting")]
        public bool RequireAllPlayersReady = true;

        [Header("Starting Settings")]
        [Tooltip("Countdown duration before game starts (seconds)")]
        public float StartingCountdownDuration = 3f;

        [Header("Playing Settings")]
        [Tooltip("Maximum game duration (0 = unlimited)")]
        public float MaxGameDuration = 600f;

        [Tooltip("Allow pausing during gameplay")]
        public bool AllowPausing = true;

        [Tooltip("Who can pause the game")]
        public PausePermission PausePermission = PausePermission.Anyone;

        [Tooltip("Allow players to join during active gameplay")]
        public bool AllowJoinDuringPlay;

        [Header("Paused Settings")]
        [Tooltip("Auto-resume after this duration (0 = manual only)")]
        public float PauseTimeout = 30f;

        [Header("Ended Settings")]
        [Tooltip("Duration to display end screen before returning to lobby")]
        public float EndScreenDuration = 10f;

        [Tooltip("Automatically return to lobby after end screen")]
        public bool AutoReturnToLobby = true;

        [Tooltip("Auto-close room after game ends (if not returning to lobby)")]
        public bool AutoCloseRoomOnEnd;

        [Header("Advanced")]
        [Tooltip("Enable state transition logging")]
        public bool EnableDebugLogs = true;
    }

    public enum PausePermission
    {
        Anyone,
        OwnerOnly,
        AdminsOnly
    }
}

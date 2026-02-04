using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.State.Messages
{
    /// <summary>
    /// Sent from client to server to request state action (e.g., pause, ready).
    /// </summary>
    public struct RoomStateActionMessage : NetworkMessage
    {
        public uint RoomID;
        public RoomStateAction Action;
        public string Payload;

        public RoomStateActionMessage(uint roomID, RoomStateAction action, string payload = null)
        {
            RoomID = roomID;
            Action = action;
            Payload = payload ?? string.Empty;
        }
    }

    public enum RoomStateAction : byte
    {
        MarkReady = 0,
        UnmarkReady = 1,
        PauseGame = 2,
        ResumeGame = 3,
        EndGame = 4,
        RestartGame = 5
    }
}

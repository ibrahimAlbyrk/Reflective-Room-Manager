using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.State
{
    /// <summary>
    /// Base interface for all room states.
    /// Defines state lifecycle and event handling.
    /// </summary>
    public interface IRoomState
    {
        /// <summary>State unique name (e.g., "Lobby", "Playing")</summary>
        string StateName { get; }

        /// <summary>Numeric ID for network sync (0-255)</summary>
        byte StateTypeID { get; }

        /// <summary>
        /// Validates if transition to target state is allowed.
        /// </summary>
        /// <param name="targetState">Target state to transition to</param>
        /// <returns>True if transition is valid</returns>
        bool CanTransitionTo(IRoomState targetState);

        /// <summary>
        /// Called when entering this state.
        /// Use for initialization logic (e.g., start countdown).
        /// </summary>
        void OnEnter(Room room, RoomStateContext context);

        /// <summary>
        /// Called when exiting this state.
        /// Use for cleanup logic (e.g., stop timers).
        /// </summary>
        void OnExit(Room room, RoomStateContext context);

        /// <summary>
        /// Called every frame/tick while in this state.
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        void OnUpdate(Room room, RoomStateContext context, float deltaTime);

        /// <summary>
        /// Called when a player joins the room while in this state.
        /// </summary>
        void OnPlayerJoined(Room room, RoomStateContext context, NetworkConnection conn);

        /// <summary>
        /// Called when a player leaves the room while in this state.
        /// </summary>
        void OnPlayerLeft(Room room, RoomStateContext context, NetworkConnection conn);

        /// <summary>
        /// Returns current state data for network sync.
        /// </summary>
        RoomStateData GetStateData(RoomStateContext context);

        /// <summary>
        /// Validates if a player can join while in this state.
        /// Called by RoomManager before AddConnection.
        /// </summary>
        /// <param name="room">Target room</param>
        /// <param name="conn">Joining player connection</param>
        /// <param name="reason">Rejection reason if false</param>
        /// <returns>True if player can join</returns>
        bool CanPlayerJoin(Room room, NetworkConnection conn, out string reason);

        /// <summary>
        /// Validates if a player can leave while in this state.
        /// Called by RoomManager before RemoveConnection.
        /// </summary>
        /// <param name="room">Target room</param>
        /// <param name="conn">Leaving player connection</param>
        /// <param name="reason">Rejection reason if false</param>
        /// <returns>True if player can leave</returns>
        bool CanPlayerLeave(Room room, NetworkConnection conn, out string reason);
    }
}

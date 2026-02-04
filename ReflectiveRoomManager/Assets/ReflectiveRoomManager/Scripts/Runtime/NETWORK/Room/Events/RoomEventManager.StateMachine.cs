using System;

namespace REFLECTIVE.Runtime.NETWORK.Room.Events
{
    using State;

    /// <summary>
    /// Partial class extension for RoomEventManager state machine events.
    /// </summary>
    public partial class RoomEventManager
    {
        /// <summary>
        /// Called on server when room state changes.
        /// Parameters: roomID, previousState, newState
        /// </summary>
        public event Action<uint, IRoomState, IRoomState> OnServerRoomStateChanged;

        internal void Invoke_OnServerRoomStateChanged(uint roomID, IRoomState fromState, IRoomState toState)
        {
            OnServerRoomStateChanged?.Invoke(roomID, fromState, toState);
        }
    }
}

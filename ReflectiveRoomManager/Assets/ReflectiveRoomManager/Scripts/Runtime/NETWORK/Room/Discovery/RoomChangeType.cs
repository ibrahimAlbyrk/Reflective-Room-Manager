namespace REFLECTIVE.Runtime.NETWORK.Room.Discovery
{
    /// <summary>
    /// Type of room change for delta updates.
    /// </summary>
    public enum RoomChangeType : byte
    {
        Added = 0,
        Updated = 1,
        Removed = 2
    }
}

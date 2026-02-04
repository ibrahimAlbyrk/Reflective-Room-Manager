namespace REFLECTIVE.Runtime.NETWORK.Room.Discovery
{
    /// <summary>
    /// Sorting options for room query results.
    /// </summary>
    public enum RoomSortOptions : byte
    {
        None = 0,
        PlayerCountAsc = 1,
        PlayerCountDesc = 2,
        NameAsc = 3,
        NameDesc = 4,
        CreationTimeAsc = 5,
        CreationTimeDesc = 6
    }
}

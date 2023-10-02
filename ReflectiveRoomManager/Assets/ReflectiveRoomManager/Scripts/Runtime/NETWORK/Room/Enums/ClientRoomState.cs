namespace REFLECTIVE.Runtime.NETWORK.Room.Enums
{
    [System.Serializable]
    public enum ClientRoomState : byte
    {
        Created,
        Joined,
        Removed,
        Exited,
        Fail
    }
}
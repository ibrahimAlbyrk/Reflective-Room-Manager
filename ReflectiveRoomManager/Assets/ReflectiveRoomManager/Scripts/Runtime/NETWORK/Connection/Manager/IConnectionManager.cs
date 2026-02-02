namespace REFLECTIVE.Runtime.NETWORK.Connection.Manager
{
    public interface IConnectionManager
    {
        RoomConnections RoomConnections { get; }
        NetworkConnections NetworkConnections { get; }
    }
}

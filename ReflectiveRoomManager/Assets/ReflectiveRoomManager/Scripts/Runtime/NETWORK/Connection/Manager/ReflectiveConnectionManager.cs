namespace REFLECTIVE.Runtime.NETWORK.Connection.Manager
{
    /// <summary>
    /// Document: https://reflective-roommanager.gitbook.io/docs/user-manual/network/connection/connectionmanager
    /// </summary>
    public static class ReflectiveConnectionManager
    {
        public static RoomConnections roomConnections => _roomConnections ??= new RoomConnections();

        public static NetworkConnections networkConnections => _networkConnections ??= new NetworkConnections();

        private static RoomConnections _roomConnections;
        private static NetworkConnections _networkConnections;
    }
}
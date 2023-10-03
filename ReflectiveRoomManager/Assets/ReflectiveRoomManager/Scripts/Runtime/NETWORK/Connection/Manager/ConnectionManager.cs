namespace REFLECTIVE.Runtime.NETWORK.Connection.Manager
{
    public static class ConnectionManager
    {
        public static RoomConnections roomConnections => _roomConnections ??= new RoomConnections();

        public static NetworkConnections networkConnections => _networkConnections ??= new NetworkConnections();

        private static RoomConnections _roomConnections;
        private static NetworkConnections _networkConnections;
    }
}
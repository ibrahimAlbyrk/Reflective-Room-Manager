namespace REFLECTIVE.Runtime.NETWORK.Connection.Manager
{
    public static class ReflectiveConnectionManager
    {
        private static readonly RoomConnections _roomConnections = new();
        private static readonly NetworkConnections _networkConnections = new();

        public static RoomConnections roomConnections => _roomConnections;
        public static NetworkConnections networkConnections => _networkConnections;
    }
}
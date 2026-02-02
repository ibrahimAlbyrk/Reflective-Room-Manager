namespace REFLECTIVE.Runtime.NETWORK.Connection.Manager
{
    public class ReflectiveConnectionManager : IConnectionManager
    {
        private static readonly ReflectiveConnectionManager _instance = new();

        private readonly RoomConnections _roomConnections = new();
        private readonly NetworkConnections _networkConnections = new();

        public static ReflectiveConnectionManager Instance => _instance;

        // Static accessors for backward compatibility
        public static RoomConnections roomConnections => _instance._roomConnections;
        public static NetworkConnections networkConnections => _instance._networkConnections;

        // Interface implementation
        RoomConnections IConnectionManager.RoomConnections => _roomConnections;
        NetworkConnections IConnectionManager.NetworkConnections => _networkConnections;
    }
}
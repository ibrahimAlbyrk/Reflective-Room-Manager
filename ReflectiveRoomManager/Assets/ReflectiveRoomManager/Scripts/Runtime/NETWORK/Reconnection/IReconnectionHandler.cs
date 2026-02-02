using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Reconnection
{
    using Room;

    public interface IReconnectionHandler
    {
        float GracePeriodSeconds { get; }
        bool CanReconnect(string playerId, Room room);
        void OnReconnected(string playerId, NetworkConnectionToClient newConn, Room room);
        void OnGracePeriodExpired(string playerId, Room room);
    }
}

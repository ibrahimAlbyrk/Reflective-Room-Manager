using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Reconnection
{
    public interface IPlayerIdentityProvider
    {
        string GetOrAssignPlayerId(NetworkConnectionToClient conn, string clientProvidedId);
        void RemovePlayer(string playerId);
    }
}

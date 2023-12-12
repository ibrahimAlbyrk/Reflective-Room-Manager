using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Connection
{
    using Data;
    
    public class NetworkConnections
    {
        //SERVER SIDE
        public readonly ConnectionEvent OnServerStarted = new(false);
        public readonly ConnectionEvent OnServerStopped = new(false);
        public readonly ConnectionEvent<NetworkConnection> OnServerConnected = new(false);
        public readonly ConnectionEvent<NetworkConnectionToClient> OnServerDisconnected = new(false);

        //CLIENT SIDE
        public readonly ConnectionEvent OnClientStarted = new(false);
        public readonly ConnectionEvent OnClientStopped = new(false);
        public readonly ConnectionEvent OnClientConnected = new(false);
        public readonly ConnectionEvent OnClientDisconnected = new(false);
    }
}
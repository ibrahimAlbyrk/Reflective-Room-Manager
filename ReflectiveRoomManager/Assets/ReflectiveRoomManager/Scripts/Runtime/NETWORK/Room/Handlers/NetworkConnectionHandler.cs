using System;
using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Handlers
{
    using Connection.Manager;
    
    public class NetworkConnectionHandler
    {
        public void OnStartServer(Action callback)
        {
            ConnectionManager.networkConnections.OnStartedServer += callback;
        }

        public void OnStopServer(Action callback)
        {
            ConnectionManager.networkConnections.OnStoppedServer += callback;
        }

        public void OnServerConnect(Action<NetworkConnection> callback)
        {
            ConnectionManager.networkConnections.OnServerConnected += callback;
        }

        public void OnServerDisconnect(Action<NetworkConnectionToClient> callback)
        {
            ConnectionManager.networkConnections.OnServerDisconnected += callback;
        }

        public void OnStartClient(Action callback)
        {
            ConnectionManager.networkConnections.OnStartedClient += callback;
        }

        public void OnStopClient(Action callback)
        {
            ConnectionManager.networkConnections.OnStoppedClient += callback;
        }

        public void OnClientConnect(Action callback)
        {
            ConnectionManager.networkConnections.OnClientConnected += callback;
        }

        public void OnClientDisconnect(Action callback)
        {
            ConnectionManager.networkConnections.OnClientDisconnected += callback;
        }
    }
}
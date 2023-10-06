using System;
using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Connection
{
    public class NetworkConnections
    {
        #region Events

        //Server Side
        public Action OnStartedServer;
        public Action OnStoppedServer;
        public Action<NetworkConnection> OnServerConnected;
        public Action<NetworkConnectionToClient> OnServerDisconnected;
        
        //Client Side
        public Action OnStartedClient;
        public Action OnStoppedClient;
        public Action OnClientConnected;
        public Action OnClientDisconnected;

        #endregion
    }
}
using System;
using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Handlers
{
    using Connection.Manager;
    
    internal static class NetworkConnectionHandler
    {
        internal static void OnStartServer(Action callback)
        {
            ReflectiveConnectionManager.networkConnections.OnServerStarted_AddListener(callback);
        }

        internal static void OnStopServer(Action callback)
        {
            ReflectiveConnectionManager.networkConnections.OnServerStopped_AddListener(callback);
        }

        internal static void OnServerConnect(Action<NetworkConnection> callback)
        {
            ReflectiveConnectionManager.networkConnections.OnServerConnected_AddListener(callback);
        }

        internal static void OnServerDisconnect(Action<NetworkConnectionToClient> callback)
        {
            ReflectiveConnectionManager.networkConnections.OnServerDisconnected_AddListener(callback);
        }

        internal static void OnStartClient(Action callback)
        {
            ReflectiveConnectionManager.networkConnections.OnClientStarted_AddListener(callback);
        }

        internal static void OnStopClient(Action callback)
        {
            ReflectiveConnectionManager.networkConnections.OnClientStopped_AddListener(callback);
        }

        internal static void OnClientConnect(Action callback)
        {
            ReflectiveConnectionManager.networkConnections.OnClientConnected_AddListener(callback);
        }

        internal static void OnClientDisconnect(Action callback)
        {
            ReflectiveConnectionManager.networkConnections.OnClientDisconnected_AddListener(callback);
        }
    }
}
using System;
using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Handlers
{
    using Connection.Manager;
    
    internal static class NetworkConnectionHandler
    {
        internal static void OnServerStart(Action callback)
        {
            ReflectiveConnectionManager.networkConnections.OnServerStarted.AddListener(callback);
        }

        internal static void OnServerStop(Action callback)
        {
            ReflectiveConnectionManager.networkConnections.OnServerStopped.AddListener(callback);
        }

        internal static void OnServerConnect(Action<NetworkConnection> callback)
        {
            ReflectiveConnectionManager.networkConnections.OnServerConnected.AddListener(callback);
        }

        internal static void OnServerDisconnect(Action<NetworkConnectionToClient> callback)
        {
            ReflectiveConnectionManager.networkConnections.OnServerDisconnected.AddListener(callback);
        }

        internal static void OnClientStart(Action callback)
        {
            ReflectiveConnectionManager.networkConnections.OnClientStarted.AddListener(callback);
        }

        internal static void OnClientStop(Action callback)
        {
            ReflectiveConnectionManager.networkConnections.OnClientStopped.AddListener(callback);
        }

        internal static void OnClientConnect(Action callback)
        {
            ReflectiveConnectionManager.networkConnections.OnClientConnected.AddListener(callback);
        }

        internal static void OnClientDisconnect(Action callback)
        {
            ReflectiveConnectionManager.networkConnections.OnClientDisconnected.AddListener(callback);
        }
    }
}
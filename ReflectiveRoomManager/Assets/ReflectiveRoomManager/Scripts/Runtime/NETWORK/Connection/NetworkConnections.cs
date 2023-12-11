using System;
using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Connection
{
    public class NetworkConnections
    {
        #region Events

        //SERVER SIDE
        private Action _onServerStarted;
        private Action _onServerStopped;
        private Action<NetworkConnection> _onServerConnected;
        private Action<NetworkConnectionToClient> _onServerDisconnected;

        //CLIENT SIDE
        private Action _onClientStarted;
        private Action _onClientStopped;
        private Action _onClientConnected;
        private Action _onClientDisconnected;
        
        #endregion

        #region Add/Remove Methods

        //SERVER SIDE
        public void OnServerStarted_AddListener(Action action) => _onServerStarted += action;
        public void OnServerStopped_AddListener(Action action) => _onServerStopped += action;
        public void OnServerConnected_AddListener(Action<NetworkConnection> action) => _onServerConnected += action;
        public void OnServerDisconnected_AddListener(Action<NetworkConnectionToClient> action) => _onServerDisconnected += action;

        public void OnServerStarted_RemoveListener(Action action) => _onServerStarted -= action;
        public void OnServerStopped_RemoveListener(Action action) => _onServerStopped -= action;
        public void OnServerConnected_RemoveListener(Action<NetworkConnection> action) => _onServerConnected -= action;
        public void OnServerDisconnected_RemoveListener(Action<NetworkConnectionToClient> action) => _onServerDisconnected -= action;

        //CLIENT SIDE
        public void OnClientStarted_AddListener(Action action) => _onClientStarted += action;
        public void OnClientStopped_AddListener(Action action) => _onClientStopped += action;
        public void OnClientConnected_AddListener(Action action) => _onClientConnected += action;
        public void OnClientDisconnected_AddListener(Action action) => _onClientDisconnected += action;

        public void OnClientStarted_RemoveListener(Action action) => _onClientStarted -= action;
        public void OnClientStopped_RemoveListener(Action action) => _onClientStopped -= action;
        public void OnClientConnected_RemoveListener(Action action) => _onClientConnected -= action;
        public void OnClientDisconnected_RemoveListener(Action action) => _onClientDisconnected -= action;

        #endregion

        #region Call Methods

        //SERVER SIDE
        internal void OnServerStarted_Call()
        {
            Debug.Log("OnServerStarted");
            
            _onServerStarted?.Invoke();
        }
        
        internal void OnServerStopped_Call()
        {
            Debug.Log("OnServerStopped");
            
            _onServerStopped?.Invoke();
        }
        
        internal void OnServerConnected_Call(NetworkConnection conn)
        {
            Debug.Log("OnServerConnected");
            
            _onServerConnected?.Invoke(conn);
        }
        
        internal void OnServerDisconnected_Call(NetworkConnectionToClient conn)
        {
            Debug.Log("OnServerDisconnected");
            
            _onServerDisconnected?.Invoke(conn);
        }
        
        //CLIENT SIDE
        internal void OnClientStarted_Call()
        {
            Debug.Log("OnClientStarted");
            
            _onClientStarted?.Invoke();
        }
        
        internal void OnClientStopped_Call()
        {
            Debug.Log("OnClientStopped");
            
            _onClientStopped?.Invoke();
        }
        
        internal void OnClientConnected_Call()
        {
            Debug.Log("OnClientConnected");
            
            _onClientConnected?.Invoke();
        }
        
        internal void OnClientDisconnected_Call()
        {
            Debug.Log("OnClientDisconnected");
            
            _onClientDisconnected?.Invoke();
        }

        #endregion
    }
}
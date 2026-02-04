using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room
{
    using Chat;

    public abstract partial class RoomManagerBase
    {
        private void InitializeChatSystem()
        {
            if (!_enableChatSystem) return;

            if (_chatSettings == null)
            {
                Debug.LogError("[RoomManagerBase] Chat system enabled but no ChatSettings assigned!");
                _enableChatSystem = false;
                return;
            }

            // Create ChatManager if not present
            _chatManager = GetComponent<ChatManager>();
            if (_chatManager == null)
            {
                _chatManager = gameObject.AddComponent<ChatManager>();
            }

            // Assign settings (required for runtime-created ChatManager)
            _chatManager.SetSettings(_chatSettings);

            // Subscribe to server/client lifecycle for chat initialization
            _connectionManager.NetworkConnections.OnServerStarted.AddListener(OnChatServerStarted);
            _connectionManager.NetworkConnections.OnServerStopped.AddListener(OnChatServerStopped);
            _connectionManager.NetworkConnections.OnClientStarted.AddListener(OnChatClientStarted);
            _connectionManager.NetworkConnections.OnClientStopped.AddListener(OnChatClientStopped);
            _connectionManager.NetworkConnections.OnServerDisconnected.AddListener(OnChatPlayerDisconnected);

            Debug.Log("[RoomManagerBase] Chat system initialized.");
        }

        private void CleanupChatSystem()
        {
            if (!_enableChatSystem || _chatManager == null) return;

            _connectionManager.NetworkConnections.OnServerStarted.RemoveListener(OnChatServerStarted);
            _connectionManager.NetworkConnections.OnServerStopped.RemoveListener(OnChatServerStopped);
            _connectionManager.NetworkConnections.OnClientStarted.RemoveListener(OnChatClientStarted);
            _connectionManager.NetworkConnections.OnClientStopped.RemoveListener(OnChatClientStopped);
            _connectionManager.NetworkConnections.OnServerDisconnected.RemoveListener(OnChatPlayerDisconnected);
        }

        private void OnChatServerStarted()
        {
            _chatManager?.Initialize();
        }

        private void OnChatServerStopped()
        {
            _chatManager?.CleanupServer();
        }

        private void OnChatClientStarted()
        {
            _chatManager?.Initialize();
        }

        private void OnChatClientStopped()
        {
            _chatManager?.CleanupClient();
        }

        private void OnChatPlayerDisconnected(NetworkConnectionToClient conn)
        {
            _chatManager?.OnPlayerDisconnected((uint)conn.connectionId);
        }
    }
}

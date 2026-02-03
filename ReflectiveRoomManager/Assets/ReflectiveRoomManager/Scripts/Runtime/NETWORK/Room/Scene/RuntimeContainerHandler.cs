using System;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.NETWORK.Room.Scenes
{
    using Structs;

    internal class RuntimeContainerHandler
    {
        private const string RUNTIME_CONTAINER_NAME = "__RuntimeContainer__";

        private Scene _currentContainer;
        private uint _pendingRoomID;

        internal void RegisterClientHandlers()
        {
            NetworkClient.RegisterHandler<ContainerSceneMessage>(OnContainerSceneMessage);
        }

        internal void UnregisterClientHandlers()
        {
            NetworkClient.UnregisterHandler<ContainerSceneMessage>();
            Cleanup();
        }

        private void OnContainerSceneMessage(ContainerSceneMessage msg)
        {
            // Host mode: server already has the scene
            if (NetworkServer.active)
            {
                SendReadyAcknowledgment(msg.RoomID, true);
                return;
            }

            _pendingRoomID = msg.RoomID;

            if (msg.UseRuntimeContainer)
            {
                CreateRuntimeContainer(msg.PhysicsMode);
            }
            else
            {
                LoadCustomContainer(msg.CustomContainerScene);
            }
        }

        private void CreateRuntimeContainer(LocalPhysicsMode physicsMode)
        {
            CleanupExistingContainer();

            try
            {
                _currentContainer = SceneManager.CreateScene(
                    RUNTIME_CONTAINER_NAME,
                    new CreateSceneParameters { localPhysicsMode = physicsMode }
                );

                if (!_currentContainer.IsValid())
                {
                    Debug.LogError("[RuntimeContainerHandler] Failed to create runtime container");
                    SendReadyAcknowledgment(_pendingRoomID, false);
                    return;
                }

                SceneManager.SetActiveScene(_currentContainer);
                SendReadyAcknowledgment(_pendingRoomID, true);
            }
            catch (Exception e)
            {
                Debug.LogError($"[RuntimeContainerHandler] Exception: {e.Message}");
                SendReadyAcknowledgment(_pendingRoomID, false);
            }
        }

        private void LoadCustomContainer(string scenePath)
        {
            CleanupExistingContainer();

            var op = SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Single);
            if (op == null)
            {
                Debug.LogError($"[RuntimeContainerHandler] Failed to load container: {scenePath}");
                SendReadyAcknowledgment(_pendingRoomID, false);
                return;
            }

            op.completed += _ =>
            {
                // Scene path may contain folder structure, get just the name
                var sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                _currentContainer = SceneManager.GetSceneByName(sceneName);

                // Fallback: if not found by name, get the active scene
                if (!_currentContainer.IsValid())
                {
                    _currentContainer = SceneManager.GetActiveScene();
                }

                SendReadyAcknowledgment(_pendingRoomID, true);
            };
        }

        private void SendReadyAcknowledgment(uint roomId, bool success)
        {
            NetworkClient.Send(new ContainerReadyMessage(roomId, success));
        }

        private void CleanupExistingContainer()
        {
            if (_currentContainer.IsValid() && _currentContainer.isLoaded)
            {
                if (_currentContainer.name == RUNTIME_CONTAINER_NAME)
                {
                    SceneManager.UnloadSceneAsync(_currentContainer);
                }
            }
        }

        internal void Cleanup()
        {
            CleanupExistingContainer();
            _currentContainer = default;
        }
    }
}

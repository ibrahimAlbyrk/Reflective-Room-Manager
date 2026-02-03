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

            if (msg.UseRuntimeContainer)
            {
                CreateRuntimeContainer(msg.RoomID, msg.PhysicsMode);
            }
            else
            {
                LoadCustomContainer(msg.RoomID, msg.CustomContainerScene);
            }
        }

        private void CreateRuntimeContainer(uint roomId, LocalPhysicsMode physicsMode)
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
                    SendReadyAcknowledgment(roomId, false);
                    return;
                }

                SceneManager.SetActiveScene(_currentContainer);
                SendReadyAcknowledgment(roomId, true);
            }
            catch (Exception e)
            {
                Debug.LogError($"[RuntimeContainerHandler] Exception: {e.Message}");
                SendReadyAcknowledgment(roomId, false);
            }
        }

        private void LoadCustomContainer(uint roomId, string scenePath)
        {
            CleanupExistingContainer();

            AsyncOperation op;
            try
            {
                op = SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Single);
            }
            catch (Exception e)
            {
                Debug.LogError($"[RuntimeContainerHandler] Failed to start scene load: {e.Message}");
                SendReadyAcknowledgment(roomId, false);
                return;
            }

            if (op == null)
            {
                Debug.LogError($"[RuntimeContainerHandler] Failed to load container: {scenePath}");
                SendReadyAcknowledgment(roomId, false);
                return;
            }

            // Capture roomId in closure to avoid race condition
            var capturedRoomId = roomId;
            var capturedScenePath = scenePath;

            op.completed += _ =>
            {
                var sceneName = System.IO.Path.GetFileNameWithoutExtension(capturedScenePath);
                _currentContainer = SceneManager.GetSceneByName(sceneName);

                if (!_currentContainer.IsValid())
                {
                    _currentContainer = SceneManager.GetActiveScene();
                }

                if (!_currentContainer.IsValid())
                {
                    Debug.LogError($"[RuntimeContainerHandler] Scene loaded but not found: {sceneName}");
                    SendReadyAcknowledgment(capturedRoomId, false);
                    return;
                }

                SendReadyAcknowledgment(capturedRoomId, true);
            };
        }

        private void SendReadyAcknowledgment(uint roomId, bool success)
        {
            if (!NetworkClient.active) return;
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

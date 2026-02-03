using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.Scenes
{
    using Enums;
    using Structs;
    using Utilities;

    internal class RoomSceneSynchronizer
    {
        private readonly RoomManagerBase _roomManager;
        private readonly PendingContainerState _pendingStates;

        internal RoomSceneSynchronizer(RoomManagerBase roomManager)
        {
            _roomManager = roomManager;
            _pendingStates = new PendingContainerState();
        }

        internal void RegisterServerHandlers()
        {
            NetworkServer.RegisterHandler<ContainerReadyMessage>(OnContainerReady);
        }

        internal void UnregisterServerHandlers()
        {
            NetworkServer.UnregisterHandler<ContainerReadyMessage>();
            _pendingStates.Clear();
        }

        internal void DoSyncScene(NetworkConnection conn, uint roomID)
        {
            var room = _roomManager.GetRoom(roomID);
            if (room == null) return;

            _pendingStates.Add(conn, roomID);

            conn.Send(new ContainerSceneMessage(
                _roomManager.UseRuntimeContainer,
                _roomManager.ClientContainerScene,
                _roomManager.PhysicsMode,
                roomID
            ));
        }

        private void OnContainerReady(NetworkConnectionToClient conn, ContainerReadyMessage msg)
        {
            if (!_pendingStates.TryGet(conn, out var expectedRoomId))
            {
                Debug.LogWarning($"[RoomSceneSynchronizer] Unexpected ContainerReadyMessage from connection {conn.connectionId}");
                return;
            }

            if (msg.RoomID != expectedRoomId)
            {
                Debug.LogWarning($"[RoomSceneSynchronizer] RoomID mismatch: expected {expectedRoomId}, got {msg.RoomID}");
                return;
            }

            _pendingStates.Remove(conn);

            if (!msg.Success)
            {
                Debug.LogError($"[RoomSceneSynchronizer] Client {conn.connectionId} failed to load container scene");
                RoomMessageUtility.SendRoomMessage(conn, ClientRoomState.Fail);
                return;
            }

            SendRoomScene(conn, msg.RoomID);
        }

        private void SendRoomScene(NetworkConnection conn, uint roomID)
        {
            var room = _roomManager.GetRoom(roomID);
            if (room == null) return;

            conn.Send(new SceneMessage
            {
                sceneName = room.Scene.name,
                sceneOperation = SceneOperation.LoadAdditive
            });
        }

        internal void RemovePendingState(NetworkConnection conn)
        {
            _pendingStates.Remove(conn);
        }
    }
}

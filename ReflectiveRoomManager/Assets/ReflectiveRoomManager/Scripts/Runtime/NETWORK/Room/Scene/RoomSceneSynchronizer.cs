using Mirror;
using System.Collections;
using REFLECTIVE.Runtime.MonoBehavior;

namespace REFLECTIVE.Runtime.NETWORK.Room.Scenes
{
    internal class RoomSceneSynchronizer
    {
        private readonly RoomManagerBase _roomManager;

        internal RoomSceneSynchronizer(RoomManagerBase roomManager)
        {
            _roomManager = roomManager;
        }

        internal void DoSyncScene(NetworkConnection conn, uint roomID)
        {
            CoroutineRunner.Instance.StartCoroutine(DoSyncScene_Cor(conn, roomID));
        }

        private IEnumerator DoSyncScene_Cor(NetworkConnection conn, uint roomID)
        {
            var room = _roomManager.GetRoom(roomID);

            if (room == null) yield break;

            var containerSceneName = _roomManager.ClientContainerScene;

            conn.Send(new SceneMessage { sceneName = containerSceneName, sceneOperation = SceneOperation.Normal });

            conn.Send(new SceneMessage { sceneName = room.Scene.name, sceneOperation = SceneOperation.LoadAdditive });
        }
    }
}

using Mirror;
using UnityEngine;
using System.Collections;

namespace REFLECTIVE.Runtime.NETWORK.Room.Scenes
{
    internal static class RoomSceneSynchronizer
    {
        internal static void DoSyncScene(NetworkConnection conn, uint roomID)
        {
            RoomManagerBase.Instance.StartCoroutine(DoSyncScene_Cor(conn, roomID));
        }

        private static IEnumerator DoSyncScene_Cor(NetworkConnection conn, uint roomID)
        {
            var room = RoomManagerBase.Instance.GetRoom(roomID);

            if (room == null) yield break;

            var containerSceneName = RoomManagerBase.Instance.ClientContainerScene;
            
            conn.Send(new SceneMessage { sceneName = containerSceneName, sceneOperation = SceneOperation.Normal });
            
            conn.Send(new SceneMessage{sceneName = room.Scene.name, sceneOperation = SceneOperation.LoadAdditive});
        }
    }
}
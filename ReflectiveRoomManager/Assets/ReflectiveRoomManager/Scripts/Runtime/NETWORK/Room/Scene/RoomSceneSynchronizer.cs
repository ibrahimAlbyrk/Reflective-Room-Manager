using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Scenes
{
    internal static class RoomSceneSynchronizer
    {
        internal static void DoSyncScene(NetworkConnection conn)
        {
            var room = RoomManagerBase.Instance.GetRoomOfPlayer(conn);

            if (room == null) return;
            
            conn.Send(new SceneMessage{sceneName = room.Scene.name, sceneOperation = SceneOperation.Normal});
        }
    }
}
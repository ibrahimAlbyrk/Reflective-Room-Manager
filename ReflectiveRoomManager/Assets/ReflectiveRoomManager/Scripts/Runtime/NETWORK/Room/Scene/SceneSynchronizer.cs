using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Scenes
{
    public class SceneSynchronizer
    {
        public SceneSynchronizer()
        {
            RoomManagerBase.Instance.Events.OnServerJoinedRoom += DoSyncScene;
        }

        private static void DoSyncScene(NetworkConnection conn)
        {
            var room = RoomManagerBase.Instance.GetRoomOfPlayer(conn);

            if (room == null) return;
            
            conn.Send(new SceneMessage{sceneName = room.Scene.name, sceneOperation = SceneOperation.Normal});
        }
    }
}
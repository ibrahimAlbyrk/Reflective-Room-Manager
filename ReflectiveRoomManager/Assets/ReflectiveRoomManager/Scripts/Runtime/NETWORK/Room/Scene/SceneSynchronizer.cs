using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Scenes
{
    public class SceneSynchronizer
    {
        public SceneSynchronizer()
        {
            RoomManagerBase.Singleton.Events.OnServerJoinedRoom += DoSyncScene;
        }

        private static void DoSyncScene(NetworkConnection conn)
        {
            var room = RoomManagerBase.Singleton.GetRoomOfPlayer(conn);

            if (room == null) return;
            
            conn.Send(new SceneMessage{sceneName = room.Scene.name, sceneOperation = SceneOperation.Normal});
        }
    }
}
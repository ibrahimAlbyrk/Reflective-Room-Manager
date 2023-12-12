using System;

namespace REFLECTIVE.Runtime.NETWORK.Room.Loader
{
    using Structs;
    using Connection.Manager;
    using SceneManagement.Manager;
    
    public class SceneRoomLoader : IRoomLoader
    {
        public void LoadRoom(Room room, RoomInfo roomInfo, Action onLoaded)
        {
            ReflectiveSceneManager.LoadScene(roomInfo.SceneName, 
                scene =>
                {
                    room.Scene = scene;
                    onLoaded?.Invoke();
                    
                    ReflectiveConnectionManager.roomConnections.OnServerRoomSceneLoaded.Call(scene);
                });
        }

        public void UnLoadRoom(Room room)
        {
            ReflectiveSceneManager.UnLoadScene(room.Scene);
        }
    }
}
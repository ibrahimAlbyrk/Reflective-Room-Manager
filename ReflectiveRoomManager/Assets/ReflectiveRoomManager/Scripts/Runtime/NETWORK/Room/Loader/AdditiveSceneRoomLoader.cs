using System;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.NETWORK.Room.Loader
{
    using Structs;
    using SceneManagement;
    
    public class AdditiveSceneRoomLoader : IRoomLoader
    {
        public void LoadRoom(Room room, RoomInfo roomInfo, Action onLoaded)
        {
            SceneManager.LoadScene(roomInfo.SceneName, LoadSceneMode.Additive, 
                scene =>
                {
                    room.Scene = scene;
                    onLoaded?.Invoke();
                });
        }

        public void UnLoadRoom(Room room)
        {
            SceneManager.UnLoadScene(room.Scene);
        }
    }
}
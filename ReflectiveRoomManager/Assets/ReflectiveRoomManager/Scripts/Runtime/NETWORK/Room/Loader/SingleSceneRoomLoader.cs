using System;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.NETWORK.Room.Loader
{
    using Structs;
    using SceneManagement;
    
    public class SingleSceneRoomLoader : IRoomLoader
    {
        private string _beforeSceneName;
        
        public void LoadRoom(Room room, RoomInfo roomInfo, Action onLoaded)
        {
            _beforeSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            
            SceneManager.LoadScene(roomInfo.SceneName, LoadSceneMode.Single, 
                scene =>
                {
                    room.Scene = scene;
                    onLoaded?.Invoke();
                });
        }

        public void UnLoadRoom(Room room)
        {
            SceneManager.LoadScene(_beforeSceneName);
        }
    }
}
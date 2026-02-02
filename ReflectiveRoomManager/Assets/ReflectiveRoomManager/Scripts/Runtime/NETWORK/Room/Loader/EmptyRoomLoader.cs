using System;

#pragma warning disable 0067

namespace REFLECTIVE.Runtime.NETWORK.Room.Loader
{
    using Structs;

    public class NoneSceneRoomLoader : IRoomLoader
    {
        public event Action<float> OnLoadProgress;

        public void LoadRoom(Room room, RoomInfo roomInfo, Action onLoaded)
        {
            onLoaded?.Invoke();
        }

        public void UnLoadRoom(Room room)
        {

        }
    }
}

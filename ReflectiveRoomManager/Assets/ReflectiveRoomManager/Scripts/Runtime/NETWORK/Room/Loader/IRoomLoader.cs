using System;

namespace REFLECTIVE.Runtime.NETWORK.Room.Loader
{
    using Structs;

    public interface IRoomLoader
    {
        event Action<float> OnLoadProgress;
        void LoadRoom(Room room, RoomInfo roomInfo, Action onLoaded);
        void UnLoadRoom(Room room);
    }
}
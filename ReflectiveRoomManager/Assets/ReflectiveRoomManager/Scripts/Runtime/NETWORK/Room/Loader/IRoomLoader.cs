using System;

namespace REFLECTIVE.Runtime.NETWORK.Room.Loader
{
    using Structs;

    public interface IRoomLoader
    {
        public void LoadRoom(Room room, RoomInfo roomInfo, Action onLoaded);
        public void UnLoadRoom(Room room);
    }
}
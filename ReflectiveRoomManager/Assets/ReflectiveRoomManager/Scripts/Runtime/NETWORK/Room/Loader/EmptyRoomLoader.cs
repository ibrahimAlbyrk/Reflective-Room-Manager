using System;

namespace REFLECTIVE.Runtime.NETWORK.Room.Loader
{
    using Structs;
    
    public class EmptyRoomLoader : IRoomLoader
    {
        public void LoadRoom(Room room, RoomInfo roomInfo, Action onLoaded)
        {
            onLoaded?.Invoke();
        }

        public void UnLoadRoom(Room room)
        {
            
        }
    }
}
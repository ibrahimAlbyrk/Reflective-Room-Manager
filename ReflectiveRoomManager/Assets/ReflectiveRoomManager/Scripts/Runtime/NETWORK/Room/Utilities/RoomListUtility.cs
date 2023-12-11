using System.Linq;
using System.Collections.Generic;

namespace REFLECTIVE.Runtime.NETWORK.Room.Utilities
{
    using Enums;
    using Structs;
    
    public static class RoomListUtility
    {
        public static RoomInfo ConvertToRoomList(Room room)
        {
            return new RoomInfo
            {
                RoomName = room.RoomName,
                MaxPlayers = room.MaxPlayers,
                CurrentPlayers = room.CurrentPlayers,
                CustomData = room.GetCustomData(),
                ConnectionIds = room.Connections.Select(conn => conn.connectionId).ToList()
            };
        }
        
        public static void UpdateRoomToList(ref List<Room> rooms, Room room)
        {
            var index = rooms.FindIndex(info => info.RoomName == room.RoomName);
            
            rooms[index] = room;
            
            var roomList = ConvertToRoomList(room);
            
            RoomMessageUtility.SenRoomUpdateMessage(roomList, RoomMessageState.Update);
        }
        
        public static void AddRoomToList(ref List<Room> rooms, Room room)
        {
            rooms.Add(room);
            
            var roomList = ConvertToRoomList(room);

            RoomMessageUtility.SenRoomUpdateMessage(roomList, RoomMessageState.Add);
        }
        
        public static void RemoveRoomToList(ref List<Room> rooms, Room room)
        {
            if (!rooms.Remove(room)) return;

            var roomList = ConvertToRoomList(room);

            RoomMessageUtility.SenRoomUpdateMessage(roomList, RoomMessageState.Remove);
        }
    }
}
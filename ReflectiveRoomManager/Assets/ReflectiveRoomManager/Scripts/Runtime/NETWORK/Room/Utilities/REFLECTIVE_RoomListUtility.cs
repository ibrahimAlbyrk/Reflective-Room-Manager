using System.Collections.Generic;

namespace REFLECTIVE.Runtime.NETWORK.Room.Utilities
{
    using Enums;
    using Structs;
    
    public static class REFLECTIVE_RoomListUtility
    {
        public static REFLECTIVE_RoomInfo ConvertToRoomList(REFLECTIVE_Room room)
        {
            return new REFLECTIVE_RoomInfo(room.RoomName, room.MaxPlayers, room.CurrentPlayers);
        }
        
        public static void UpdateRoomToList(ref List<REFLECTIVE_Room> rooms, REFLECTIVE_Room reflectiveRoom)
        {
            var index = rooms.FindIndex(info => info.RoomName == reflectiveRoom.RoomName);
            
            rooms[index] = reflectiveRoom;
            
            REFLECTIVE_RoomMessageUtility.SenRoomUpdateMessage(ConvertToRoomList(reflectiveRoom), REFLECTIVE_RoomMessageState.Update);
        }
        
        public static void AddRoomToList(ref List<REFLECTIVE_Room> rooms, REFLECTIVE_Room reflectiveRoom)
        {
            rooms.Add(reflectiveRoom);

            REFLECTIVE_RoomMessageUtility.SenRoomUpdateMessage(ConvertToRoomList(reflectiveRoom), REFLECTIVE_RoomMessageState.Add);
        }
        
        public static void RemoveRoomToList(ref List<REFLECTIVE_Room> rooms, REFLECTIVE_Room reflectiveRoom)
        {
            if (!rooms.Remove(reflectiveRoom)) return;
            
            REFLECTIVE_RoomMessageUtility.SenRoomUpdateMessage(ConvertToRoomList(reflectiveRoom), REFLECTIVE_RoomMessageState.Remove);
        }
    }
}
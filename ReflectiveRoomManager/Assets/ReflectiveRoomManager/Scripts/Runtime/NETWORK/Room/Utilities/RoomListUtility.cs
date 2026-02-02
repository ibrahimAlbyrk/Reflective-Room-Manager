using System.Collections.Generic;

using UnityEngine;

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
                ID = room.ID,
                IsPrivate = room.IsPrivate,
                RoomName = room.Name,
                MaxPlayers = room.MaxPlayers,
                CurrentPlayers = room.CurrentPlayers,
                CustomData = room.GetCustomData()
            };
        }
        
        public static void UpdateRoomToList(List<Room> rooms, Room room)
        {
            var index = rooms.FindIndex(info => info.Name == room.Name);

            if (index == -1)
            {
                Debug.LogError($"Room '{room.Name}' not found in list");
                return;
            }

            rooms[index] = room;
            
            var roomList = ConvertToRoomList(room);
            
            RoomMessageUtility.SenRoomUpdateMessage(roomList, RoomMessageState.Update);
        }
        
        public static void AddRoomToList(List<Room> rooms, Room room)
        {
            rooms.Add(room);
            
            var roomList = ConvertToRoomList(room);

            RoomMessageUtility.SenRoomUpdateMessage(roomList, RoomMessageState.Add);
        }
        
        public static void RemoveRoomToList(List<Room> rooms, Room room)
        {
            if (!rooms.Remove(room)) return;

            var roomList = ConvertToRoomList(room);

            RoomMessageUtility.SenRoomUpdateMessage(roomList, RoomMessageState.Remove);
        }
    }
}
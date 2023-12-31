﻿using UnityEngine;
using System.Collections.Generic;

namespace REFLECTIVE.Runtime.NETWORK.Room.Service
{
    using Structs;
    
    public static class RoomClient
    {
        #region Public Variables

        /// <summary>
        /// ID of the room where the client is joined.
        /// <remarks>Returns -1 if the client is not in the room</remarks>
        /// </summary>
        public static uint CurrentRoomID;

        #endregion
        
        #region Transaction Methods

        public static void CreateRoom(string roomName, string sceneName, int maxPlayers, bool isPrivate = false)
        {
            var roomInfo = new RoomInfo
            {
                RoomName = roomName,
                IsPrivate = isPrivate,
                SceneName = sceneName,
                MaxPlayers = maxPlayers,
                CustomData = new Dictionary<string, string>()
            };
            
            RoomManagerBase.RequestCreateRoom(roomInfo);
        }

        public static void CreateRoom(string roomName, string sceneName, int maxPlayers, params (string, string)[] customData)
        {
            CreateRoom(roomName, sceneName, maxPlayers,false,  customData);
        }
        
        public static void CreateRoom(string roomName, string sceneName, int maxPlayers, bool isPrivate, params (string, string)[] customData)
        {
            var data = new Dictionary<string, string>();

            foreach (var (key, value) in customData)
            {
                data.Add(key, value);
            }

            var roomInfo = new RoomInfo
            {
                RoomName = roomName,
                IsPrivate = isPrivate,
                SceneName = sceneName,
                MaxPlayers = maxPlayers,
                CustomData = data
            };

            RoomManagerBase.RequestCreateRoom(roomInfo);
        }
        
        public static void CreateRoom(string roomName, string sceneName, int maxPlayers, Dictionary<string, string> customData)
        {
            CreateRoom(roomName, sceneName, maxPlayers, false, customData);
        }

        public static void CreateRoom(string roomName, string sceneName, int maxPlayers, bool isPrivate, Dictionary<string, string> customData)
        {
            var roomInfo = new RoomInfo
            {
                RoomName = roomName,
                IsPrivate = isPrivate,
                SceneName = sceneName,
                MaxPlayers = maxPlayers,
                CustomData = customData ?? new Dictionary<string, string>()
            };

            RoomManagerBase.RequestCreateRoom(roomInfo);
        }

        public static void JoinRoom(string roomName)
        {
            RoomManagerBase.RequestJoinRoom(roomName);
        }

        public static void ExitRoom()
        {
            RoomManagerBase.RequestExitRoom();
        }
        
        public static void ExitRoom(bool isDisconnected)
        {
            RoomManagerBase.RequestExitRoom(isDisconnected);
        }

        #endregion

        #region Data Methods
        
        public static string GetRoomCustomData(string dataName)
        {
            var room = RoomManagerBase.Instance.GetRoomOfClient();

            if (!room.CustomData.TryGetValue(dataName, out var dataValue))
            {
                Debug.LogWarning("No such data was found");
                
                return default;
            }

            return dataValue;
        }

        #endregion
    }
}
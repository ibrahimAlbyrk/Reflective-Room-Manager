using System;
using Mirror;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.NETWORK.Room
{
    using Enums;
    using Service;
    using Structs;

    public abstract partial class RoomManagerBase
    {
        #region Get Room Methods

        /// <summary>
        /// Returns a list of all rooms
        /// </summary>
        /// <remarks>Only works on server</remarks>
        /// <returns></returns>
        public IEnumerable<Room> GetRooms() => m_rooms;

        /// <summary>
        /// Retrieve a room based on the given room name.
        /// </summary>
        /// <param name="roomName">The name of the room to retrieve.</param>
        /// <remarks>Only works on server</remarks>
        /// <returns>The room object if found, otherwise null.</returns>
        public Room GetRoom(string roomName)
        {
            var room = m_rooms.FirstOrDefault(room => room.RoomName == roomName);

            return room;
        }

        /// <summary>
        /// Retrieve a room info based on the given room name.
        /// </summary>
        /// <param name="roomName">The name of the room to retrieve information for.</param>
        /// <remarks>Only works on client</remarks>
        /// <returns>The RoomInfo object containing information about the room. Returns null if the room does not exist.</returns>
        public RoomInfo GetRoomInfo(string roomName)
        {
            var roomInfo = _roomListInfos.FirstOrDefault(roomInfo => roomInfo.RoomName == roomName);

            return roomInfo;
        }
        
        /// <summary>
        /// Returns a list of all room infos
        /// </summary>
        /// <remarks>Only works on client</remarks>
        /// <returns></returns>
        public IEnumerable<RoomInfo> GetRoomInfos() => _roomListInfos;

        /// <summary>
        /// The function return information about the room where the "connection" is located
        /// </summary>
        /// <remarks>Only works on server</remarks>
        /// <param name="conn"></param>
        /// <returns>Information about the room where the "connection" is located.</returns>
        public Room GetRoomOfPlayer(NetworkConnection conn)
        {
            return m_rooms.FirstOrDefault(room => room.Connections.Any(connection => connection == conn));
        }

        /// <summary>
        /// The function returns information about the room to which id belongs
        /// </summary>
        /// <remarks>Only works on server</remarks>
        /// <param name="ID"></param>
        /// <returns>Information about the room where the "connection" is located.</returns>
        public Room GetRoomOfID(uint ID)
        {
            return m_rooms.FirstOrDefault(room => room.ID == ID);
        }
        
        /// <summary>
        /// The function returns information about the room info to which id belongs
        /// </summary>
        /// <remarks>Only works on client</remarks>
        /// <param name="ID"></param>
        /// <returns>Information about the room info where the "connection" is located.</returns>
        public RoomInfo GetRoomInfoOfID(uint ID)
        {
            return _roomListInfos.FirstOrDefault(room => room.ID == ID);
        }

        /// <summary>
        /// The function return information about the room where the "connection ID" is located
        /// </summary>
        /// <remarks>Only works on client</remarks>
        /// <returns>Information about the room where the "connection ID" is located.</returns>
        public RoomInfo GetRoomOfClient()
        {
            return _roomListInfos.FirstOrDefault(room => room.ID == RoomClient.CurrentRoomID);
        }

        /// <summary>
        /// The function return information about where the scene* is located
        /// </summary>
        /// <remarks>Only works on server</remarks>
        /// <param name="scene"></param>
        /// <returns>Information about the room where the scene* is located</returns>
        public Room GetRoomOfScene(Scene scene)
        {
            return m_rooms.FirstOrDefault(room => room.Scene == scene);
        }

        #endregion

        #region Request Methods

        /// <summary>
        /// Sends a request to the server for room creation with the specified information
        /// </summary>
        /// <remarks>Only works on client</remarks>
        /// <param name="roomInfo">The <see cref="RoomInfo"/> instance that contains the room's </param>
        internal static void RequestCreateRoom(RoomInfo roomInfo)
        {
            if (NetworkClient.connection == null) return;

            var serverRoomMessage =
                new ServerRoomMessage(ServerRoomState.Create, roomInfo);

            NetworkClient.Send(serverRoomMessage);
        }

        /// <summary>
        /// Sends a request to join the specified room
        /// </summary>
        /// <remarks>Only works on client</remarks>
        /// <param name="roomName"></param>
        internal static void RequestJoinRoom(string roomName)
        {
            if (NetworkClient.connection == null) return;

            var roomInfo = new RoomInfo
            {
                RoomName = roomName
            };

            var serverRoomMessage = new ServerRoomMessage(ServerRoomState.Join, roomInfo);

            NetworkClient.Send(serverRoomMessage);
        }

        /// <summary>
        /// Sends a request to the server to exit the client's room
        /// </summary>
        /// <remarks>Only works on client</remarks>
        /// <param name="isDisconnected"></param>
        internal static void RequestExitRoom(bool isDisconnected = false)
        {
            if (NetworkClient.connection == null) return;

            var serverRoomMessage =
                new ServerRoomMessage(ServerRoomState.Exit, default, isDisconnected);

            NetworkClient.Send(serverRoomMessage);
        }

        #endregion

        #region Room Loader Mehods

        /// <summary>
        /// Loads a specific room with the provided information
        /// and executes a callback after the room is loaded.
        /// </summary>
        /// <param name="room">The room object to load.</param>
        /// <param name="roomInfo">The information of the room to be loaded.</param>
        /// <param name="onLoaded">An optional callback to be executed after the room is loaded.</param>
        /// <exception cref="NullReferenceException">Thrown if the Room Loader is null.</exception>
        protected void LoadRoom(Room room, RoomInfo roomInfo, Action onLoaded = null)
        {
            if (_roomLoader == null)
                throw new NullReferenceException("Room Loader is null");

            _roomLoader.LoadRoom(room, roomInfo, onLoaded);
        }

        /// <summary>
        /// UnLoads a specific room and executes
        /// a callback after the room is unloaded.
        /// </summary>
        /// <param name="room">The room to be unloaded.</param>
        /// <exception cref="NullReferenceException">Thrown if the Room Loader is null.</exception>
        protected void UnLoadRoom(Room room)
        {
            if (_roomLoader == null)
                throw new NullReferenceException("Room Loader is null");

            _roomLoader.UnLoadRoom(room);
        }

        #endregion

        #region Room Methods

        /// <summary>
        /// Performs room creation with the room information sent.
        /// If the client's connection information is null, it creates the room as belonging to the server.
        /// If the connection is not null, it creates it as belonging to the client.
        /// </summary>
        /// <param name="roomInfo">The information about the room to be created.</param>
        /// <param name="conn">The optional network connection to use when creating the room.</param>
        internal abstract void CreateRoom(RoomInfo roomInfo, NetworkConnection conn = null);

        /// <summary>
        /// Joins the client into the specified room
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="room"></param>
        internal abstract void JoinRoom(NetworkConnection conn, Room room);
        
        /// <summary>
        /// Joins the client into the room with the specified room' name
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="roomName"></param>
        internal abstract void JoinRoom(NetworkConnection conn, string roomName);

        /// <summary>
        /// Joins the client into the room with the specified room' ID
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="roomID"></param>
        internal abstract void JoinRoom(NetworkConnection conn, uint roomID);

        /// <summary>
        /// It works on the server side. Deletes all rooms and removes all customers from the rooms.
        /// </summary>
        internal abstract void RemoveAllRoom(bool forced = false);

        /// <summary>
        /// It works on the server side. It deletes the specified Room and removes all customers from the room.
        /// </summary>
        /// <param name="roomName"></param>
        /// <param name="forced"></param>
        internal abstract void RemoveRoom(string roomName, bool forced = false);
        
        /// <summary>
        /// It works on the server side. It deletes the specified Room and removes all customers from the room.
        /// </summary>
        /// <param name="room"></param>
        /// <param name="forced"></param>
        internal abstract void RemoveRoom(Room room, bool forced = false);

        /// <summary>
        /// It works on the server side. It performs the process of a client exiting from the server.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="isDisconnected"></param>
        internal abstract void ExitRoom(NetworkConnection conn, bool isDisconnected);

        #endregion
    }
}
using System;
using Mirror;
using System.Linq;
using System.Collections.Generic;

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
        /// The function return information about the room where the "connection ID" is located
        /// </summary>
        /// <remarks>Only works on client</remarks>
        /// <returns>Information about the room where the "connection ID" is located.</returns>
        public RoomInfo GetRoomOfClient()
        {
            return _roomListInfos.FirstOrDefault(room => room.ConnectionIds.Any(id => id == RoomClient.ID));
        }

        /// <summary>
        /// The function return information about where the scene* is located
        /// </summary>
        /// <remarks>Only works on server</remarks>
        /// <param name="scene"></param>
        /// <returns>Information about the room where the scene* is located</returns>
        public Room GetRoomOfScene(UnityEngine.SceneManagement.Scene scene)
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
        public static void RequestCreateRoom(RoomInfo roomInfo)
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
        public static void RequestJoinRoom(string roomName)
        {
            if (NetworkClient.connection == null) return;

            var roomInfo = new RoomInfo { Name = roomName };

            var serverRoomMessage = new ServerRoomMessage(ServerRoomState.Join, roomInfo);

            NetworkClient.Send(serverRoomMessage);
        }

        /// <summary>
        /// Sends a request to the server to exit the client's room
        /// </summary>
        /// <remarks>Only works on client</remarks>
        /// <param name="isDisconnected"></param>
        public static void RequestExitRoom(bool isDisconnected = false)
        {
            if (NetworkClient.connection == null) return;

            var serverRoomMessage =
                new ServerRoomMessage(ServerRoomState.Exit, default, isDisconnected);

            NetworkClient.Send(serverRoomMessage);
        }

        #endregion

        #region Room Loader Mehods

        protected void LoadRoom(Room room, RoomInfo roomInfo, Action onLoaded = null)
        {
            if (_roomLoader == null)
                throw new NullReferenceException("Room Loader is null");

            _roomLoader.LoadRoom(room, roomInfo, onLoaded);
        }

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
        /// <param name="conn"></param>
        /// <param name="roomInfo"></param>
        public abstract void CreateRoom(NetworkConnection conn = null,
            RoomInfo roomInfo = default);

        /// <summary>
        /// Joins the client into the specified room
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="room"></param>
        public abstract void JoinRoom(NetworkConnection conn, Room room);
        
        /// <summary>
        /// Joins the client into the room with the specified room' name
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="roomName"></param>
        public abstract void JoinRoom(NetworkConnection conn, string roomName);

        /// <summary>
        /// It works on the server side. Deletes all rooms and removes all customers from the rooms.
        /// </summary>
        public abstract void RemoveAllRoom(bool forced = false);

        /// <summary>
        /// It works on the server side. It deletes the specified Room and removes all customers from the room.
        /// </summary>
        /// <param name="roomName"></param>
        /// <param name="forced"></param>
        public abstract void RemoveRoom(string roomName, bool forced = false);

        /// <summary>
        /// It works on the server side. It performs the process of a client exiting from the server.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="isDisconnected"></param>
        public abstract void ExitRoom(NetworkConnection conn, bool isDisconnected);

        #endregion
    }
}
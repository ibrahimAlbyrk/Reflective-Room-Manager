﻿using Mirror;
using System.Linq;
using System.Collections.Generic;

namespace REFLECTIVE.Runtime.NETWORK.Room.Structs
{
    [System.Serializable]
    public class Room
    {
        public bool IsServer;
        
        public string RoomName;

        public UnityEngine.SceneManagement.Scene Scene;
        
        public int MaxPlayers;
        public int CurrentPlayers;

        public List<NetworkConnection> Connections;
        
        public Room(string roomName, int maxPlayers, bool isServer)
        {
            IsServer = isServer;
            RoomName = roomName;
            MaxPlayers = maxPlayers;
            CurrentPlayers = 0;
            Connections = new List<NetworkConnection>();
            
            Scene = default;
        }

        public bool AddConnection(NetworkConnection conn)
        {
            if (Connections.Contains(conn)) return false;

            Connections.Add(conn);

            CurrentPlayers++;
            
            return true;
        }

        public List<NetworkConnection> RemoveAllConnections()
        {
            var connections = Connections.ToList();

            var room = this;
            
            connections.ForEach(connection =>
            {
                room.RemoveConnection(connection);
            });

            return connections;
        }
        
        public bool RemoveConnection(NetworkConnection conn)
        {
            var isRemoved = Connections.Remove(conn);

            if (isRemoved) CurrentPlayers--;

            return isRemoved;
        }
    }
}
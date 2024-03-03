using System;
using Mirror;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.NETWORK.Room
{
    /// <summary>
    /// Represents a room in a networked game session.
    /// </summary>
    [Serializable]
    public class Room
    {
        public uint ID;

        public bool IsPrivate;
        public bool IsServer;
        
        public string Name;

        public Scene Scene;
        
        public int MaxPlayers;
        public int CurrentPlayers;

        public readonly List<NetworkConnection> Connections;

        private Dictionary<string, string> _customData;
        
        /// <summary>
        /// Represents a room in a network game.
        /// </summary>
        /// <param name="name">The name of the room.</param>
        /// <param name="maxPlayers">The maximum number of players allowed in the room.</param>
        /// <param name="isServer">A flag indicating whether this room is a server.</param>
        internal Room(string name, int maxPlayers, bool isServer)
        {
            IsServer = isServer;
            Name = name;
            MaxPlayers = maxPlayers;
            CurrentPlayers = 0;
            Connections = new List<NetworkConnection>();
            
            Scene = default;

            _customData = new Dictionary<string, string>();
        }

        #region Custom Data

        internal Dictionary<string, string> GetCustomData()
        {
            return new Dictionary<string, string>(_customData);
        }
        
        internal void SetCustomData(Dictionary<string, string> customData)
        {
            _customData = new Dictionary<string, string>(customData);
        }

        internal void SetCustomData(params (string, string)[] customData)
        {
            _customData = new Dictionary<string, string>();
            
            foreach (var (key, value) in customData)
            {
                _customData.Add(key, value);
            }
        }

        internal void AddCustomData(string key, string value)
        {
            if (_customData.TryAdd(key, value)) return;
            
            Debug.LogWarning("There is data with the same name for custom data");
        }
        
        internal bool RemoveCustomDataset(string dataName)
        {
            var isRemoved = _customData.Remove(dataName);

            return isRemoved;
        }

        internal void RemoveAllCustomData()
        {
            _customData.Clear();
        }

        #endregion

        #region Connection

        /// <summary>
        /// Adds a network connection to the list of connections.
        /// </summary>
        /// <param name="conn">The network connection to be added.</param>
        /// <returns>True if the connection was successfully added, false if it already exists in the list.</returns>
        internal bool AddConnection(NetworkConnection conn)
        {
            if (Connections.Contains(conn)) return false;

            Connections.Add(conn);

            CurrentPlayers++;
            
            return true;
        }

        /// <summary>
        /// Removes all connections from the current network room.
        /// </summary>
        /// <returns>A list of NetworkConnection objects that were removed.</returns>
        internal List<NetworkConnection> RemoveAllConnections()
        {
            var connections = Connections.ToList();

            var room = this;
            
            connections.ForEach(connection =>
            {
                room.RemoveConnection(connection);
            });

            return connections;
        }

        /// <summary>
        /// Removes a network connection from the list of connections and updates the count of current players.
        /// </summary>
        /// <param name="conn">The network connection to be removed. </param>
        /// <returns>True if the connection is successfully removed, false otherwise.</returns>
        internal bool RemoveConnection(NetworkConnection conn)
        {
            var isRemoved = Connections.Remove(conn);

            if (isRemoved) CurrentPlayers--;

            return isRemoved;
        }

        #endregion
    }
}
using Mirror;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.NETWORK.Room.Structs
{
    using SceneManagement.Manager;
    
    [System.Serializable]
    public class Room
    {
        public bool IsServer;
        
        public string RoomName;

        public Scene Scene;
        
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

        #region Connection

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

        #endregion

        #region Scene

        public void ChangeScene(string sceneName)
        {
            ReflectiveSceneManager.UnLoadScene(Scene);
            
            ReflectiveSceneManager.LoadScene(sceneName, loadedScene =>
            {
                Scene = loadedScene;
            });
            
            Connections.ForEach(conn =>
            {
                conn.Send(new SceneMessage{sceneName = sceneName, sceneOperation = SceneOperation.Normal});
            });
        }

        #endregion
    }
}
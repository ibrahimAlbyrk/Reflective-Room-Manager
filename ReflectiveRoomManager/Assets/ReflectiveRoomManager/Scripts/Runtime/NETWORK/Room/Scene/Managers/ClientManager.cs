using Mirror;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.NETWORK.Room.Scenes
{
    internal class ClientManager : IClientManager
    {
        private readonly INetworkOperationManager _networkOperation;

        public ClientManager(INetworkOperationManager networkOperation)
        {
            _networkOperation = networkOperation;
        }

        public void KeepAllClients(List<NetworkIdentity> garbageObjects, SceneChangeHandler sceneChangeHandler)
        {
            var room = sceneChangeHandler.Room; 

            for (var i = 0; i < room.Connections.Count; i++)
            {
                var conn = room.Connections[i];
                
                if (conn.identity == null) continue;

                var clientObject = conn.identity;

                NetworkServer.RemoveConnection(conn.identity.connectionToClient.connectionId);

                garbageObjects.Add(clientObject);
            }
        }

        public void RemoveAllClients(SceneChangeHandler sceneChangeHandler)
        {
            var room = sceneChangeHandler.Room; 

            for (var i = 0; i < room.Connections.Count; i++)
            {
                var conn = room.Connections[i];
                
                if (conn.identity == null) continue;

                NetworkServer.RemovePlayerForConnection(conn.identity.connectionToClient, true);
            }
        }

        public void ResetClientsTransformForClient(List<NetworkIdentity> identities)
        {
            //if host then return
            if (NetworkServer.active) return;
            
            foreach (var identity in identities)
            {
                _networkOperation.NetworkTransformsReset(identity.gameObject);
            }
        }
        
        public void MoveClientsToScene(List<NetworkIdentity> garbageObjects, Scene loadedScene)
        {
            for (var i = 0; i < garbageObjects.Count; i++)
            {
                var identity = garbageObjects[i];
                
                SceneManager.MoveGameObjectToScene(identity.gameObject, loadedScene);

                NetworkServer.AddConnection(identity.connectionToClient);
                
                _networkOperation.NetworkTransformsReset(identity.gameObject);
            }
        }
    }
}
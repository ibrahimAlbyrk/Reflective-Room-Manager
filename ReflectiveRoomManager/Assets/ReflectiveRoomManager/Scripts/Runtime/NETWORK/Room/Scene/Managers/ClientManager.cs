using Mirror;
using System.Collections.Generic;
using UnityEngine;
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

        public void KeepAllClients(List<NetworkIdentity> identities, SceneChangeHandler sceneChangeHandler)
        {
            var room = sceneChangeHandler.Room; 

            for (var i = 0; i < room.Connections.Count; i++)
            {
                var conn = room.Connections[i];
                
                if (conn.identity == null) continue;

                var clientObject = conn.identity;

                NetworkServer.RemoveConnection(clientObject.connectionToClient.connectionId);
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
                if(identity == null) continue;
                
                _networkOperation.NetworkTransformsReset(identity.gameObject);
            }
        }
        
        public void MoveClientsToScene(List<NetworkIdentity> identities, Scene loadedScene)
        {
            for (var i = 0; i < identities.Count; i++)
            {
                var identity = identities[i];
                
                SceneManager.MoveGameObjectToScene(identity.gameObject, loadedScene);

                NetworkServer.AddConnection(identity.connectionToClient);
                
                _networkOperation.NetworkTransformsReset(identity.gameObject);
            }
        }
    }
}
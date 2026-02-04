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

        public void RemoveAllClients(SceneChangeHandler sceneChangeHandler)
        {
            var connections = sceneChangeHandler.Room.Connections;

            for (var i = connections.Count - 1; i >= 0; i--)
            {
                var conn = connections[i];

                if (conn?.identity == null) continue;

                NetworkServer.RemovePlayerForConnection(conn.identity.connectionToClient, RemovePlayerOptions.Destroy);
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
                
                _networkOperation.NetworkTransformsReset(identity.gameObject);
            }
        }
    }
}
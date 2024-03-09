using Mirror;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.NETWORK.Room.Scenes
{
    internal interface IClientManager
    {
        public void KeepAllClients(List<NetworkIdentity> garbageObjects, SceneChangeHandler sceneChangeHandler);
        public void ResetClientsTransformForClient(List<NetworkIdentity> identities);
        public void RemoveAllClients(SceneChangeHandler sceneChangeHandler);
        public void MoveClientsToScene(List<NetworkIdentity> garbageObjects, Scene loadedScene);
    }
}
using Mirror;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.NETWORK.Room.Scenes
{
    internal interface IClientManager
    {
        /// <summary>
        /// Makes necessary adjustments to hide client objects during scene switching
        /// </summary>
        /// <param name="garbageObjects">The list of NetworkIdentity objects that need to be cleaned up.</param>
        /// <param name="sceneChangeHandler">The scene change handler associated with the room. </param>
        public void KeepAllClients(List<NetworkIdentity> garbageObjects, SceneChangeHandler sceneChangeHandler);

        /// <summary>
        /// Resetting the networkTransforms of each client object to solve
        /// the problem ofthe networkTransform shifting to different positions when the scene changes
        /// </summary>
        /// <param name="identities">The list of NetworkIdentity objects representing the client GameObjects.</param>
        public void ResetClientsTransformForClient(List<NetworkIdentity> identities);

        /// <summary>
        /// Removes all client objects associated with a room during a scene change.
        /// </summary>
        /// <param name="sceneChangeHandler">The scene change handler associated with the room.</param>
        public void RemoveAllClients(SceneChangeHandler sceneChangeHandler);

        /// <summary>
        /// Moves client objects to a specified scene and performs necessary adjustments for scene change.
        /// </summary>
        /// <param name="garbageObjects">The list of NetworkIdentity objects that need to be moved to the new scene.</param>
        /// <param name="loadedScene">The new scene that the client objects will be moved to.</param>
        public void MoveClientsToScene(List<NetworkIdentity> garbageObjects, Scene loadedScene);
    }
}
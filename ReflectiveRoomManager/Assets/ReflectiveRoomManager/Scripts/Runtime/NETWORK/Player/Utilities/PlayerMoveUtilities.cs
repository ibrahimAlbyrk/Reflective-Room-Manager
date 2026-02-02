using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.NETWORK.Player.Utilities
{
    using Physic.Collision.D3;
    using Physic.Collision.D2;
    
    public class PlayerMoveUtilities : MonoBehaviour
    {
        /// <summary>
        /// Moves the client's object to the specified scene.
        /// While this operation is performed,
        /// it updates the physics classes in the object with the new scene information.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="scene"></param>
        public static void PlayerMoveToScene(NetworkConnection conn, Scene scene)
        {
            if (conn == null) { Debug.LogWarning("[PlayerMove] Connection is null"); return; }
            if (conn.identity == null) { Debug.LogWarning("[PlayerMove] Connection has no identity"); return; }
            if (conn.identity.gameObject == null) { Debug.LogWarning("[PlayerMove] Identity has no gameObject"); return; }
            if (scene == default) { Debug.LogWarning("[PlayerMove] Target scene is invalid"); return; }
            
            var playerObj = conn.identity.gameObject;
            
            SceneManager.MoveGameObjectToScene(playerObj, scene);
            
            UpdateCollisions(playerObj, scene);
        }

        /// <summary>
        /// It updates the physics classes in the object with the new scene information.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="scene"></param>
        private static void UpdateCollisions(GameObject obj, Scene scene)
        {
            var colls3D = obj.GetComponentsInChildren<Collision3D>();

            foreach (var coll in colls3D)
            {
                coll.UpdatePhysicScene(scene.GetPhysicsScene());
            }
                    
            var colls2D = obj.GetComponentsInChildren<Collision2D>();

            foreach (var coll in colls2D)
            {
                coll.UpdatePhysicScene(scene.GetPhysicsScene2D());
            }
        }
    }
}
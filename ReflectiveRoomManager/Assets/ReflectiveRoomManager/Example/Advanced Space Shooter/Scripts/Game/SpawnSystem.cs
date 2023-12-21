using Mirror;
using UnityEngine;
using REFLECTIVE.Runtime.Extensions;

namespace Examples.SpaceShooter.Game
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class SpawnSystem : NetworkBehaviour
    {
        [ServerCallback]
        public void SpawnPlayer()
        {
            var gameManager = gameObject.RoomContainer().GetSingleton<GameManager>();
            
            var spawnRange = gameManager.GetData().GameAreaRadius;

            var spawnPosition = SpawnUtilities.GetSpawnPosition(spawnRange);

            var spawnRotation = SpawnUtilities.GetSpawnRotation();
            
            RPC_SpawnPlayer(spawnPosition, spawnRotation);
        }

        [TargetRpc]
        private void RPC_SpawnPlayer(Vector3 pos, Quaternion rot)
        {
            var playerTransform = transform;

            playerTransform.position = pos;
            playerTransform.rotation = rot;
        }
    }
}
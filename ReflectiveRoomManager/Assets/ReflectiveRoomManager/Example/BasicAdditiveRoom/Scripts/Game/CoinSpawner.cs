using Mirror;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using REFLECTIVE.Runtime.Singleton;
using REFLECTIVE.Runtime.NETWORK.Utilities;

namespace Examples.Basic.Game
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class CoinSpawner : RoomSingleton<CoinSpawner>
    {
        [SerializeField] private int _count = 15;
        
        [SerializeField] private Vector3 _area;

        [SerializeField] private GameObject _coinPrefab;
        
        private readonly List<GameObject> _spawnedCoins = new();

        [ServerCallback]
        public void DestroyCoin(GameObject obj)
        {
            var destroyObj = _spawnedCoins.FirstOrDefault(c => c == obj);

            if (destroyObj == null) return;
            
            _spawnedCoins.Remove(destroyObj);
            
            NetworkServer.Destroy(destroyObj);
        }
        
        [ServerCallback]
        private void Start()
        {
            for (var i = 0; i < _count; i++)
            {
                var coin = SpawnCoin();
                _spawnedCoins.Add(coin);
            }
        }

        [ServerCallback]
        private void Update()
        {
            if (_spawnedCoins.Count >= _count) return;
            
            var coin = SpawnCoin();
            _spawnedCoins.Add(coin);
        }

        [ServerCallback]
        private GameObject SpawnCoin()
        {
            var x = Random.Range(-_area.x, _area.x);
            var z = Random.Range(-_area.z, _area.z);

            var scene = gameObject.scene;

            var coin = NetworkSpawnUtilities.SpawnObjectForScene(scene, _coinPrefab, new Vector3(x, 1, z), Quaternion.identity);

            return coin;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            
            Gizmos.DrawWireCube(Vector3.zero, _area * 2);
        }
    }
}
using Mirror;
using UnityEngine;
using REFLECTIVE.Runtime.Extensions;
using REFLECTIVE.Runtime.Physic.Collision.D3;

namespace Examples.SpaceShooter
{
    using Game;
    using Spaceship;
    
    [RequireComponent(typeof(NetworkIdentity))]
    public class Collectible : NetworkBehaviour
    {
        [SerializeField] private CollisionSphere _collision3D;
        
        [SerializeField] private GameObject _visual;
        
        [SerializeField] private GameObject HitEffect;

        private bool _isDetected;

        private Vector3 _dir;
        
        //Destroy Variables
        private float _destroyTimer;
        private bool _isStartDestroy;
        
        //Firework Variables
        private float _fireworkTimer;
        private bool _isStartFireworks;
        private GameObject _firework;
        
        [ServerCallback]
        private void Start()
        {
            _dir = new Vector3(Random.value, Random.value, Random.value);
            
            _collision3D.OnCollisionEnter += OnCollisionEntered;
        }
        
        private void OnCollisionEntered(Collider coll)
        {
            if (_isDetected) return;
            
            if (coll == null) return;

            var ship = coll.GetComponent<SpaceshipController>();
                
            if (ship != null)
            {
                var leaderboardManager = gameObject.RoomContainer().GetSingleton<LeaderboardManager>();
                    
                leaderboardManager.AddScore(ship.Username, 5);
                ObjectDestroyer();
                _isDetected = true;
                RPC_SpawnEffect();
            }
        }

        [ServerCallback]
        private void Update()
        {
            transform.Rotate(_dir);
        }
        
        private void FixedUpdate()
        {
            if (_isStartFireworks)
            {
                if (_fireworkTimer > 1)
                {
                    Destroy(_firework);
            
                    _fireworkTimer = 0;
                    _isStartFireworks = false;
                }
                else
                    _fireworkTimer += Time.fixedDeltaTime;
            }
            
            if (!isServer) return;
            
            if (_isStartDestroy)
            {
                if (_destroyTimer > 1.5f)
                {
                    NetworkServer.Destroy(gameObject);
            
                    _destroyTimer = 0;
                    _isStartFireworks = false;
                }
                else
                    _destroyTimer += Time.fixedDeltaTime;
            }
        }

        private void ObjectDestroyer()
        {
            RPC_CloseVisual();
            _isStartDestroy = true;
        }

        [ClientRpc]
        private void RPC_CloseVisual()
        {
            _visual.SetActive(false);
        }

        [ClientRpc]
        private void RPC_SpawnEffect()
        {
            if (HitEffect == null) return;

            _firework = Instantiate(HitEffect, transform.position, Quaternion.identity);

            if (_firework == null) return;

            _firework.GetComponentInChildren<ParticleSystem>().Play();

            _isStartFireworks = true;
            
            Destroy(_firework, 1f);
        }
    }
}
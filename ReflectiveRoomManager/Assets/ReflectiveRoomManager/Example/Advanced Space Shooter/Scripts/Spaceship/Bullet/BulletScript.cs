using Mirror;
using UnityEngine;
using REFLECTIVE.Runtime.Extensions;
using REFLECTIVE.Runtime.Physic.Collision.D3;

namespace Examples.SpaceShooter.Spaceship
{
    using AI;
    using Game;

    public class BulletScript : NetworkBehaviour
    {
        [SerializeField] private Collision3D _collision3D;

        [Tooltip("The particle effect of hitting something.")]
        public GameObject HitEffect;

        public ParticleSystem Trail;

        private bool _isHit;
        private bool _isMove = true;

        private GameObject _owner;

        private bool _init;
        private bool _isEnemy;

        private float _bulletLifeTime;
        private float _bulletSpeed;
        private float _bulletDamage;
        
        //Destroy Variables
        private const float _delayTime = 1.1f;
        private float _timer;
        private bool _isStartDestroySequence;
        
        //Firework Variables
        private float _fireworkTimer;
        private bool _isStartFireworks;
        private GameObject _firework;

        private bool _takeDamage;

        public void Init(GameObject owner, bool isEnemy = false, float bulletDamage = 1f, float bulletLifeTime = 3f,
            float bulletSpeed = 100f)
        {
            _owner = owner;

            _bulletLifeTime = bulletLifeTime;
            _bulletSpeed = bulletSpeed;
            _bulletDamage = bulletDamage;

            Invoke(nameof(Lifetime), _bulletLifeTime);

            _init = true;

            _isEnemy = isEnemy;
        }

        [ServerCallback]
        private void Start()
        {
            _collision3D.OnCollisionEnter += OnCollisionEntered;
        }

        [ServerCallback]
        private void LateUpdate()
        {
            if (_isStartDestroySequence)
            {
                if (_timer > _delayTime)
                {
                    if(gameObject != null)
                        NetworkServer.Destroy(gameObject);
            
                    _timer = 0;
                    _isStartDestroySequence = false;
                }
                else
                    _timer += Time.fixedDeltaTime;
            }
        }

        private void FixedUpdate()
        {
            if (_isStartFireworks)
            {
                if (_fireworkTimer > 1)
                {
                    if(_firework != null)
                        Destroy(_firework);
            
                    _fireworkTimer = 0;
                    _isStartFireworks = false;
                }
                else
                    _fireworkTimer += Time.fixedDeltaTime;
            }

            if (!_init || !_isMove) return;

            if (!isServer) return;
            
            transform.position += transform.forward * (_bulletSpeed * Time.fixedDeltaTime);
        }

        [ServerCallback]
        private void OnCollisionEntered(Collider coll)
        {
            if (_isHit) return;
            
            if (coll == null) return;
            if (coll.gameObject == _owner) return;
            
            TakeDamageToObstacle(_owner, coll.gameObject);
            
            _isHit = true;
        }
        
        [ServerCallback]
        private void TakeDamageToObstacle(GameObject owner, GameObject obstacle)
        {
            if (_takeDamage) return;

            _takeDamage = true;
            
            if (obstacle == null) return;
            
            if (gameObject == owner) return;

            DestroySequence();

            var username = "";

            if (!_isEnemy)
            {
                username = owner.GetComponent<SpaceshipController>()?.Username;   
            }

            var Damageable = obstacle.GetComponent<Damageable>();

            if (Damageable != null)
            {
                if (Damageable.GetHealth() <= 0)
                {
                    AddScore(username, 30);
                }
                else
                {
                    Damageable.DealDamage(_bulletDamage);
                }

                return;
            }

            var destructionScript = obstacle.GetComponent<DestructionScript>();
            if (destructionScript != null)
            {
                if (destructionScript.HP <= 0) return;

                destructionScript.HP -= _bulletDamage;

                var basicAI = destructionScript.GetComponent<BasicAI>();

                var isDead = destructionScript.HP <= 0;

                if (basicAI != null)
                {
                    basicAI.Threat();

                    if (isDead) AddScore(username, 30);
                }
                else if
                    (isDead) AddScore(username, 1);
            }
        }

        [ServerCallback]
        private void AddScore(string username, int value)
        {
            if (_isEnemy || string.IsNullOrEmpty(username)) return;

            var _leaderboardManager = gameObject.RoomContainer().GetSingleton<LeaderboardManager>();

            _leaderboardManager?.AddScore(username, value);
        }

        [ServerCallback]
        private void DestroySequence()
        {
            _isMove = false;

            RPC_CloseVisual();

            RPC_SpawnFireworks(transform.position, Quaternion.identity);
            
            _isStartDestroySequence = true;
        }

        [ServerCallback]
        private void Lifetime() => NetworkServer.Destroy(gameObject);

        [ClientRpc]
        private void RPC_CloseVisual()
        {
            if (Trail == null) return;

            Trail.gameObject.SetActive(false);
        }

        [ClientRpc]
        private void RPC_SpawnFireworks(Vector3 pos, Quaternion rot)
        {
            if (HitEffect == null) return;

            _firework = Instantiate(HitEffect, pos, rot);

            if (_firework == null) return;

            _firework.GetComponentInChildren<ParticleSystem>().Play();

            _isStartFireworks = true;
            
            Destroy(_firework, 1f);
        }
    }
}
using Mirror;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using REFLECTIVE.Runtime.NETWORK.Utilities;
using REFLECTIVE.Runtime.Physic.Collision.D3;
using Random = UnityEngine.Random;

namespace Examples.SpaceShooter.AI
{
    using Spaceship;
    using Utilities;

    [RequireComponent(typeof(NetworkIdentity))]
    public class BasicAI : NetworkBehaviour
    {
        #region Serialize Var

        [Header("Setup Settings")]
        public CollisionSphere Collision3D;
        public Transform origin;
        public Transform barrel;
        [SerializeField] private DestructionScript _destruction;

        [SerializeField] private float _damage = 10;

        public GameObject bullet;

        [Header("General Settings")] public float timeInThreatenedMode = 20f;
        public float aggresiveSpeed = 2.75f;
        public float aggresiveTurnSpeed = 0.65f;
        public float normalSpeed = 1f;
        public float normalTurnSpeed = 10f;

        [SerializeField] private float _fireCountDown = .5f;

        [Header("Patrol Settings")] public float patrolRange = 1000f;
        public int pointCount = 50;

        public bool aggresive;

        [Header("Detection Settings")]
        [SerializeField] private float _stopRange = 20f;

        [SerializeField] private float _bulletLifeTime = 5f;
        [SerializeField] private float _bulletSpeed = 150f;

        #endregion

        #region Private Vars

        private readonly List<Vector3> _points = new();

        private Transform _targetPlayer;

        private Rigidbody _rigid;

        private int _pointIndex;

        private float _fireTimer;

        private float prevspeed;

        private float prevturn;

        #endregion

        #region Base Methods

        private Vector3 _firstPosition;

        [ServerCallback]
        private void Start()
        {
            for (var i = 0; i < pointCount; i++)
            {
                var point = origin.position + Random.insideUnitSphere * patrolRange;

                _points.Add(point);
            }

            _pointIndex = 0;

            _fireTimer = _fireCountDown;

            _firstPosition = origin.position;

            Collision3D.OnCollisionEnter += OnCollisionEntered;
            Collision3D.OnCollisionExit += OnCollisionExited;
        }

        [ServerCallback]
        private void FixedUpdate()
        {
            if (_destruction.HP <= 0) return;

            if (_targetPlayer != null && _targetPlayer.gameObject.activeSelf) Chase();
            else Go();
        }
        
        #endregion

        #region Threat Methods

        public void Threat()
        {
            if (!aggresive)
                Threatened();
        }

        [Command(requiresAuthority = false)]
        private void Threatened()
        {
            StartCoroutine(Cor_Threatened());
        }

        [ServerCallback]
        private IEnumerator Cor_Threatened()
        {
            prevturn = normalTurnSpeed;
            prevspeed = normalSpeed;
            normalSpeed = aggresiveSpeed;
            normalTurnSpeed = aggresiveTurnSpeed;

            yield return new WaitForSeconds(timeInThreatenedMode);

            normalSpeed = prevspeed;
            normalTurnSpeed = prevturn;
        }
        
        private void OnCollisionEntered(Collider coll)
        {
            if (coll == null || !coll.CompareTag("Spaceship")) return;

            if (coll.GetComponent<Health>().IsDead) return;

            _targetPlayer = coll.transform;
        }

        private void OnCollisionExited(Collider coll)
        {
            if (coll == null || _targetPlayer == null) return;

            if (coll.gameObject != _targetPlayer.gameObject) return;

            _targetPlayer = null;
        }
        
        #endregion

        #region Chase Methods

        private void Chase()
        {
            var isInside = MathUtilities.InDistance(transform.position, _targetPlayer.position, _stopRange);
            
            var dir = transform.forward * aggresiveSpeed;

            if (isInside)
                dir = Vector3.zero;

            transform.Translate(dir, Space.World);
            transform.rotation = Quaternion.Lerp(transform.rotation,
                Quaternion.LookRotation(_targetPlayer.position - transform.position, transform.up),
                Time.deltaTime / aggresiveTurnSpeed);

            if (_fireTimer >= _fireCountDown)
            {
                Fire();
                _fireTimer = 0f;
            }
            else _fireTimer += Time.fixedDeltaTime;
        }

        private void Go()
        {
            var _currentMovePosition = _points[_pointIndex];

            var outDistance = MathUtilities.OutDistance(transform.position, _currentMovePosition, 15);
            
            if (outDistance)
            {
                transform.Translate(transform.forward * normalSpeed, Space.World);
                transform.rotation = Quaternion.Lerp(transform.rotation,
                    Quaternion.LookRotation(_currentMovePosition - transform.position, transform.up),
                    Time.fixedDeltaTime);
                return;
            }

            _pointIndex++;

            if (_pointIndex > _points.Count - 1)
                _pointIndex = 0;
        }

        #endregion

        #region Fire Methods

        private void Fire()
        {
            var scene = gameObject.scene;
            var pos = barrel.position;
            var rot = Quaternion.LookRotation(transform.forward, transform.up);
            
            var bulletObj = NetworkSpawnUtilities.SpawnObjectForScene(scene, bullet, pos, rot);
            
            bulletObj.GetComponent<BulletScript>().Init(gameObject, isEnemy: true, _damage, _bulletLifeTime, _bulletSpeed);
        }

        #endregion

        private void OnDrawGizmosSelected()
        {
            if (origin == null) return;

            if (!Application.isPlaying)
            {
                Gizmos.color = Color.yellow;

                Gizmos.DrawWireSphere(origin.position, _stopRange);
            }
            else
            {
                for (var i = 0; i < pointCount; i++)
                {
                    var pos = _points[i];

                    Gizmos.color = new Color(0.61f, 0.4f, 1f);

                    Gizmos.DrawSphere(pos, 1f);

                    if (i + 1 >= pointCount) break;

                    var nextPos = _points[i + 1];

                    Gizmos.color = Color.blue;

                    Gizmos.DrawLine(pos, nextPos);
                }
            }

            Gizmos.color = Color.green;

            Gizmos.DrawWireSphere(!Application.isPlaying ? origin.position : _firstPosition, patrolRange);
        }
    }
}
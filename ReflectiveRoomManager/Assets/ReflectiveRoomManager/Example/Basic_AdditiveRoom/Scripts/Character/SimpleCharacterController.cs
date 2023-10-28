using Mirror;
using UnityEngine;
using System.Linq;
using System.Collections;
using REFLECTIVE.Runtime.Extensions;

namespace Example.Basic.Character
{
    using Game;
    
    public class SimpleCharacterController : NetworkBehaviour
    {
        [SyncVar] public int ID;
        
        public float speed = 10f;

        public float coinDetectRadius = 1;

        private PhysicsScene _physics;

        private CoinSpawner _coinSpawner;
        private ScoreManager _scoreManager;

        private Transform _camTransform;
        
        [ServerCallback]
        private void Start()
        {
            _physics = gameObject.scene.GetPhysicsScene();
            
            StartCoroutine(DetectCollider());

            _coinSpawner = gameObject.RoomContainer().Get<CoinSpawner>();
            _scoreManager = gameObject.RoomContainer().Get<ScoreManager>();
        }
        
        [ClientCallback]
        private void Update()
        {
            if (!isOwned) return;

            if (_camTransform == null)
                _camTransform = GameObject.FindGameObjectWithTag("PlayerCamera").transform;

            var xMovement = Input.GetAxisRaw("Horizontal");
            var zMovement = Input.GetAxisRaw("Vertical");

            var dir = (_camTransform.forward * zMovement + _camTransform.right * xMovement).normalized;
            dir.y = 0;

            transform.Translate(dir * (speed * Time.deltaTime));
        }

        [ServerCallback]
        private IEnumerator DetectCollider()
        {
            for (;;)
            {
                var colls = new Collider[5];

                var count = _physics.OverlapSphere(transform.position, coinDetectRadius, colls, LayerMask.GetMask("Coin"),
                    QueryTriggerInteraction.Collide);

                if (count < 1) yield return null;

                foreach (var coll in colls.ToList().Where(coll => coll != null))
                {
                    _coinSpawner?.DestroyCoin(coll.gameObject);
                
                    _scoreManager?.AddScore(ID, 1);
                    
                    yield return new WaitForFixedUpdate();
                }
                
                yield return new WaitForEndOfFrame();
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            
            Gizmos.DrawWireSphere(transform.position, coinDetectRadius);
        }
    }
}
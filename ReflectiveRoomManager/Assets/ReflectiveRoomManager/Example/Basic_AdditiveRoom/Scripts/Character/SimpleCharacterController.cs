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

        [ServerCallback]
        private void Start()
        {
            _physics = gameObject.scene.GetPhysicsScene();
            
            StartCoroutine(DetectCollider());

            _coinSpawner = gameObject.Container().Get<CoinSpawner>();
            _scoreManager = gameObject.Container().Get<ScoreManager>();
        }
        
        [ClientCallback]
        private void Update()
        {
            if (!isOwned) return;

            var xMovement = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
            var zMovement = Input.GetAxis("Vertical") * speed * Time.deltaTime;

            transform.Translate(new Vector3(xMovement, 0, zMovement));
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
                    _coinSpawner.DestroyCoin(coll.gameObject);
                
                    _scoreManager.AddScore(ID, 5);
                    
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
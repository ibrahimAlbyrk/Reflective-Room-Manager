using System;
using System.Collections;
using System.Linq;
using Example.Basic.Game;
using Mirror;
using UnityEngine;

namespace Example.Basic.Character
{
    public class SimpleCharacterController : NetworkBehaviour
    {
        [SyncVar] public int ID;
        
        public float speed = 10f;

        public float coinDetectRadius = 1;

        private PhysicsScene _physics;

        private void Start()
        {
            _physics = gameObject.scene.GetPhysicsScene();
            
            StartCoroutine(DetectCollider());
        }
        
        private void Update()
        {
            if (!isOwned) return;

            var xMovement = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
            var zMovement = Input.GetAxis("Vertical") * speed * Time.deltaTime;

            transform.Translate(new Vector3(xMovement, 0, zMovement));
        }

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
                    NetworkServer.Destroy(coll.gameObject);
                
                    ScoreManager.Instance.AddScore(0, 5);
                    
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
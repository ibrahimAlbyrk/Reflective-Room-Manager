using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace REFLECTIVE.Runtime.Physic.Collision
{
    public abstract class CollisionBase<TCollider, TPhysicScene> : MonoBehaviour,
#if UNITY_EDITOR
        IEditableForEditor where TCollider : Component
#endif
    {
        public event Action<TCollider> OnCollisionEnter;
        public event Action<TCollider> OnCollisionStay;
        public event Action<TCollider> OnCollisionExit;
        
        [HideInInspector] public bool Editable;

        [SerializeField] protected LayerMask m_layer = ~0;
        [SerializeField] public int GarbageColliderSize = 3;

        [SerializeField] public Vector3 Center;
        
        protected TPhysicScene m_physicsScene;

        protected TCollider[] m_garbageColliders;
        
        private List<TCollider> _colliders;

#if UNITY_EDITOR
        public void SetEditable(bool state) => Editable = state;
        public void ChangeEditable()
        {
            Editable = !Editable;
        }
#endif

        public void UpdatePhysicScene(TPhysicScene physicsScene)
        {
            m_physicsScene = physicsScene;
        }
        
        public void SetLayer(LayerMask layerMask) => m_layer = layerMask;
        
        protected abstract void CalculateCollision();

        protected abstract void GetPhysicScene();

        private void Awake() => SetCollidersCapacity();

        private void Start() => GetPhysicScene();

        private void FixedUpdate()
        {
            if (m_physicsScene == null)
            {
                GetPhysicScene();
                Debug.LogError("Physics scene not found", gameObject);
                return;
            }

            CalculateCollision();

            HandleNewCollisions(m_garbageColliders);
            HandleContinuedCollisions(m_garbageColliders);
            HandleCollisionsExit(m_garbageColliders);

            HandleColliderCleaner();
        }

        private void SetCollidersCapacity()
        {
            _colliders = new List<TCollider>(GarbageColliderSize);
            m_garbageColliders = new TCollider[GarbageColliderSize];
        }

        private void HandleColliderCleaner()
        {
            //COLLIDER
            for (var i = _colliders.Count - 1; i >= 0; i--)
            {
                if(_colliders[i] != null) continue;
                
                _colliders.RemoveAt(i);
            }
            
            //GARBAGE
            for (var i = 0; i < m_garbageColliders.Length; i++)
            {
                m_garbageColliders[i] = null;
            }
        }

        /// <summary>
        /// If colliding colliders are not in the list,
        /// they add them to the list and trigger the event.
        /// </summary>
        /// <param name="colliders"></param>
        private void HandleNewCollisions(TCollider[] colliders)
        {
            for (var i = colliders.Length - 1; i >= 0; i--)
            {
                var coll = colliders[i];

                if (coll == null) continue;
                
                if (_colliders.Contains(coll)) continue;

                _colliders.Add(coll);
                HandleCollisionEnter(coll);
            }
        }

        /// <summary>
        /// colliding colliders will trigger the event if they are in the list
        /// </summary>
        /// <param name="colliders"></param>
        private void HandleContinuedCollisions(TCollider[] colliders)
        {
            foreach (var coll in _colliders)
            {
                if (coll == null) continue;
                
                if (!colliders.Contains(coll)) continue;

                HandleCollisionStay(coll);
            }
        }

        /// <summary>
        /// If colliders in the list are not included in the colliders,
        /// it removes them from the list and triggers the event.
        /// </summary>s
        /// <param name="colliders"></param>
        private void HandleCollisionsExit(TCollider[] colliders)
        {
            for (var i = _colliders.Count - 1; i >= 0; i--)
            {
                var coll = _colliders[i];
                
                if (coll == null) continue;
                
                if (colliders.Contains(coll)) continue;

                _colliders.RemoveAt(i);
                HandleCollisionExit(coll);
            }
        }

        private void HandleCollisionEnter(TCollider coll) => OnCollisionEnter?.Invoke(coll);

        private void HandleCollisionStay(TCollider coll) => OnCollisionStay?.Invoke(coll);

        private void HandleCollisionExit(TCollider coll) => OnCollisionExit?.Invoke(coll);
    }
}
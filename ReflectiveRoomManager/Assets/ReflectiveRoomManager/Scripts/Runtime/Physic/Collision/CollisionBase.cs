using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace REFLECTIVE.Runtime.Physic.Collision
{
    public interface IEditableForEditor
    {
        public void SetEditable(bool state);
        public void ChangeEditable();
    }
    
    [DisallowMultipleComponent]
    public abstract class CollisionBase<TCollider> : MonoBehaviour, IEditableForEditor where TCollider : Component
    {
        public event Action<TCollider> OnCollisionEnter;
        public event Action<TCollider> OnCollisionStay;
        public event Action<TCollider> OnCollisionExit;
        
        [HideInInspector] public bool Editable;

        [Header("Configuration")]
        [SerializeField] protected LayerMask m_layer = ~0;
        [SerializeField] protected int m_garbageColliderSize = 5;
        
        protected PhysicsScene m_physicsScene;

        private readonly List<TCollider> _colliders = new();

        public void SetEditable(bool state) => Editable = state;
        public void ChangeEditable()
        {
            Editable = !Editable;
        }

        public void SetLayer(LayerMask layerMask) => m_layer = layerMask;
        
        protected abstract TCollider[] CalculateCollision();

        protected abstract void GetPhysicScene();

        protected virtual void DrawGUI(){}

        private void OnDrawGizmosSelected() => DrawGUI();

        private void Start() => GetPhysicScene();

        private void FixedUpdate()
        {
            if (m_physicsScene == default)
            {
                Debug.LogError("Physics scene not found", gameObject);
                return;
            }

            var colliders = CalculateCollision();

            HandleNewCollisions(colliders);
            HandleContinuedCollisions(colliders);
            HandleCollisionsExit(colliders);
        }

        /// <summary>
        /// If colliding colliders are not in the list,
        /// they add them to the list and trigger the event.
        /// </summary>
        /// <param name="colliders"></param>
        private void HandleNewCollisions(TCollider[]colliders)
        {
            foreach (var coll in colliders.ToArray())
            {
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
                if (!colliders.Contains(coll) || coll == null) continue;

                HandleCollisionStay(coll);
            }
        }

        /// <summary>
        /// If colliders in the list are not included in the colliders,
        /// it removes them from the list and triggers the event.
        /// </summary>
        /// <param name="colliders"></param>
        private void HandleCollisionsExit(TCollider[] colliders)
        {
            foreach (var coll in _colliders.ToArray())
            {
                if (colliders.Contains(coll)) continue;
                
                _colliders.Remove(coll);
                HandleCollisionExit(coll);
            }
        }

        private void HandleCollisionEnter(TCollider coll) => OnCollisionEnter?.Invoke(coll);

        private void HandleCollisionStay(TCollider coll) => OnCollisionStay?.Invoke(coll);

        private void HandleCollisionExit(TCollider coll) => OnCollisionExit?.Invoke(coll);
    }
}
using UnityEngine;
using UnityEngine.Serialization;

namespace REFLECTIVE.Runtime.Physic.Collision.D3
{
    public class CollisionBox3D : Collision3D
    {
        [Header("Settings")]
        [SerializeField] private Vector3 _center;
        [SerializeField] private Vector3 _size = new(1, 1, 1);

        public Vector3 Center
        {
            get => _center;
            set => _center = value;
        }
        
        public Vector3 Size
        {
            get => _size;
            set => _size = value;
        }

        protected override Collider[] CalculateCollision()
        {
            var pos = transform.position + _center;

            var colliders = new Collider[m_garbageColliderSize];

            m_physicsScene.OverlapBox(pos, _size, colliders, transform.rotation, m_layer);

            return colliders;
        }
    }
}
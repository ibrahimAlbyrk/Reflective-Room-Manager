using UnityEngine;

namespace REFLECTIVE.Runtime.Physic.Collision.D2
{
    public class CollisionBox2D : Collision2D
    {
        [Header("Settings")]
        [SerializeField] private Vector2 _center;
        [SerializeField] private Vector2 _size = new(1, 1);
        
        public Vector2 Center
        {
            get => _center;
            set => _center = value;
        }
        
        public Vector2 Size
        {
            get => _size;
            set => _size = value;
        }

        protected override Collider2D[] CalculateCollision()
        { 
            var pos = (Vector2)transform.position + _center;

            var colliders = new Collider2D[m_garbageColliderSize];
            
            m_physicsScene.OverlapBox(pos, _size, 0, colliders, m_layer);

            return colliders;
        }
    }
}
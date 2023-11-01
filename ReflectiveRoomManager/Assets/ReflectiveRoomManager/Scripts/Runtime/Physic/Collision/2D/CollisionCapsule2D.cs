using UnityEngine;

namespace REFLECTIVE.Runtime.Physic.Collision.D2
{
    public class CollisionCapsule2D : Collision2D
    {
        [Header("Settings")]
        [SerializeField] public Vector2 _center;
        [SerializeField] public Vector2 _size = new (.5f, 2);

        public Vector2 Center
        {
            get => _center;
            set => _center = value;
        }
        
        public Vector2 Size
        {
            get => _size;
            set
            {
                _size = value;
                
                _size.x = Mathf.Max(_size.x, 0);
                _size.y = Mathf.Max(_size.x, 0);
                
                if (_size.y < _size.x * 2) _size.y = _size.x * 2;
                if (_size.x > _size.y / 2) _size.x = _size.y / 2;
            }
        }

        protected override Collider2D[] CalculateCollision()
        {
            var pos = transform.position + (Vector3)_center;

            var colliders = new Collider2D[m_garbageColliderSize];

            m_physicsScene.OverlapCapsule(pos, _size, CapsuleDirection2D.Vertical, 0, colliders, m_layer);
            
            return colliders;
        }
    }
}
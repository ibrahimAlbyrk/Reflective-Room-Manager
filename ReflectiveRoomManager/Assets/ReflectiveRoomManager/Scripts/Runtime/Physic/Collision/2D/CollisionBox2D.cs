using UnityEngine;

namespace REFLECTIVE.Runtime.Physic.Collision.D2
{
    public class CollisionBox2D : Collision2D
    {
        [Header("Settings")]
        [SerializeField] private Vector2 _size = new(1, 1);
        
        public Vector2 Size
        {
            get => _size;
            set => _size = value;
        }

        protected override Collider2D[] CalculateCollision()
        { 
            var pos = (Vector2)(transform.position + Center);

            var size = Vector2.Scale(transform.localScale, _size);
            
            m_physicsScene.OverlapBox(pos, size, 0, m_garbageColliders, m_layer);

            return m_garbageColliders;
        }
    }
}
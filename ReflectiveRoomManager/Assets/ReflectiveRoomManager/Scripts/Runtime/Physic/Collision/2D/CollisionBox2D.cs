using UnityEngine;

namespace REFLECTIVE.Runtime.Physic.Collision.D2
{
    public class CollisionBox2D : Collision2D
    {
        [Header("Settings")]
        [SerializeField] private Vector2 _offset;
        [SerializeField] private Vector2 _size = new(1, 1);
        
        public Vector2 Offset
        {
            get => _offset;
            set => _offset = value;
        }
        
        public Vector2 Size
        {
            get => _size;
            set => _size = value;
        }

        protected override Collider2D[] CalculateCollision()
        { 
            var pos = (Vector2)transform.position + _offset;

            var colliders = new Collider2D[m_garbageColliderSize];

            m_physicsScene.OverlapBox(pos, _size, 0, colliders, m_layer);

            return colliders;
        }
        
        protected override void DrawGUI()
        {
            Gizmos.color = Color.green;
            Gizmos.matrix = Matrix4x4.TRS(transform.position + (Vector3)_offset, transform.rotation, _size);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            
            if (!Editable) return;
            
            Gizmos.color = new Color(.5f, 1, .5f, .2f);
            Gizmos.DrawCube(Vector3.zero, Vector3.one);
        }
    }
}
using UnityEngine;

namespace REFLECTIVE.Runtime.Physic.Collision.D3
{
    public class CollisionBox3D : Collision3D
    {
        [Header("Settings")]
        [SerializeField] private Vector3 _offset;
        [SerializeField] private Vector3 _size = new(1, 1, 1);

        public Vector3 Offset
        {
            get => _offset;
            set => _offset = value;
        }
        
        public Vector3 Size
        {
            get => _size;
            set => _size = value;
        }

        protected override Collider[] CalculateCollision()
        {
            var pos = transform.position + _offset;

            var colliders = new Collider[m_garbageColliderSize];

            m_physicsScene.OverlapBox(pos, _size, colliders, transform.rotation, m_layer);

            return colliders;
        }

        protected override void DrawGUI()
        {
            Gizmos.color = Color.green;
            Gizmos.matrix = Matrix4x4.TRS(transform.position + _offset, transform.rotation, _size);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            
            if (!Editable) return;
            
            Gizmos.color = new Color(.5f, 1, .5f, .2f);
            Gizmos.DrawCube(Vector3.zero, Vector3.one);
        }
    }
}
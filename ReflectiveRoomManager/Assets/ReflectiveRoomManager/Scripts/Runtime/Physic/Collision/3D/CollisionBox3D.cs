using UnityEngine;

namespace REFLECTIVE.Runtime.Physic.Collision.D3
{
    public class CollisionBox3D : Collision3D
    {
        [Header("Settings")]
        [SerializeField] private Vector3 _size = new(1, 1, 1);

        public Vector3 Size
        {
            get => _size;
            set => _size = value;
        }

        protected override void CalculateCollision()
        {
            var pos = transform.TransformPoint(Center);

            var size = Vector3.Scale(transform.localScale, _size);
            
            m_physicsScene.OverlapBox(pos, size, m_garbageColliders, transform.rotation, m_layer);
        }
    }
}
using UnityEngine;

namespace REFLECTIVE.Runtime.Physic.Collision.D2
{
    public class CollisionBox2D : Collision2D
    {
        [SerializeField] private Vector2 _size = new(1, 1);
        
        public Vector2 Size
        {
            get => _size;
            set => _size = value;
        }

        protected override void CalculateCollision()
        { 
            var size = Vector3.Scale(transform.localScale, _size);
            
            var pos = transform.TransformPoint(Center);

            var angle = transform.rotation.eulerAngles.z;
            
            m_physicsScene.OverlapBox(pos, size, angle, m_garbageColliders, m_layer);
        }
    }
}
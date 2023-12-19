using UnityEngine;

namespace REFLECTIVE.Runtime.Physic.Collision.D2
{
    using Utilities;
    
    public class CollisionCircle : Collision2D
    {
        [SerializeField] private float _radius = .5f;
        
        public float Radius
        {
            get => _radius;
            set => _radius = value;
        }

        protected override void CalculateCollision()
        {
            var pos = transform.TransformPoint(Center);

            var radius = CollisionTransformUtilities.GetRadius2D(transform, _radius);
            
            m_physicsScene.OverlapCircle(pos, radius, m_garbageColliders, m_layer);
        }
    }
}
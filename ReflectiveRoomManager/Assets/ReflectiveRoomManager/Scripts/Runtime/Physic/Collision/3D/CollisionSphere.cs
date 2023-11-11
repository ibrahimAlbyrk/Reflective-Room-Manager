using UnityEngine;

namespace REFLECTIVE.Runtime.Physic.Collision.D3
{
    using Utilities;
    
    public class CollisionSphere : Collision3D
    {
        [Header("Settings")]
        [SerializeField] private float _radius = 1;
        
        public float Radius
        {
            get => _radius;
            set => _radius = value;
        }
        
        protected override Collider[] CalculateCollision()
        {
            var pos = transform.position + Center;

            var radius = CollisionTransformUtilities.GetRadius(transform, _radius);
            
            m_physicsScene.OverlapSphere(pos, radius, m_garbageColliders, m_layer, QueryTriggerInteraction.UseGlobal);

            return m_garbageColliders;
        }
    }
}
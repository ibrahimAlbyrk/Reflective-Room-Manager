using UnityEngine;

namespace REFLECTIVE.Runtime.Physic.Collision.D3
{
    using Utilities;
    
    public class CollisionSphere : Collision3D
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

            var radius = CollisionTransformUtilities.GetRadius(transform, _radius);
            
            m_physicsScene.OverlapSphere(pos, radius, m_garbageColliders, m_layer, QueryTriggerInteraction.UseGlobal);
        }
    }
}
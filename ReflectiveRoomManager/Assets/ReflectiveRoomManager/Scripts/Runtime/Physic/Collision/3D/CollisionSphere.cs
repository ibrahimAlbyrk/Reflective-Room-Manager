using UnityEngine;

namespace REFLECTIVE.Runtime.Physic.Collision.D3
{
    public class CollisionSphere : Collision3D
    {
        [Header("Settings")]
        [SerializeField] private Vector3 _offset;
        [SerializeField] private float _radius = 1;

        protected override Collider[] CalculateCollision()
        {
            var pos = transform.position + _offset;

            var colliders = new Collider[m_garbageColliderSize];

            m_physicsScene.OverlapSphere(pos, _radius, colliders, m_layer, QueryTriggerInteraction.UseGlobal);

            return colliders;
        }
        
        protected override void DrawGUI()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, _radius);
            
            if (!Editable) return;
            
            Gizmos.color = new Color(.5f, 1, .5f, .2f);
            Gizmos.DrawSphere(transform.position, _radius);
        }
    }
}
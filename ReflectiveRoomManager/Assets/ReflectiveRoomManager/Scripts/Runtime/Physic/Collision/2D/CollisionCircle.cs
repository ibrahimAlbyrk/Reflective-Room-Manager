using UnityEngine;

namespace REFLECTIVE.Runtime.Physic.Collision.D2
{
    public class CollisionCircle : Collision2D
    {
        [Header("Settings")]
        [SerializeField] private Vector2 _offset;
        [SerializeField] private float _radius = 1;

        protected override Collider2D[] CalculateCollision()
        {
            var pos = (Vector2)transform.position + _offset;

            var colliders = new Collider2D[m_garbageColliderSize];

            m_physicsScene.OverlapCircle(pos, _radius, colliders, m_layer);

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
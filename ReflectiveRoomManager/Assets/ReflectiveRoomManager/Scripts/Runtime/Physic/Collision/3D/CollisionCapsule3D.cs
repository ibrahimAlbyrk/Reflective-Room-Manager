using UnityEngine;

namespace REFLECTIVE.Runtime.Physic.Collision.D3
{
    public class CollisionCapsule3D : Collision3D
    {
        [Header("Settings")]
        [SerializeField] public Vector3 _center;
        [SerializeField] public float _radius = .5f;
        [SerializeField] public float _height  = 2;

        public Vector3 Center
        {
            get => _center;
            set => _center = value;
        }
        
        public float Radius
        {
            get => _radius;
            set
            {
                _radius = Mathf.Max(value, 0);
                if (_height < _radius * 2) _height = _radius * 2;
            }
        }
        
        public float Height
        {
            get => _height;
            set
            {
                _height = Mathf.Max(value, 0);
                
                if (_radius > _height / 2) _radius = _height / 2;
            }
        }

        protected override Collider[] CalculateCollision()
        {
            var pos = transform.position + _center;

            var colliders = new Collider[m_garbageColliderSize];

            var point0 = transform.TransformDirection(pos + Vector3.up * (Height / 2 - Radius));
            var point1 = transform.TransformDirection(pos + Vector3.down * (Height / 2 - Radius));

            m_physicsScene.OverlapCapsule(point0, point1, Radius, colliders, m_layer);
            
            return colliders;
        }
    }
}
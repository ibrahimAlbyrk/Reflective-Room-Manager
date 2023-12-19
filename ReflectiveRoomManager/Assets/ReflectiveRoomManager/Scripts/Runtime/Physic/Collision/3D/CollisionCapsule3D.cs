using UnityEngine;

namespace REFLECTIVE.Runtime.Physic.Collision.D3
{
    public class CollisionCapsule3D : Collision3D
    {
        [SerializeField] private float _radius = .5f;
        [SerializeField] private float _height  = 2;

        public int DirType { get; set; } = 1;

        public Vector3[] Dirs { get; set; } =
        {
            Vector3.down,
            Vector3.up
        };

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

        protected override void CalculateCollision()
        {
            var scale = transform.localScale;

            scale.x = Mathf.Abs(scale.x);
            scale.y = Mathf.Max(scale.y, 0);
            scale.z = Mathf.Abs(scale.z);

            var height = Height * scale.y;
            var radius = Radius * Mathf.Max(scale.x, scale.z);
            
            var offset = height / 2 - radius;
            
            var point0 = transform.TransformPoint(Center + Dirs[0] * offset);
            var point1 = transform.TransformPoint(Center + Dirs[1] * offset);

            m_physicsScene.OverlapCapsule(point0, point1, radius, m_garbageColliders, m_layer);
        }
    }
}
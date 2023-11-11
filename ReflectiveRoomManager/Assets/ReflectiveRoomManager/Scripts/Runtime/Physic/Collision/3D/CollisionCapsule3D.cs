using UnityEngine;

namespace REFLECTIVE.Runtime.Physic.Collision.D3
{
    public class CollisionCapsule3D : Collision3D
    {
        [Header("Settings")]
        [SerializeField] private float _radius = .5f;
        [SerializeField] private float _height  = 2;

        public int DirType = 1;

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

        protected override Collider[] CalculateCollision()
        {
            var pos = transform.position + Center;

            var scale = transform.localScale;

            scale.x = Mathf.Abs(scale.x);
            scale.y = Mathf.Max(scale.y, 0);
            scale.z = Mathf.Abs(scale.z);

            var height = Height * scale.y;
            var radius = Height * Mathf.Max(scale.x, scale.z);
            
            if (height < radius * 2) height = radius * 2;
            if (radius  > height / 2) radius  = height / 2;

            var dir0 = transform.TransformDirection(Dirs[0]);
            var dir1 = transform.TransformDirection(Dirs[1]);
            
            var point0 = pos + dir0 * (height / 2 - radius);
            var point1 = pos + dir1 * (height / 2 - radius);
            
            Debug.DrawLine(point0, point1, Color.red, .2f);

            m_physicsScene.OverlapCapsule(point0, point1, Radius, m_garbageColliders, m_layer);
            
            return m_garbageColliders;
        }
    }
}
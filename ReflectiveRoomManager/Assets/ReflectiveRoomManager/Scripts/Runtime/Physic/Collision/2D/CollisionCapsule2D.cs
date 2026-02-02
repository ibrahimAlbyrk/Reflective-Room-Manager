using UnityEngine;

namespace REFLECTIVE.Runtime.Physic.Collision.D2
{
    public class CollisionCapsule2D : Collision2D
    {
        [SerializeField] private Vector2 _size = new (.5f, 2);

        public int DirType { get; set; } = 1;

        public CapsuleDirection2D Dir { get; set; } = CapsuleDirection2D.Vertical;

        public float Radius
        {
            get => _size.x;
            set
            {
                _size.x = Mathf.Max(value, 0);
                if (_size.y < _size.x * 2) _size.y = _size.x * 2;
            }
        }

        public float Height
        {
            get => _size.y;
            set
            {
                _size.y = Mathf.Max(value, 0);
                
                if (_size.x  > _size.y / 2) _size.x  = _size.y / 2;
            }
        }

        protected override void CalculateCollision()
        {
            var pos = transform.position + Center;

            var size = new Vector2(_size.x * transform.localScale.x, _size.y * transform.localScale.y);

            size.x = Mathf.Abs(size.x);
            size.y = Mathf.Max(size.y, 0);

            if (size.x  > size.y / 2) size.x  = size.y / 2;
            
            m_physicsScene.OverlapCapsule(pos, size, Dir, 0, m_garbageColliders, m_layer);
        }
    }
}
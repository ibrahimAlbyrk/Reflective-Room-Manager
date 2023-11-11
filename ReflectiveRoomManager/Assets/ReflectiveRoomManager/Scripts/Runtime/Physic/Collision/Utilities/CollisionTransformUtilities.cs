using UnityEngine;

namespace REFLECTIVE.Runtime.Physic.Collision.Utilities
{
    public static class CollisionTransformUtilities
    {
        public static float GetRadius(Transform transform, float radius)
        {
            var scale = transform.localScale;
            
            return radius * Mathf.Max(scale.x, scale.y, scale.z);
        }
        
        public static float GetRadius2D(Transform transform, float radius)
        {
            var scale = transform.localScale;
            
            return radius * Mathf.Max(scale.x, scale.y);
        }
    }
}
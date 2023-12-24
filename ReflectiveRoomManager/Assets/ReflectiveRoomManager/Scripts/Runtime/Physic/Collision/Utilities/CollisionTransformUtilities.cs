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
        
        public static float GetTransformRadius(Transform transform)
        {
            var scale = transform.localScale;
            
            return Mathf.Max(scale.x, scale.y, scale.z);
        }
        
        public static float GetRadius2D(Transform transform, float radius)
        {
            var scale = transform.localScale;
            
            return radius * Mathf.Max(scale.x, scale.y);
        }
        
        public static float GetTransformRadius2D(Transform transform)
        {
            var scale = transform.localScale;
            
            return Mathf.Max(scale.x, scale.y);
        }
    }
}
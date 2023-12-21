using UnityEngine;

namespace Examples.SpaceShooter.Utilities
{
    public static class MathUtilities
    {
        public static bool InDistance(Vector3 pointA, Vector3 pointB, float distance) =>
            (pointA - pointB).sqrMagnitude < distance * distance;
        
        public static bool OutDistance(Vector3 pointA, Vector3 pointB, float distance) =>
            (pointA - pointB).sqrMagnitude > distance * distance;
    }
}
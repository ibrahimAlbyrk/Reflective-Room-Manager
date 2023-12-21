using UnityEngine;

namespace Examples.SpaceShooter
{
    public static class SpawnUtilities
    {
        public static Vector3 GetSpawnPosition(float maxRange) => Random.insideUnitSphere * maxRange;

        public static Quaternion GetSpawnRotation() => Random.rotation;
    }
}
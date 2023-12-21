using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace Examples.SpaceShooter.Utilities
{
    public static class RandomUtilities
    {
        public static T GetRandomElement<T>(this IEnumerable<T> collection)
        {
            var array = collection as T[] ?? collection.ToArray();
            
            var collectionLenght = array.Length;
            
            var randomIndex = Random.Range(0, collectionLenght);

            return array[randomIndex];
        }

        public static float GetRandomValue(this Vector2 vec) => Random.Range(vec.x, vec.y);
        
        public static int GetRandomValue(this Vector2Int vec) => Random.Range(vec.x, vec.y);
    }
}
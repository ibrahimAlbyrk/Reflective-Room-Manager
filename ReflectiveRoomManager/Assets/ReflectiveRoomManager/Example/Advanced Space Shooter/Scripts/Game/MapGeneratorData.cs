using UnityEngine;

namespace Examples.SpaceShooter.Game.Data
{
    [CreateAssetMenu(menuName = "Game/Map Generator Data")]
    public class MapGeneratorData : ScriptableObject
    {
        [Header("General Settings")]
        public float GameAreaRadius = 1000f;

        [Header("AI Settings")]
        public GameObject[] AIPrefabs;
        
        [Space(10)]
        [Header("Fuel Station")]
        [Header("AI Settings")]
        public Vector2 AIFuelStationCountRange = new (1, 2);
        public float AIFuelStationSpawnRange;
        public float AIFuelStationPatrolRange = 100f;
        public float AIFuelStationDetectionRange = 200f;
        
        [Space(10)]
        [Header("Area")]
        [Header("AI Settings")]
        public Vector2 AIAreaCountRange = new (10, 20);
        public float AIAreaPatronRange = 200f;
        public float AIAreaDetectionRange = 300f;
        
        [Header("Environment Settings")]
        public GameObject[] EnvironmentPrefabs;
        public float EnvironmentMinSpawnRange = 1000;
        public float EnvironmentMaxSpawnRange = 3000;
        public int EnvironmentCount = 10;

        [Header("Meteor Settings")]
        public GameObject[] MeteorPrefabs;
        public int MeteorCount = 300;

        [Header("Fuel Station Settings")]
        public GameObject[] FuelStationPrefabs;
        public int FuelStationCount = 10;

        [Header("Feature Settings")]
        public GameObject[] FeaturePrefabs;
        public int FeatureCount = 10;
        
        [Header("Collectible Settings")]
        public GameObject[] CollectiblePrefabs;
        public int CollectibleCount = 50;
    }
}
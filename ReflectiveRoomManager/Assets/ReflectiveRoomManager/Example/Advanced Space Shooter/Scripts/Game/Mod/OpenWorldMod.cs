using Mirror;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using REFLECTIVE.Runtime.NETWORK.Utilities;

namespace Examples.SpaceShooter.Game.Mod
{
    using AI;
    using Utilities;

    [System.Serializable]
    public class OpenWorldMod : GameMod
    {
        [Space(10), Header("Content Settings"), SerializeField]
        private string[] _contentNames;

        [Space(10), Header("Iterator Settings")] [SerializeField]
        private float _iteratorInterval = 10;

        [Header("Debug")] [SerializeField] private float _iteratorTimer;

        private Dictionary<string, Transform> _contents = new();

        private SyncList<Transform> _spawnedCollectibles = new();
        private SyncList<Transform> _spawnedMeteors = new();
        private SyncList<Transform> _spawnedFuelStations = new();
        private SyncList<Transform> _spawnedFeatures = new();
        private SyncDictionary<Transform, Transform> _spawnedFuelStationAIs = new();
        private SyncList<Transform> _spawnedAreaAIs = new();

        public override void StartOnServer()
        {
            _isSpawned = true;
            
            CreateContents();

            SpawnHandler();
        }

        public override void FixedRun()
        {
            if (!_isSpawned) return;
            
            IteratorHandler();
        }

        private void CreateContents()
        {
            foreach (var contentName in _contentNames)
            {
                var content = CreateContent(contentName);

                _contents.Add(contentName, content);
            }
        }

        private void IteratorHandler()
        {
            if (_iteratorTimer >= _iteratorInterval)
            {
                _iteratorTimer = 0f;

                CheckAllSpawnedObjects();
            }
            else _iteratorTimer += Time.fixedDeltaTime;
        }

        #region Spawn Check Methods

        private void CheckAllSpawnedObjects()
        {
            CheckMeteors();
            CheckFuelStations();
            CheckAIs();
            CheckFeatures();
            CheckCollectibles();
        }

        private void CheckMeteors()
        {
            var count = 0;

            foreach (var _spawnedMeteor in _spawnedMeteors.ToList().Where(_spawnedMeteor => _spawnedMeteor == null))
            {
                _spawnedMeteors.Remove(_spawnedMeteor);

                count++;
            }

            SpawnMeteor(count);
        }

        private void CheckFuelStations()
        {
            var count = 0;

            foreach (var _spawnedFuelStation in _spawnedFuelStations.ToList()
                         .Where(_spawnedFuelStation => _spawnedFuelStation == null))
            {
                _spawnedFuelStations.Remove(_spawnedFuelStation);

                count++;
            }

            SpawnFuelStation(count);
        }

        private void CheckAIs()
        {
            CheckFuelStationAI();
            CheckAreaAI();
        }

        private void CheckFuelStationAI()
        {
            foreach (var (ai, fuelStation) in _spawnedFuelStationAIs.ToList())
            {
                if (ai != null) continue;

                SpawnAIsToFuelStation();

                _spawnedFuelStationAIs[fuelStation] = ai;
            }
        }

        private void CheckAreaAI()
        {
            var count = 0;

            foreach (var _spawnedAreaAI in _spawnedAreaAIs.ToList().Where(_spawnedAreaAI => _spawnedAreaAI == null))
            {
                _spawnedFuelStations.Remove(_spawnedAreaAI);

                count++;
            }

            SpawnAIToArea(count);
        }

        private void CheckFeatures()
        {
            var count = 0;

            foreach (var _spawnedFeature in _spawnedFeatures.ToList().Where(_spawnedFeature => _spawnedFeature == null))
            {
                _spawnedFeatures.Remove(_spawnedFeature);

                count++;
            }

            SpawnFeature(count);
        }
        
        private void CheckCollectibles()
        {
            var count = 0;

            foreach (var _spawnedCollectible in _spawnedCollectibles.ToList().Where(_spawnedCollectible => _spawnedCollectible == null))
            {
                _spawnedCollectibles.Remove(_spawnedCollectible);

                count++;
            }

            SpawnCollectible(count);
        }

        #endregion

        #region Spawn Methods

        private void SpawnHandler()
        {
            SpawnEnvironments();

            SpawnMeteors();

            SpawnFuelStations();

            SpawnAIs();

            SpawnFeatures();

            SpawnCollectibles();
        }

        #region Props

        private void SpawnEnvironments()
        {
            SpawnWithConfigurations(
                    _mapGeneratorData.EnvironmentPrefabs,
                    _contents["Environment"],
                    new Vector2(_mapGeneratorData.EnvironmentMinSpawnRange, _mapGeneratorData.EnvironmentMaxSpawnRange),
                    _mapGeneratorData.EnvironmentCount)
                .ToSyncList();
        }

        private void SpawnMeteors()
        {
            _spawnedMeteors = SpawnWithConfigurations(
                    _mapGeneratorData.MeteorPrefabs,
                    _contents["Meteor"],
                    _mapGeneratorData.GameAreaRadius,
                    _mapGeneratorData.MeteorCount)
                .ToSyncList();
        }

        private void SpawnMeteor(int count)
        {
            var spawned = SpawnWithConfigurations(
                _mapGeneratorData.MeteorPrefabs,
                _contents["Meteor"],
                _mapGeneratorData.GameAreaRadius,
                count);

            foreach (var t in spawned)
            {
                _spawnedMeteors.Add(t);
            }
        }

        private void SpawnFuelStations()
        {
            _spawnedFuelStations = SpawnWithConfigurations(
                    _mapGeneratorData.FuelStationPrefabs,
                    _contents["FuelStation"],
                    _mapGeneratorData.GameAreaRadius,
                    _mapGeneratorData.FuelStationCount)
                .ToSyncList();
        }

        private void SpawnFuelStation(int count)
        {
            var spawned = SpawnWithConfigurations(
                _mapGeneratorData.FuelStationPrefabs,
                _contents["FuelStation"],
                _mapGeneratorData.GameAreaRadius,
                count);

            foreach (var t in spawned)
            {
                _spawnedFuelStations.Add(t);
            }
        }

        private void SpawnFeatures()
        {
            _spawnedFeatures = SpawnWithConfigurations(
                    _mapGeneratorData.FeaturePrefabs,
                    _contents["Feature"],
                    _mapGeneratorData.GameAreaRadius,
                    _mapGeneratorData.FeatureCount)
                .ToSyncList();
        }

        private void SpawnCollectibles()
        {
            _spawnedCollectibles = SpawnWithConfigurations(
                    _mapGeneratorData.CollectiblePrefabs,
                    _contents["Collectible"],
                    _mapGeneratorData.GameAreaRadius,
                    _mapGeneratorData.CollectibleCount)
                .ToSyncList();
        }

        private void SpawnFeature(int count)
        {
            var spawned = SpawnWithConfigurations(
                _mapGeneratorData.FeaturePrefabs,
                _contents["Feature"],
                _mapGeneratorData.GameAreaRadius,
                count);

            foreach (var t in spawned)
            {
                _spawnedFeatures.Add(t);
            }
        }
        
        private void SpawnCollectible(int count)
        {
            var spawned = SpawnWithConfigurations(
                _mapGeneratorData.CollectiblePrefabs,
                _contents["Collectible"],
                _mapGeneratorData.GameAreaRadius,
                count);

            foreach (var t in spawned)
            {
                _spawnedCollectibles.Add(t);
            }
        }

        #endregion

        #region AI

        private void SpawnAIs()
        {
            SpawnAIsToFuelStation();

            SpawnAIsToArea();
        }

        private void SpawnAIsToFuelStation()
        {
            foreach (var fuelStation in _spawnedFuelStations)
            {
                var fuelStationCount = Random.Range(
                    _mapGeneratorData.AIFuelStationCountRange.x,
                    _mapGeneratorData.AIFuelStationCountRange.y);

                var prefabs = _mapGeneratorData.AIPrefabs;

                for (var i = 0; i < fuelStationCount; i++)
                {
                    var pos = fuelStation.position +
                              Random.insideUnitSphere * _mapGeneratorData.AIFuelStationSpawnRange;

                    var selectedAI = prefabs.GetRandomElement();

                    var spawnedAI = NetworkSpawnUtilities.SpawnObject(selectedAI, pos, Quaternion.identity, _contents["AI"]);

                    CMD_SetSpawnedAIConfiguration(spawnedAI, _mapGeneratorData.AIFuelStationPatrolRange,
                        _mapGeneratorData.AIFuelStationDetectionRange);

                    _spawnedFuelStationAIs.Add(fuelStation, spawnedAI.transform);
                }
            }
        }

        private Transform SpawnAIToFuelStation(Vector3 stationPos)
        {
            var prefabs = _mapGeneratorData.AIPrefabs;

            var pos = stationPos +
                      Random.insideUnitSphere * _mapGeneratorData.AIFuelStationSpawnRange;

            var selectedAI = prefabs.GetRandomElement();

            var spawnedAI = NetworkSpawnUtilities.SpawnObject(selectedAI, pos, Quaternion.identity, _contents["AI"]);

            CMD_SetSpawnedAIConfiguration(spawnedAI, _mapGeneratorData.AIFuelStationPatrolRange,
                _mapGeneratorData.AIFuelStationDetectionRange);

            return spawnedAI.transform;
        }

        private void SpawnAIsToArea()
        {
            var areaCount = Random.Range(
                _mapGeneratorData.AIAreaCountRange.x,
                _mapGeneratorData.AIAreaCountRange.y);

            var spawnedAIs = SpawnWithConfigurations(_mapGeneratorData.AIPrefabs, _contents["AI"],
                _mapGeneratorData.GameAreaRadius, areaCount);

            foreach (var spawnedAI in spawnedAIs)
            {
                CMD_SetSpawnedAIConfiguration(spawnedAI.gameObject, _mapGeneratorData.AIAreaPatronRange,
                    _mapGeneratorData.AIAreaDetectionRange);

                _spawnedAreaAIs.Add(spawnedAI);
            }
        }

        private void SpawnAIToArea(int count)
        {
            var spawnedAIs = SpawnWithConfigurations(_mapGeneratorData.AIPrefabs, _contents["AI"],
                _mapGeneratorData.GameAreaRadius, count);

            foreach (var spawnedAI in spawnedAIs)
            {
                CMD_SetSpawnedAIConfiguration(spawnedAI.gameObject, _mapGeneratorData.AIAreaPatronRange,
                    _mapGeneratorData.AIAreaDetectionRange);

                _spawnedAreaAIs.Add(spawnedAI);
            }
        }

        private void CMD_SetSpawnedAIConfiguration(GameObject ai, float patronRange, float detectionRange) =>
            RPC_SetSpawnedAIConfiguration(ai, patronRange, detectionRange);

        private void RPC_SetSpawnedAIConfiguration(GameObject ai, float patronRange, float detectionRange)
        {
            var aiScript = ai.GetComponent<BasicAI>();

            aiScript.patrolRange = patronRange;
            aiScript.Collision3D.Radius = detectionRange;
        }

        #endregion

        #endregion

        #region Utilities

        private Transform CreateContent(string contentName, Transform parent = default)
        {
            var currentContent = _manager.transform.Find($"{contentName}Content");

            if (currentContent != null)
                return currentContent;

            var content = new GameObject($"{contentName}Content").transform;
            content.SetParent(parent ? parent : _manager.transform);

            return content;
        }

        private Vector3 GetRandomPointInGameArea()
        {
            return Random.insideUnitSphere * _mapGeneratorData.GameAreaRadius;
        }

        private List<Transform> SpawnWithConfigurations(IReadOnlyList<GameObject> objs, Transform content, float radius,
            float count)
        {
            return SpawnWithConfigurations(objs, content, new Vector2(radius, radius), count);
        }

        private List<Transform> SpawnWithConfigurations(IReadOnlyList<GameObject> objs, Transform content,
            Vector2 radiusRange,
            float count)
        {
            var list = new List<Transform>();

            for (var i = 0; i < count; i++)
            {
                var meteorPrefab = objs.GetRandomElement();

                var pos = Random.insideUnitSphere * Random.Range(radiusRange.x, radiusRange.y);
                var rot = Random.rotation;
                
                var spawnedObject = NetworkSpawnUtilities.SpawnObject(meteorPrefab, pos, rot, content);

                list.Add(spawnedObject.transform);
            }

            return list;
        }

        #endregion
    }
}
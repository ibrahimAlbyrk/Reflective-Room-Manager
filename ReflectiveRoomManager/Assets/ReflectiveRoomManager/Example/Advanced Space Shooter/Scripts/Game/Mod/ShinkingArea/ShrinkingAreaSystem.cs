using Mirror;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using REFLECTIVE.Runtime.Singleton;
using REFLECTIVE.Runtime.Extensions;
using REFLECTIVE.Runtime.NETWORK.Room;
using REFLECTIVE.Runtime.NETWORK.Utilities;

namespace Examples.SpaceShooter.Game.Mod.ShrinkingArea
{
    using Data;
    using Spaceship;
    using Utilities;
    using PostProcess;
    
    public class ShrinkingAreaSystem : RoomSingleton<ShrinkingAreaSystem>
    {
        #region Serialize Variables
        
        private MapGeneratorData _data;

        [SerializeField] private ShrinkingArea_UI _shrinkingAreaUI;

        [SerializeField] private ShrinkingData[] _states;

        [SerializeField] private GameObject _zonePrefab;

        [Header("Outside Settings")] [SerializeField]
        private float _outSideDetectTime = 2;

        #endregion

        #region Private Variables

        private readonly Dictionary<SpaceshipController, float> _outsideShips = new();

        private float _areaRange;

        private int _stateIndex;

        private Coroutine _shrinkingCor;

        private GameObject _zoneObj;

        private bool _isInit;

        private Room _currentRoom;

        #endregion

        #region Base Methods
        
        [ServerCallback]
        private void Start()
        {
            _currentRoom = RoomManagerBase.Instance.GetRoomOfScene(gameObject.scene);
            
            var gameManager = gameObject.RoomContainer().GetSingleton<GameManager>();
            
            _data = gameManager.GetData();
            
            _areaRange = _data.GameAreaRadius;

            var data = _states[_stateIndex];

            StartCoroutine(OnShrinking(data));
            
            SpawnZone();

            _isInit = true;
        }

        [ServerCallback]
        private void FixedUpdate()
        {
            if (!_isInit) return;
            
            var currentState = _states[_stateIndex];

            CheckShipDistance();
            
            foreach (var (outsideShip, time) in _outsideShips.ToDictionary(pair => pair.Key, pair => pair.Value))
            {
                if(time > Time.time) continue;
                
                _outsideShips[outsideShip] = Time.time + _outSideDetectTime;
                outsideShip.Damageable.DealDamage(currentState.Damage);
            }
            
            UpdateZone();
        }

        #endregion
        
        [ClientRpc]
        private void RPC_ShrinkingStartedHandler(float time)
        {
            _shrinkingAreaUI.StartShrinkingHandler(time);
        }
        
        #region Zone Methods

        [Server]
        private void SpawnZone()
        {
            _zoneObj = NetworkSpawnUtilities.SpawnObjectForScene(gameObject.scene, _zonePrefab);
            
            UpdateZone();
        }

        [Server]
        private void UpdateZone()
        {
            _zoneObj.transform.localScale = Vector3.one * _areaRange;
        }

        #endregion
        
        #region Shrinking Area Methods

        private IEnumerator OnShrinking(ShrinkingData data)
        {
            yield return new WaitForSeconds(data.CooldownTime);
            
            RPC_ShrinkingStartedHandler(data.ShrinkingTime);
            
            var rangeDistance = Mathf.Abs(_areaRange - data.Range);

            var startingTime = Time.time;

            var firstAreaRange = _areaRange;
            
            while (rangeDistance > .1f)
            {
                var timePassed = Time.time - startingTime;
                
                _areaRange = Mathf.Lerp(firstAreaRange, data.Range, timePassed / data.ShrinkingTime);

                rangeDistance = Mathf.Abs(_areaRange - data.Range);

                yield return new WaitForFixedUpdate();
            }

            if (_stateIndex >= _states.Length - 1) yield break;

            _stateIndex++;

            var newData = _states[_stateIndex];

            if (_shrinkingCor != null) StopCoroutine(_shrinkingCor);

            _shrinkingCor = StartCoroutine(OnShrinking(newData));
        }

        private void CheckShipDistance()
        {
            var ships = GetPlayersFromRoom();
            
            foreach (var ship in ships)
            {
                if(ship == null) continue;
                
                var outDistance = MathUtilities.OutDistance(Vector3.zero, ship.transform.position, _areaRange);

                if (outDistance)
                {
                    _outsideShips.TryAdd(ship, Time.time + _outSideDetectTime);
                    ship.PostProcessManager.RPC_SetPostProcess(PostProcessMode.ZoneArea);
                }
                else
                {
                    if (_outsideShips.ContainsKey(ship))
                    {
                        _outsideShips.Remove(ship);
                        ship.PostProcessManager.RPC_SetPostProcess(PostProcessMode.Global);
                    }
                }
            }
        }

        #endregion

        #region Utilities

        private IEnumerable<SpaceshipController> GetPlayersFromRoom()
        {
            return _currentRoom.Connections.ToList()
                .Select(conn =>conn?.identity?.gameObject.GetComponent<SpaceshipController>());
        }

        #endregion
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.white;
            
            Gizmos.DrawWireSphere(Vector3.zero, _areaRange);
        }
    }
}
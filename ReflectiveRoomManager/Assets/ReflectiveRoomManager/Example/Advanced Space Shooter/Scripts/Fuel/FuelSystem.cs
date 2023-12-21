using Mirror;
using UnityEngine;
using REFLECTIVE.Runtime.Physic.Collision.D3;

namespace Examples.SpaceShooter.Spaceship
{
    using UI.Fields;
    
    [RequireComponent(typeof(NetworkIdentity))]
    public class FuelSystem : NetworkBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private CollisionSphere _collision3D;

        [Header("Fuel Settings")]
        [SerializeField] private float _maxFuel = 100;
        [SerializeField] private float _fuelDrainAmount = .3f;
        [SerializeField] private float _fuelFastDrainAmount = .6f;
        [SerializeField] private float _fuelAddAmount = 5f;

        [SerializeField] private float _currentFuel;
        
        [SerializeField] private Bar_UI _barUI;
        
        private bool _onStation;

        private SpaceshipController _controller;

        private PhysicsScene _physicsScene;

        [ServerCallback]
        private void Awake()
        {
            _collision3D.OnCollisionEnter += OnColliderEntered;
            _collision3D.OnCollisionExit += OnColliderExited;
        }
        
        [ClientCallback]
        private void Start()
        {
            if (!isOwned) return;

            _controller = GetComponent<SpaceshipController>();
            
            _currentFuel = _maxFuel;
            
            _barUI.SetValue(_currentFuel, _maxFuel);
        }

        [ClientCallback]
        private void Update()
        {
            if (!isOwned) return;

            FuelHandler();
        }

        [TargetRpc]
        private void RPC_SetOnStation(bool state) => _onStation = state;

        #region Collision Methods

        private void OnColliderEntered(Collider coll)
        {
            if (coll == null) return;
            
            RPC_SetOnStation(true);
        }
        
        private void OnColliderExited(Collider coll)
        {
            if (coll == null) return;
            
            RPC_SetOnStation(false);
        }

        #endregion

        #region Fuel Methods

        public void ResetFuel() => _currentFuel = _maxFuel;

        private void FuelHandler()
        {
            if(_onStation) OnRefuel();
            else OnDrainFuel();

            if (_currentFuel <= 0 && _controller.IsRunningMotor)
            {
                _controller.IsRunningMotor = false;
            }
        }
        
        private void OnRefuel()
        {
            var amount = _fuelAddAmount * Time.fixedDeltaTime;

            _currentFuel = Mathf.Min(_currentFuel + amount, _maxFuel);
            
            _barUI.SetValue(_currentFuel, _maxFuel);
            
            if (_currentFuel > 0 && !_controller.IsRunningMotor)
            {
                _controller.IsRunningMotor = true;
            }
        }

        private void OnDrainFuel()
        {
            var isThrottle = _controller.RawInput.w > 0f;

            var drain = isThrottle ? _fuelFastDrainAmount : _fuelDrainAmount;
            
            var amount = drain * Time.fixedDeltaTime;

            _currentFuel = Mathf.Max(_currentFuel - amount, 0f);
            
            _barUI.SetValue(_currentFuel, _maxFuel);
        }

        #endregion
    }
}
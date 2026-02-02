using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.Physic
{
    using NETWORK.Room;
    
    public class PhysicSimulator : MonoBehaviour
    {
        [SerializeField] private bool _isOnlyServer;
        
        private Simulator _simulator;
        private bool _initialized;

        private void FixedUpdate()
        {
            if (_isOnlyServer && NetworkClient.active) return;

            if (!NetworkServer.active) return;

            if (!_initialized)
            {
                var physicsMode = RoomManagerBase.Instance.PhysicsMode;
                _simulator = PhysicSimulatorFactory.Create(gameObject, physicsMode);
                _simulator?.OnAwake();
                _initialized = true;
            }

            _simulator?.FixedUpdate();
        }
    }
}

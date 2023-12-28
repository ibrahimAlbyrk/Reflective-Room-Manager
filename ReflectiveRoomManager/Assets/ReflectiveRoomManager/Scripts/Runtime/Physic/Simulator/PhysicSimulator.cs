using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.Physic
{
    using NETWORK.Room;
    
    public class PhysicSimulator : MonoBehaviour
    {
        [SerializeField] private bool _isOnlyServer;
        
        private Simulator _simulator;
        
        private void Awake()
        {
            if (_isOnlyServer && NetworkClient.active)
            {
                enabled = false;
                return;
            }
            
            if (!NetworkServer.active)
            {
                enabled = false;
                return;
            }

            var physicsMode = RoomManagerBase.Instance.PhysicsMode;

            _simulator = PhysicSimulatorFactory.Create(gameObject, physicsMode);
            
            _simulator?.OnAwake();
        }

        private void FixedUpdate()
        {
            if (_isOnlyServer && NetworkClient.active) return;
            
            if (!NetworkServer.active) return;

            _simulator?.FixedUpdate();
        }
    }
}

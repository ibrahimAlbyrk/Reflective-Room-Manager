using System;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

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

            var physicsMode = RoomManagerBase.Singleton.PhysicsMode; 
            
            _simulator = physicsMode switch
            {
                LocalPhysicsMode.Physics3D => new Simulator3D(gameObject),
                LocalPhysicsMode.Physics2D => new Simulator2D(gameObject),
                LocalPhysicsMode.None => null,
                _ => throw new ArgumentException()
            };
            
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

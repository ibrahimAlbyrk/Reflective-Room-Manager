using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.Physic
{
    public class PhysicSimulator : MonoBehaviour
    {
        private Simulator _simulator;

        public void SetSimulator(Simulator simulator)
        {
            _simulator = simulator;
        }
        
        private void Awake()
        {
            if (!NetworkServer.active)
            {
                enabled = false;
                return;
            }
            
            _simulator?.OnAwake();
        }

        private void FixedUpdate()
        {
            if (!NetworkServer.active) return;

            _simulator?.FixedUpdate();
        }
    }
}

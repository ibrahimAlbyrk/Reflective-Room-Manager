using Mirror;
using UnityEngine;

namespace Examples.SpaceShooter.Spaceship
{
    public class Damageable : NetworkBehaviour
    {
        [SerializeField] private Health _health;
        
        public float GetHealth() => _health.GetHealth();
        
        [ServerCallback]
        public void DealDamage(float dealToDamage)
        {
            var controller = GetComponent<SpaceshipController>();

            if (controller != null)
            {
                controller.RPC_Shake();
            }
            
            _health.Remove(dealToDamage);
        }
    }
}
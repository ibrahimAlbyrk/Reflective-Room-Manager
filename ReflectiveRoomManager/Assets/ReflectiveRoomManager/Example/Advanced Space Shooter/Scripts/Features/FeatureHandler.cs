using Mirror;
using UnityEngine;

namespace Examples.SpaceShooter.Features
{
    public class FeatureHandler : NetworkBehaviour
    {
        [SerializeField] private Feature_SO _feature;

        public Feature_SO GetFeature() => _feature;
        
        public void Destroy() => CMD_Destroy();

        [Command(requiresAuthority = false)]
        private void CMD_Destroy() => NetworkServer.Destroy(gameObject);
    }
}
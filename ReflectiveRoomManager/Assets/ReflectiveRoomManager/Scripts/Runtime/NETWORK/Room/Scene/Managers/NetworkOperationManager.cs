using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.Scenes
{
    public class NetworkOperationManager : INetworkOperationManager
    {
        public void NetworkTransformsReset(GameObject gameObject)
        {
            var networkTransforms = gameObject.GetComponentsInChildren<NetworkTransformBase>(true);

            foreach (var networkTransform in networkTransforms)
            {
                if(networkTransform == null) continue;
                
                networkTransform.Reset();
            }
        }
    }
}
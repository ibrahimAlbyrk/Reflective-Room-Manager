using Mirror;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Examples.SpaceShooter.PostProcess
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class PostProcessManager : NetworkBehaviour
    {
        [SerializeField] private PostProcessVolume _volume;
        
        [SerializeField] private PostProcessProfile GlobalProcess;
        [SerializeField] private PostProcessProfile ZoneArenaProcess;

        [ClientRpc]
        public void RPC_SetPostProcess(PostProcessMode mode)
        {
            _volume.profile = mode switch
            {
                PostProcessMode.Global => GlobalProcess,
                PostProcessMode.ZoneArea => ZoneArenaProcess,
                _ => _volume.profile
            };
        }
    }

    public enum PostProcessMode
    {
        Global,
        ZoneArea
    }
}
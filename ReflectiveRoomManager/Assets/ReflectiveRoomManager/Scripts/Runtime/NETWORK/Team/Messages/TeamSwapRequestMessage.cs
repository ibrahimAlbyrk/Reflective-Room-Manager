using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Team.Messages
{
    /// <summary>
    /// Client-to-Server message requesting to swap to a different team.
    /// </summary>
    public struct TeamSwapRequestMessage : NetworkMessage
    {
        /// <summary>
        /// Target team ID to swap to.
        /// </summary>
        public uint TargetTeamID;

        public TeamSwapRequestMessage(uint targetTeamID)
        {
            TargetTeamID = targetTeamID;
        }
    }
}

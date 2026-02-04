using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Team.Messages
{
    /// <summary>
    /// Server-to-Client message with the result of a team swap request.
    /// </summary>
    public struct TeamSwapResultMessage : NetworkMessage
    {
        /// <summary>
        /// Whether the swap was successful.
        /// </summary>
        public bool Success;

        /// <summary>
        /// Error message if swap failed.
        /// </summary>
        public string ErrorMessage;

        /// <summary>
        /// New team ID (valid only if Success is true).
        /// </summary>
        public uint NewTeamID;

        public TeamSwapResultMessage(bool success, string errorMessage, uint newTeamID)
        {
            Success = success;
            ErrorMessage = errorMessage;
            NewTeamID = newTeamID;
        }

        public static TeamSwapResultMessage CreateSuccess(uint newTeamID)
        {
            return new TeamSwapResultMessage(true, null, newTeamID);
        }

        public static TeamSwapResultMessage CreateFailure(string errorMessage)
        {
            return new TeamSwapResultMessage(false, errorMessage, 0);
        }
    }
}

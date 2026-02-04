using System.Collections.Generic;
using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Team.Formation
{
    /// <summary>
    /// Interface for team formation strategies.
    /// Implement this to create custom team assignment logic.
    /// </summary>
    public interface ITeamFormationStrategy
    {
        /// <summary>
        /// Display name of this strategy.
        /// </summary>
        string StrategyName { get; }

        /// <summary>
        /// Assigns a player to a team based on the strategy's logic.
        /// </summary>
        /// <param name="teams">Available teams to assign to.</param>
        /// <param name="conn">The connection to assign.</param>
        /// <param name="context">Context information for the assignment decision.</param>
        /// <returns>The team the player should be assigned to, or null if assignment failed.</returns>
        Team AssignPlayer(IReadOnlyList<Team> teams, NetworkConnection conn, TeamContext context);
    }
}

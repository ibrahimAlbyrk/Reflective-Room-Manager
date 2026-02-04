namespace REFLECTIVE.Runtime.NETWORK.Team.Integration
{
    using REFLECTIVE.Runtime.NETWORK.Party;

    /// <summary>
    /// Validates party-team compatibility.
    /// Ensures parties can fit within team constraints.
    /// </summary>
    public static class PartyTeamValidator
    {
        /// <summary>
        /// Checks if a party can join a specific team.
        /// </summary>
        /// <param name="party">The party to validate.</param>
        /// <param name="team">The target team.</param>
        /// <returns>True if the party can fit on the team.</returns>
        public static bool CanPartyJoinTeam(Party party, Team team)
        {
            if (party == null || team == null)
                return false;

            return party.MemberCount <= team.AvailableSlots;
        }

        /// <summary>
        /// Validates a party-team assignment with detailed result.
        /// </summary>
        /// <param name="party">The party to validate.</param>
        /// <param name="team">The target team.</param>
        /// <returns>Validation result with details.</returns>
        public static ValidationResult ValidatePartyTeamAssignment(Party party, Team team)
        {
            if (party == null)
            {
                return new ValidationResult(false, "Party is null", ValidationErrorCode.InvalidParty);
            }

            if (team == null)
            {
                return new ValidationResult(false, "Team is null", ValidationErrorCode.InvalidTeam);
            }

            if (party.MemberCount == 0)
            {
                return new ValidationResult(false, "Party has no members", ValidationErrorCode.EmptyParty);
            }

            if (team.IsFull)
            {
                return new ValidationResult(false, "Team is full", ValidationErrorCode.TeamFull);
            }

            if (party.MemberCount > team.AvailableSlots)
            {
                return new ValidationResult(
                    false,
                    $"Party size ({party.MemberCount}) exceeds team capacity ({team.AvailableSlots} slots available)",
                    ValidationErrorCode.PartySizeExceedsCapacity
                );
            }

            if (party.MemberCount > team.MaxSize)
            {
                return new ValidationResult(
                    false,
                    $"Party size ({party.MemberCount}) exceeds team max size ({team.MaxSize})",
                    ValidationErrorCode.PartySizeExceedsMaxTeamSize
                );
            }

            return new ValidationResult(true, null, ValidationErrorCode.None);
        }

        /// <summary>
        /// Checks if a party can fit on any available team.
        /// </summary>
        /// <param name="party">The party to validate.</param>
        /// <param name="teamManager">The team manager with available teams.</param>
        /// <returns>True if at least one team can accommodate the party.</returns>
        public static bool CanPartyJoinAnyTeam(Party party, TeamManager teamManager)
        {
            if (party == null || teamManager == null)
                return false;

            foreach (var team in teamManager.Teams)
            {
                if (CanPartyJoinTeam(party, team))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the first team that can accommodate a party.
        /// </summary>
        /// <param name="party">The party to place.</param>
        /// <param name="teamManager">The team manager with available teams.</param>
        /// <returns>A team that can fit the party, or null if none available.</returns>
        public static Team GetFirstAvailableTeamForParty(Party party, TeamManager teamManager)
        {
            if (party == null || teamManager == null)
                return null;

            foreach (var team in teamManager.Teams)
            {
                if (CanPartyJoinTeam(party, team))
                    return team;
            }

            return null;
        }
    }

    /// <summary>
    /// Result of a party-team validation.
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Whether the validation passed.
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// Error message if validation failed.
        /// </summary>
        public string ErrorMessage { get; }

        /// <summary>
        /// Error code for programmatic handling.
        /// </summary>
        public ValidationErrorCode ErrorCode { get; }

        public ValidationResult(bool isValid, string errorMessage, ValidationErrorCode errorCode)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
        }
    }

    /// <summary>
    /// Error codes for party-team validation failures.
    /// </summary>
    public enum ValidationErrorCode
    {
        None,
        InvalidParty,
        InvalidTeam,
        EmptyParty,
        TeamFull,
        PartySizeExceedsCapacity,
        PartySizeExceedsMaxTeamSize
    }
}

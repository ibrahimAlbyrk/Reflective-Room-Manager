using System;
using Mirror;
using REFLECTIVE.Runtime.NETWORK.Utilities;

namespace REFLECTIVE.Runtime.NETWORK.Party
{
    /// <summary>
    /// Represents a pending party invitation.
    /// Tracks expiration and invitation metadata.
    /// </summary>
    [Serializable]
    public class PartyInvite
    {
        /// <summary>
        /// Connection that sent the invite.
        /// </summary>
        public NetworkConnection Inviter { get; }

        /// <summary>
        /// Connection ID of the inviter for serialization.
        /// </summary>
        public int InviterConnectionId => Inviter.GetConnectionId();

        /// <summary>
        /// Connection that is invited.
        /// </summary>
        public NetworkConnection Target { get; }

        /// <summary>
        /// Connection ID of the target for serialization.
        /// </summary>
        public int TargetConnectionId => Target.GetConnectionId();

        /// <summary>
        /// When the invite was sent.
        /// </summary>
        public DateTime SentAt { get; }

        /// <summary>
        /// When the invite expires.
        /// </summary>
        public DateTime ExpiresAt { get; }

        /// <summary>
        /// Whether the invite has expired.
        /// </summary>
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

        /// <summary>
        /// Time remaining until expiration.
        /// </summary>
        public TimeSpan TimeRemaining => IsExpired ? TimeSpan.Zero : ExpiresAt - DateTime.UtcNow;

        public PartyInvite(NetworkConnection inviter, NetworkConnection target, int timeoutSeconds)
        {
            Inviter = inviter;
            Target = target;
            SentAt = DateTime.UtcNow;
            ExpiresAt = SentAt.AddSeconds(timeoutSeconds);
        }
    }
}

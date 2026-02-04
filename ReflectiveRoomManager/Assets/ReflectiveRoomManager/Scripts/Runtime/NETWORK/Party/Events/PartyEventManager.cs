using System;
using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Party.Events
{
    using Messages;

    /// <summary>
    /// Manages events for the party system.
    /// Provides both server-side and client-side event hooks.
    /// </summary>
    public class PartyEventManager
    {
        #region Server-Side Events

        /// <summary>
        /// Called on server when a party is created.
        /// Parameters: Party
        /// </summary>
        public event Action<Party> OnPartyCreated;

        /// <summary>
        /// Called on server when a party is disbanded.
        /// Parameters: Party
        /// </summary>
        public event Action<Party> OnPartyDisbanded;

        /// <summary>
        /// Called on server when a member joins a party.
        /// Parameters: Party, NetworkConnection (new member)
        /// </summary>
        public event Action<Party, NetworkConnection> OnMemberJoined;

        /// <summary>
        /// Called on server when a member leaves a party.
        /// Parameters: Party, NetworkConnection (leaving member)
        /// </summary>
        public event Action<Party, NetworkConnection> OnMemberLeft;

        /// <summary>
        /// Called on server when a member is kicked from a party.
        /// Parameters: Party, NetworkConnection (kicked member)
        /// </summary>
        public event Action<Party, NetworkConnection> OnMemberKicked;

        /// <summary>
        /// Called on server when party leadership changes.
        /// Parameters: Party, NetworkConnection (old leader), NetworkConnection (new leader)
        /// </summary>
        public event Action<Party, NetworkConnection, NetworkConnection> OnLeaderChanged;

        /// <summary>
        /// Called on server when an invite is sent.
        /// Parameters: Party, NetworkConnection (inviter), NetworkConnection (target)
        /// </summary>
        public event Action<Party, NetworkConnection, NetworkConnection> OnInviteSent;

        /// <summary>
        /// Called on server when an invite is responded to.
        /// Parameters: Party, NetworkConnection (responder), bool (accepted)
        /// </summary>
        public event Action<Party, NetworkConnection, bool> OnInviteResponse;

        #endregion

        #region Client-Side Events

        /// <summary>
        /// Called on client when they successfully create a party.
        /// Parameters: uint (party ID)
        /// </summary>
        public event Action<uint> OnClientPartyCreated;

        /// <summary>
        /// Called on client when they join a party.
        /// Parameters: uint (party ID)
        /// </summary>
        public event Action<uint> OnClientPartyJoined;

        /// <summary>
        /// Called on client when they leave or are removed from a party.
        /// Parameters: uint (party ID)
        /// </summary>
        public event Action<uint> OnClientPartyLeft;

        /// <summary>
        /// Called on client when they are kicked from a party.
        /// Parameters: uint (party ID)
        /// </summary>
        public event Action<uint> OnClientPartyKicked;

        /// <summary>
        /// Called on client when they receive a party invite.
        /// Parameters: uint (party ID), string (inviter name)
        /// </summary>
        public event Action<uint, string> OnClientInviteReceived;

        /// <summary>
        /// Called on client when the party leader changes.
        /// Parameters: uint (party ID), int (new leader connection ID)
        /// </summary>
        public event Action<uint, int> OnClientLeaderChanged;

        /// <summary>
        /// Called on client when party state is synchronized.
        /// Parameters: PartySyncMessage
        /// </summary>
        public event Action<PartySyncMessage> OnClientPartySync;

        /// <summary>
        /// Called on client when a member joins the party.
        /// Parameters: uint (party ID), PartyMemberData (new member)
        /// </summary>
        public event Action<uint, PartyMemberData> OnClientMemberJoined;

        /// <summary>
        /// Called on client when a member leaves the party.
        /// Parameters: uint (party ID), int (connection ID of member who left)
        /// </summary>
        public event Action<uint, int> OnClientMemberLeft;

        #endregion

        #region Server Event Invokers

        internal void Invoke_OnPartyCreated(Party party)
        {
            OnPartyCreated?.Invoke(party);
        }

        internal void Invoke_OnPartyDisbanded(Party party)
        {
            OnPartyDisbanded?.Invoke(party);
        }

        internal void Invoke_OnMemberJoined(Party party, NetworkConnection conn)
        {
            OnMemberJoined?.Invoke(party, conn);
        }

        internal void Invoke_OnMemberLeft(Party party, NetworkConnection conn)
        {
            OnMemberLeft?.Invoke(party, conn);
        }

        internal void Invoke_OnMemberKicked(Party party, NetworkConnection conn)
        {
            OnMemberKicked?.Invoke(party, conn);
        }

        internal void Invoke_OnLeaderChanged(Party party, NetworkConnection oldLeader, NetworkConnection newLeader)
        {
            OnLeaderChanged?.Invoke(party, oldLeader, newLeader);
        }

        internal void Invoke_OnInviteSent(Party party, NetworkConnection inviter, NetworkConnection target)
        {
            OnInviteSent?.Invoke(party, inviter, target);
        }

        internal void Invoke_OnInviteResponse(Party party, NetworkConnection conn, bool accepted)
        {
            OnInviteResponse?.Invoke(party, conn, accepted);
        }

        #endregion

        #region Client Event Invokers

        internal void Invoke_OnClientPartyCreated(uint partyID)
        {
            OnClientPartyCreated?.Invoke(partyID);
        }

        internal void Invoke_OnClientPartyJoined(uint partyID)
        {
            OnClientPartyJoined?.Invoke(partyID);
        }

        internal void Invoke_OnClientPartyLeft(uint partyID)
        {
            OnClientPartyLeft?.Invoke(partyID);
        }

        internal void Invoke_OnClientPartyKicked(uint partyID)
        {
            OnClientPartyKicked?.Invoke(partyID);
        }

        internal void Invoke_OnClientInviteReceived(uint partyID, string inviterName)
        {
            OnClientInviteReceived?.Invoke(partyID, inviterName);
        }

        internal void Invoke_OnClientLeaderChanged(uint partyID, int newLeaderConnectionID)
        {
            OnClientLeaderChanged?.Invoke(partyID, newLeaderConnectionID);
        }

        internal void Invoke_OnClientPartySync(PartySyncMessage msg)
        {
            OnClientPartySync?.Invoke(msg);
        }

        internal void Invoke_OnClientMemberJoined(uint partyID, PartyMemberData member)
        {
            OnClientMemberJoined?.Invoke(partyID, member);
        }

        internal void Invoke_OnClientMemberLeft(uint partyID, int connectionID)
        {
            OnClientMemberLeft?.Invoke(partyID, connectionID);
        }

        #endregion
    }
}

using Mirror;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace REFLECTIVE.Runtime.NETWORK.Room.GUI.Modules
{
    using REFLECTIVE.Runtime.NETWORK.Party;
    using REFLECTIVE.Runtime.NETWORK.Party.Messages;

    public class PartyHUDModule
    {
        public bool HasParty => _partyId != 0;
        public int MemberCount => _members.Count;
        public bool HasInvites => _invites.Count > 0;
        public bool ShowingPanel { get; set; }

        private uint _partyId;
        private string _partyName;
        private int _leaderId;
        private List<PartyMemberData> _members = new();
        private List<(uint partyId, string inviter)> _invites = new();

        private string _nameField = "Party";
        private string _inviteId = "0";
        private Vector2 _scroll;

        public void RegisterEvents()
        {
            var rm = RoomManagerBase.Instance;
            if (rm?.ClientPartyEvents == null) return;
            rm.ClientPartyEvents.OnClientPartySync += OnSync;
            rm.ClientPartyEvents.OnClientInviteReceived += OnInvite;
            rm.ClientPartyEvents.OnClientPartyLeft += OnLeft;
        }

        public void UnregisterEvents()
        {
            var rm = RoomManagerBase.Instance;
            if (rm?.ClientPartyEvents == null) return;
            rm.ClientPartyEvents.OnClientPartySync -= OnSync;
            rm.ClientPartyEvents.OnClientInviteReceived -= OnInvite;
            rm.ClientPartyEvents.OnClientPartyLeft -= OnLeft;
        }

        private void OnSync(PartySyncMessage msg)
        {
            _partyId = msg.PartyID;
            _partyName = msg.PartyName;
            _leaderId = msg.LeaderConnectionID;
            _members = msg.Members?.ToList() ?? new List<PartyMemberData>();
        }

        private void OnInvite(uint partyId, string inviter)
        {
            _invites.Add((partyId, inviter));
        }

        private void OnLeft(uint partyId)
        {
            if (_partyId == partyId)
            {
                _partyId = 0;
                _partyName = null;
                _members.Clear();
            }
        }

        public void DrawPanel(float x, float y)
        {
            GUILayout.BeginArea(new Rect(x, y, 220f, 280f), HUDStyles.BoxStyle);

            GUILayout.BeginHorizontal();
            GUILayout.Label("PARTY", HUDStyles.HeaderStyle);
            if (GUILayout.Button("X", GUILayout.Width(25)))
                ShowingPanel = false;
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            if (_partyId == 0)
            {
                GUILayout.Label("Party Name:");
                _nameField = GUILayout.TextField(_nameField);

                if (GUILayout.Button("Create Party"))
                {
                    NetworkClient.Send(new PartyCreateMessage
                    {
                        PartyName = _nameField,
                        MaxSize = 4,
                        IsPublic = false,
                        AutoAcceptFriends = false,
                        AllowVoiceChat = false
                    });
                }
            }
            else
            {
                GUILayout.Label($"{_partyName} ({_members.Count})");

                _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(100));
                foreach (var m in _members)
                {
                    var leader = m.ConnectionID == _leaderId ? " ★" : "";
                    var ready = m.IsReady ? " ✓" : "";
                    GUILayout.Label($"  {m.PlayerName}{leader}{ready}");
                }
                GUILayout.EndScrollView();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Invite ID:", GUILayout.Width(60));
                _inviteId = GUILayout.TextField(_inviteId, GUILayout.Width(40));
                if (GUILayout.Button("Send") && int.TryParse(_inviteId, out var id))
                {
                    NetworkClient.Send(new PartyInviteMessage
                    {
                        PartyID = _partyId,
                        TargetConnectionID = id
                    });
                }
                GUILayout.EndHorizontal();

                if (GUILayout.Button("Leave Party"))
                {
                    NetworkClient.Send(new PartyLeaveMessage { PartyID = _partyId });
                    _partyId = 0;
                    _partyName = null;
                    _members.Clear();
                }
            }

            GUILayout.EndArea();
        }

        public void DrawInvites(float x, float y)
        {
            for (var i = _invites.Count - 1; i >= 0; i--)
            {
                var inv = _invites[i];

                GUILayout.BeginArea(new Rect(x, y, 200f, 50f), HUDStyles.BoxStyle);
                GUILayout.Label($"Invite: {inv.inviter}");

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Accept"))
                {
                    NetworkClient.Send(new PartyInviteResponseMessage { PartyID = inv.partyId, Accepted = true });
                    _invites.RemoveAt(i);
                }
                if (GUILayout.Button("Decline"))
                {
                    NetworkClient.Send(new PartyInviteResponseMessage { PartyID = inv.partyId, Accepted = false });
                    _invites.RemoveAt(i);
                }
                GUILayout.EndHorizontal();

                GUILayout.EndArea();
                y += 55f;
            }
        }

        public void ClearData()
        {
            _partyId = 0;
            _partyName = null;
            _members.Clear();
        }
    }
}

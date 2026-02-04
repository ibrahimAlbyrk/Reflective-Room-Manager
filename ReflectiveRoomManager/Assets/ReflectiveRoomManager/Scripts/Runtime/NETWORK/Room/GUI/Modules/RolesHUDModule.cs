using Mirror;
using UnityEngine;
using System.Collections.Generic;

namespace REFLECTIVE.Runtime.NETWORK.Room.GUI.Modules
{
    using Structs;
    using Roles;
    using Roles.Messages;
    using Roles.Handlers;

    public class RolesHUDModule : IHUDModule
    {
        public string TabName => "Roles";
        public RoomRole MyRole => _myRole;

        private RoomRole _myRole = RoomRole.Guest;
        private int _myPerms;
        private Dictionary<uint, RoomRoleEntry> _players = new();
        private Vector2 _scroll;
        private string _targetId = "0";
        private int _roleIndex = 1;
        private int _localConnId = -1;

        public void RegisterEvents()
        {
            RoomRoleNetworkHandlers.OnClientRoomRoleChanged += OnChanged;
            RoomRoleNetworkHandlers.OnClientRoomRoleListReceived += OnList;
        }

        public void UnregisterEvents()
        {
            RoomRoleNetworkHandlers.OnClientRoomRoleChanged -= OnChanged;
            RoomRoleNetworkHandlers.OnClientRoomRoleListReceived -= OnList;
        }

        private void OnChanged(uint roomId, uint connId, RoomRole role, int perms)
        {
            _players[connId] = new RoomRoleEntry { ConnectionID = connId, Role = role, CustomPermissions = perms };
            var local = GetLocalConnId();
            if (local >= 0 && local == (int)connId)
            {
                _myRole = role;
                _myPerms = perms;
            }
        }

        private void OnList(uint roomId, RoomRoleEntry[] roles)
        {
            _players.Clear();
            var local = GetLocalConnId();
            foreach (var e in roles)
            {
                _players[e.ConnectionID] = e;
                if (local >= 0 && local == (int)e.ConnectionID)
                {
                    _myRole = e.Role;
                    _myPerms = e.CustomPermissions;
                }
            }
        }

        public void DrawTab(RoomInfo room)
        {
            // My role
            GUILayout.BeginHorizontal();
            GUILayout.Label("Role:", GUILayout.Width(40));
            var old = UnityEngine.GUI.color;
            UnityEngine.GUI.color = GetColor(_myRole);
            GUILayout.Label($"{_myRole}", GUILayout.Width(80));
            UnityEngine.GUI.color = old;
            GUILayout.Label($"({GetPerms(_myRole)})");
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            // Players
            GUILayout.Label("Players:");
            _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(100));

            var localId = GetLocalConnId();
            foreach (var kvp in _players)
            {
                var e = kvp.Value;
                var isMe = localId >= 0 && localId == (int)e.ConnectionID;
                var mark = isMe ? " ★" : "";

                GUILayout.BeginHorizontal();
                old = UnityEngine.GUI.color;
                UnityEngine.GUI.color = GetColor(e.Role);
                GUILayout.Label($"#{e.ConnectionID}{mark}: {e.Role}");
                UnityEngine.GUI.color = old;
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();

            // Assign (Moderator+)
            if (_myRole >= RoomRole.Moderator)
            {
                GUILayout.Space(5);
                GUILayout.Label("Assign Role:", HUDStyles.HeaderStyle);

                GUILayout.BeginHorizontal();
                GUILayout.Label("ID:", GUILayout.Width(25));
                _targetId = GUILayout.TextField(_targetId, GUILayout.Width(35));

                var roles = new[] { "G", "M", "Mod", "Adm", "Own" };
                _roleIndex = GUILayout.SelectionGrid(_roleIndex, roles, 5);
                GUILayout.EndHorizontal();

                if (GUILayout.Button("Assign"))
                {
                    if (uint.TryParse(_targetId, out var tid))
                    {
                        NetworkClient.Send(new RoleAssignmentRequest
                        {
                            RoomID = room.ID,
                            TargetConnectionID = tid,
                            Role = (RoomRole)_roleIndex
                        });
                    }
                }
            }
        }

        public void ClearData()
        {
            _myRole = RoomRole.Guest;
            _myPerms = 0;
            _players.Clear();
            _localConnId = -1;
        }

        private int GetLocalConnId()
        {
            if (_localConnId >= 0) return _localConnId;
            if (NetworkServer.localConnection != null)
            {
                _localConnId = NetworkServer.localConnection.connectionId;
                return _localConnId;
            }
            return -1;
        }

        public static Color GetColor(RoomRole r) => r switch
        {
            RoomRole.Guest => Color.gray,
            RoomRole.Member => Color.white,
            RoomRole.Moderator => Color.cyan,
            RoomRole.Admin => Color.yellow,
            RoomRole.Owner => new Color(1f, 0.6f, 0.2f),
            _ => Color.white
        };

        private static string GetPerms(RoomRole r) => r switch
        {
            RoomRole.Owner => "All",
            RoomRole.Admin => "Mod+Settings",
            RoomRole.Moderator => "Kick/Mute",
            RoomRole.Member => "Chat",
            RoomRole.Guest => "View",
            _ => "?"
        };
    }
}

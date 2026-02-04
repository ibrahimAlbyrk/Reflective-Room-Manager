using Mirror;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace REFLECTIVE.Runtime.NETWORK.Room.GUI
{
    using Service;
    using Structs;
    using State;
    using State.Messages;
    using State.Handlers;
    using Roles;
    using Roles.Messages;
    using Roles.Handlers;
    using REFLECTIVE.Runtime.NETWORK.Party;
    using REFLECTIVE.Runtime.NETWORK.Party.Messages;
    using REFLECTIVE.Runtime.NETWORK.Team;
    using REFLECTIVE.Runtime.NETWORK.Team.Messages;

    [AddComponentMenu("REFLECTIVE/Network Room Manager HUD")]
    public class RoomManagerHUD : MonoBehaviour
    {
        [SerializeField] private float offsetX;
        [SerializeField] private float offsetY;

        // Room fields
        private static string _roomNameField = "Room";
        private static string _maxPlayers = "4";
        private static bool _isServer;
        private static bool _showingRoomList;
        private static Vector2 _roomListScroll;

        // Party fields
        private static string _partyNameField = "Party";
        private static string _inviteTargetId = "0";
        private static bool _showingPartyPanel;
        private static Vector2 _partyScroll;

        // Party client data
        private static uint _currentPartyId;
        private static string _currentPartyName;
        private static int _currentPartyLeaderId;
        private static List<PartyMemberData> _partyMembers = new();
        private static List<PendingInvite> _pendingInvites = new();

        // Team client data
        private static uint _currentTeamId;
        private static string _currentTeamName;
        private static Color _currentTeamColor = Color.white;
        private static TeamData[] _allTeams;
        private static Vector2 _teamScroll;

        // State Machine client data
        private static byte _currentStateId;
        private static float _stateElapsedTime;
        private static Dictionary<string, string> _stateData = new();
        private static bool _isReady;

        // Role System client data
        private static RoomRole _myRole = RoomRole.Guest;
        private static int _myCustomPermissions;
        private static Dictionary<uint, RoomRoleEntry> _playerRoles = new();
        private static Vector2 _roleScroll;
        private static string _roleTargetId = "0";
        private static int _selectedRoleIndex = 1;
        private static int _cachedLocalConnectionId = -1;

        // Tab system for in-room view
        private static int _selectedTab;
        private static readonly string[] TabNames = { "State", "Team", "Roles" };

        // Styles
        private static GUIStyle _boxStyle;
        private static GUIStyle _headerStyle;
        private static GUIStyle _tabStyle;
        private static GUIStyle _tabActiveStyle;
        private static bool _stylesInitialized;

        private struct PendingInvite
        {
            public uint PartyId;
            public string InviterName;
        }

        #region Lifecycle

        protected virtual void Start() => RegisterClientEventHandlers();
        protected virtual void OnDestroy() => UnregisterClientEventHandlers();

        private void RegisterClientEventHandlers()
        {
            var rm = RoomManagerBase.Instance;
            if (rm == null) return;

            if (rm.EnablePartySystem && rm.ClientPartyEvents != null)
            {
                rm.ClientPartyEvents.OnClientPartySync += OnPartySync;
                rm.ClientPartyEvents.OnClientInviteReceived += OnInviteReceived;
                rm.ClientPartyEvents.OnClientPartyLeft += OnPartyLeft;
            }

            if (rm.EnableTeamSystem && rm.ClientTeamEvents != null)
            {
                rm.ClientTeamEvents.OnClientTeamAssigned += OnTeamAssigned;
                rm.ClientTeamEvents.OnClientTeamsUpdated += OnTeamsUpdated;
                rm.ClientTeamEvents.OnClientTeamLeft += OnTeamLeft;
            }

            if (rm.EnableStateMachine)
            {
                RoomStateNetworkHandlers.OnClientRoomStateChanged += OnStateChanged;
                RoomStateNetworkHandlers.OnClientRoomStateSync += OnStateSync;
            }

            if (rm.EnableRoleSystem)
            {
                RoomRoleNetworkHandlers.OnClientRoomRoleChanged += OnRoleChanged;
                RoomRoleNetworkHandlers.OnClientRoomRoleListReceived += OnRoleListReceived;
            }
        }

        private void UnregisterClientEventHandlers()
        {
            var rm = RoomManagerBase.Instance;
            if (rm == null) return;

            if (rm.EnablePartySystem && rm.ClientPartyEvents != null)
            {
                rm.ClientPartyEvents.OnClientPartySync -= OnPartySync;
                rm.ClientPartyEvents.OnClientInviteReceived -= OnInviteReceived;
                rm.ClientPartyEvents.OnClientPartyLeft -= OnPartyLeft;
            }

            if (rm.EnableTeamSystem && rm.ClientTeamEvents != null)
            {
                rm.ClientTeamEvents.OnClientTeamAssigned -= OnTeamAssigned;
                rm.ClientTeamEvents.OnClientTeamsUpdated -= OnTeamsUpdated;
                rm.ClientTeamEvents.OnClientTeamLeft -= OnTeamLeft;
            }

            if (rm.EnableStateMachine)
            {
                RoomStateNetworkHandlers.OnClientRoomStateChanged -= OnStateChanged;
                RoomStateNetworkHandlers.OnClientRoomStateSync -= OnStateSync;
            }

            if (rm.EnableRoleSystem)
            {
                RoomRoleNetworkHandlers.OnClientRoomRoleChanged -= OnRoleChanged;
                RoomRoleNetworkHandlers.OnClientRoomRoleListReceived -= OnRoleListReceived;
            }
        }

        #endregion

        #region Event Handlers

        private void OnPartySync(PartySyncMessage msg)
        {
            _currentPartyId = msg.PartyID;
            _currentPartyName = msg.PartyName;
            _currentPartyLeaderId = msg.LeaderConnectionID;
            _partyMembers = msg.Members?.ToList() ?? new List<PartyMemberData>();
        }

        private void OnInviteReceived(uint partyId, string inviterName)
        {
            _pendingInvites.Add(new PendingInvite { PartyId = partyId, InviterName = inviterName });
        }

        private void OnPartyLeft(uint partyId)
        {
            if (_currentPartyId == partyId)
            {
                _currentPartyId = 0;
                _currentPartyName = null;
                _partyMembers.Clear();
            }
        }

        private void OnTeamAssigned(uint teamId, string teamName)
        {
            _currentTeamId = teamId;
            _currentTeamName = teamName;
        }

        private void OnTeamsUpdated(TeamData[] teams)
        {
            _allTeams = teams;
            if (_currentTeamId != 0 && teams != null)
            {
                var t = teams.FirstOrDefault(x => x.TeamID == _currentTeamId);
                if (t.TeamID != 0) _currentTeamColor = t.TeamColor;
            }
        }

        private void OnTeamLeft(uint teamId)
        {
            if (_currentTeamId == teamId)
            {
                _currentTeamId = 0;
                _currentTeamName = null;
            }
        }

        private void OnStateChanged(uint roomId, RoomStateData data)
        {
            _currentStateId = data.StateTypeID;
            _stateElapsedTime = data.ElapsedTime;
            _stateData = data.Data ?? new Dictionary<string, string>();
            if (_currentStateId != 0) _isReady = false;
        }

        private void OnStateSync(uint roomId, byte stateId, float elapsed, Dictionary<string, string> data)
        {
            _currentStateId = stateId;
            _stateElapsedTime = elapsed;
            if (data != null) _stateData = data;
        }

        private void OnRoleChanged(uint roomId, uint connId, RoomRole role, int perms)
        {
            _playerRoles[connId] = new RoomRoleEntry { ConnectionID = connId, Role = role, CustomPermissions = perms };
            var local = GetLocalConnectionId();
            if (local >= 0 && local == (int)connId)
            {
                _myRole = role;
                _myCustomPermissions = perms;
            }
        }

        private void OnRoleListReceived(uint roomId, RoomRoleEntry[] roles)
        {
            _playerRoles.Clear();
            var local = GetLocalConnectionId();
            foreach (var e in roles)
            {
                _playerRoles[e.ConnectionID] = e;
                if (local >= 0 && local == (int)e.ConnectionID)
                {
                    _myRole = e.Role;
                    _myCustomPermissions = e.CustomPermissions;
                }
            }
        }

        #endregion

        #region Main GUI

        protected virtual void OnGUI()
        {
            if (!NetworkClient.active && !NetworkServer.active) return;

            InitStyles();
            _isServer = !NetworkClient.isConnected && NetworkServer.active;

            var rm = RoomManagerBase.Instance;
            if (!rm) return;

            // Pending party invites (top-left)
            if (!_isServer && rm.EnablePartySystem && _pendingInvites.Count > 0)
                DrawPendingInvites();

            if (!_isServer)
            {
                var room = rm.GetCurrentRoomInfo();
                if (!string.IsNullOrEmpty(room.RoomName))
                {
                    DrawInRoomUI(rm, room);
                    return;
                }
            }

            // Lobby UI
            if (_showingRoomList)
                DrawRoomList(rm);
            else if (_showingPartyPanel && !_isServer)
                DrawPartyPanel();
            else
                DrawLobbyUI(rm);
        }

        private static void InitStyles()
        {
            if (_stylesInitialized) return;

            _boxStyle = new GUIStyle(UnityEngine.GUI.skin.box)
            {
                normal = { background = MakeTex(2, 2, new Color(0.1f, 0.1f, 0.1f, 0.85f)) },
                padding = new RectOffset(8, 8, 8, 8)
            };

            _headerStyle = new GUIStyle(UnityEngine.GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            _tabStyle = new GUIStyle(UnityEngine.GUI.skin.button)
            {
                fixedHeight = 25,
                margin = new RectOffset(2, 2, 2, 2)
            };

            // Active tab - use text color instead of background to avoid visual glitches
            _tabActiveStyle = new GUIStyle(UnityEngine.GUI.skin.button)
            {
                fixedHeight = 25,
                margin = new RectOffset(2, 2, 2, 2),
                fontStyle = FontStyle.Bold
            };
            _tabActiveStyle.normal.textColor = new Color(0.4f, 1f, 0.4f);
            _tabActiveStyle.hover.textColor = new Color(0.5f, 1f, 0.5f);

            _stylesInitialized = true;
        }

        #endregion

        #region Lobby UI

        private void DrawLobbyUI(RoomManagerBase rm)
        {
            var w = 220f;
            var h = _isServer ? 100f : 130f;
            if (!_isServer && rm.EnablePartySystem) h += 25f;

            GUILayout.BeginArea(new Rect(Screen.width - w - 10 + offsetX, 10 + offsetY, w, h), _boxStyle);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Name:", GUILayout.Width(45));
            _roomNameField = GUILayout.TextField(_roomNameField);
            GUILayout.Label("Max:", GUILayout.Width(30));
            _maxPlayers = GUILayout.TextField(_maxPlayers, GUILayout.Width(25));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Create"))
            {
                var info = new RoomInfo
                {
                    RoomName = _roomNameField,
                    SceneName = rm.RoomScene,
                    MaxPlayers = int.TryParse(_maxPlayers, out var mp) ? mp : 4,
                    CustomData = new Dictionary<string, string>()
                };
                if (_isServer) RoomServer.CreateRoom(info);
                else RoomClient.CreateRoom(info);
            }

            if (!_isServer && GUILayout.Button("Join"))
                RoomClient.JoinRoom(_roomNameField);

            if (GUILayout.Button("Rooms"))
                _showingRoomList = true;
            GUILayout.EndHorizontal();

            if (!_isServer && rm.EnablePartySystem)
            {
                var partyLabel = _currentPartyId != 0 ? $"Party ({_partyMembers.Count})" : "Party";
                if (GUILayout.Button(partyLabel))
                    _showingPartyPanel = true;
            }

            GUILayout.EndArea();
        }

        private void DrawRoomList(RoomManagerBase rm)
        {
            var w = 250f;
            var h = 300f;

            GUILayout.BeginArea(new Rect(Screen.width - w - 10 + offsetX, 10 + offsetY, w, h), _boxStyle);

            GUILayout.BeginHorizontal();
            GUILayout.Label("ROOMS", _headerStyle);
            if (GUILayout.Button("X", GUILayout.Width(25)))
                _showingRoomList = false;
            GUILayout.EndHorizontal();

            _roomListScroll = GUILayout.BeginScrollView(_roomListScroll, GUILayout.Height(h - 50));

            if (_isServer)
            {
                foreach (var room in rm.GetRooms())
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"{room.Name} ({room.CurrentPlayers}/{room.MaxPlayers})");
                    if (GUILayout.Button("Del", GUILayout.Width(35)))
                        RoomServer.RemoveRoom(room.Name, forced: true);
                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                foreach (var room in rm.GetRoomInfos())
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"{room.RoomName} ({room.CurrentPlayers}/{room.MaxPlayers})");
                    if (GUILayout.Button("Join", GUILayout.Width(40)))
                        RoomClient.JoinRoom(room.RoomName);
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        #endregion

        #region In-Room UI

        private void DrawInRoomUI(RoomManagerBase rm, RoomInfo room)
        {
            // Top bar - Room info
            DrawRoomInfoBar(room);

            // Count active tabs
            var tabs = new List<string>();
            if (rm.EnableStateMachine) tabs.Add("State");
            if (rm.EnableTeamSystem) tabs.Add("Team");
            if (rm.EnableRoleSystem) tabs.Add("Roles");

            if (tabs.Count == 0) return;

            // Main panel with tabs
            var panelW = 280f;
            var panelH = 320f;

            GUILayout.BeginArea(new Rect(Screen.width - panelW - 10 + offsetX, 55 + offsetY, panelW, panelH), _boxStyle);

            // Tab bar
            GUILayout.BeginHorizontal();
            for (var i = 0; i < tabs.Count; i++)
            {
                var style = _selectedTab == i ? _tabActiveStyle : _tabStyle;
                if (GUILayout.Button(tabs[i], style))
                    _selectedTab = i;
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            // Tab content
            _selectedTab = Mathf.Clamp(_selectedTab, 0, tabs.Count - 1);
            var currentTab = tabs.Count > 0 ? tabs[_selectedTab] : "";

            switch (currentTab)
            {
                case "State":
                    DrawStateTab(rm, room);
                    break;
                case "Team":
                    DrawTeamTab(room);
                    break;
                case "Roles":
                    DrawRolesTab(room);
                    break;
            }

            GUILayout.EndArea();
        }

        private void DrawRoomInfoBar(RoomInfo room)
        {
            var barW = 350f;
            var barH = 35f;

            GUILayout.BeginArea(new Rect(Screen.width - barW - 10 + offsetX, 10 + offsetY, barW, barH), _boxStyle);
            GUILayout.BeginHorizontal();

            GUILayout.Label($"{room.RoomName}", GUILayout.Width(100));
            GUILayout.Label($"{room.CurrentPlayers}/{room.MaxPlayers}", GUILayout.Width(40));

            // Role indicator
            var roleColor = GetRoleColor(_myRole);
            var oldColor = UnityEngine.GUI.color;
            UnityEngine.GUI.color = roleColor;
            GUILayout.Label($"[{_myRole}]", GUILayout.Width(70));
            UnityEngine.GUI.color = oldColor;

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Exit", GUILayout.Width(50)))
            {
                RoomClient.ExitRoom();
                ClearRoomData();
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private static void ClearRoomData()
        {
            _currentTeamId = 0;
            _currentTeamName = null;
            _allTeams = null;
            _currentStateId = 0;
            _stateElapsedTime = 0f;
            _stateData.Clear();
            _isReady = false;
            _myRole = RoomRole.Guest;
            _myCustomPermissions = 0;
            _playerRoles.Clear();
            _cachedLocalConnectionId = -1;
        }

        #endregion

        #region State Tab

        private void DrawStateTab(RoomManagerBase rm, RoomInfo room)
        {
            var stateName = GetStateName(_currentStateId);
            var stateColor = GetStateColor(_currentStateId);

            GUILayout.BeginHorizontal();
            var oldColor = UnityEngine.GUI.color;
            UnityEngine.GUI.color = stateColor;
            GUILayout.Label($"● {stateName}", GUILayout.Width(100));
            UnityEngine.GUI.color = oldColor;
            GUILayout.Label($"Time: {FormatTime(_stateElapsedTime)}");
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            switch (_currentStateId)
            {
                case 0: DrawLobbyState(rm, room); break;
                case 1: DrawStartingState(rm); break;
                case 2: DrawPlayingState(rm); break;
                case 3: DrawPausedState(rm); break;
                case 4: DrawEndedState(rm); break;
            }
        }

        private void DrawLobbyState(RoomManagerBase rm, RoomInfo room)
        {
            _stateData.TryGetValue("ReadyPlayers", out var readyStr);
            int.TryParse(readyStr ?? "0", out var readyCount);

            GUILayout.Label($"Ready: {readyCount}/{room.CurrentPlayers}");

            if (_stateData.TryGetValue("CountdownActive", out var cdStr) && cdStr == "True")
            {
                _stateData.TryGetValue("CountdownRemaining", out var remStr);
                float.TryParse(remStr ?? "0", out var remaining);

                var old = UnityEngine.GUI.color;
                UnityEngine.GUI.color = Color.yellow;
                GUILayout.Label($"Starting in {remaining:F0}s...");
                UnityEngine.GUI.color = old;
            }

            GUILayout.Space(10);

            var btnText = _isReady ? "✓ READY" : "Ready Up";
            var btnColor = _isReady ? Color.green : Color.white;
            var old2 = UnityEngine.GUI.color;
            UnityEngine.GUI.color = btnColor;

            if (GUILayout.Button(btnText, GUILayout.Height(35)))
            {
                SendStateAction(_isReady ? RoomStateAction.UnmarkReady : RoomStateAction.MarkReady);
                _isReady = !_isReady;
            }

            UnityEngine.GUI.color = old2;
        }

        private void DrawStartingState(RoomManagerBase rm)
        {
            var duration = rm?.StateConfig?.StartingCountdownDuration ?? 3f;
            var remaining = Mathf.Max(0f, duration - _stateElapsedTime);

            var bigStyle = new GUIStyle(UnityEngine.GUI.skin.label)
            {
                fontSize = 64,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            bigStyle.normal.textColor = Color.cyan;

            GUILayout.FlexibleSpace();
            GUILayout.Label(Mathf.CeilToInt(remaining).ToString(), bigStyle, GUILayout.Height(80));
            GUILayout.Label("GET READY!", _headerStyle);
            GUILayout.FlexibleSpace();
        }

        private void DrawPlayingState(RoomManagerBase rm)
        {
            var maxDur = rm?.StateConfig?.MaxGameDuration ?? 0f;

            if (maxDur > 0f)
            {
                var remaining = Mathf.Max(0f, maxDur - _stateElapsedTime);
                GUILayout.Label($"Remaining: {FormatTime(remaining)}");
            }

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            if (rm?.StateConfig?.AllowPausing == true)
            {
                if (GUILayout.Button("Pause"))
                    SendStateAction(RoomStateAction.PauseGame);
            }

            if (GUILayout.Button("End Game"))
                SendStateAction(RoomStateAction.EndGame);
            GUILayout.EndHorizontal();
        }

        private void DrawPausedState(RoomManagerBase rm)
        {
            var timeout = rm?.StateConfig?.PauseTimeout ?? 30f;

            var old = UnityEngine.GUI.color;
            UnityEngine.GUI.color = Color.yellow;
            GUILayout.Label("PAUSED", _headerStyle);
            UnityEngine.GUI.color = old;

            if (timeout > 0f)
            {
                var rem = Mathf.Max(0f, timeout - _stateElapsedTime);
                GUILayout.Label($"Auto-resume in {rem:F0}s");
            }

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Resume"))
                SendStateAction(RoomStateAction.ResumeGame);
            if (GUILayout.Button("End"))
                SendStateAction(RoomStateAction.EndGame);
            GUILayout.EndHorizontal();
        }

        private void DrawEndedState(RoomManagerBase rm)
        {
            var duration = rm?.StateConfig?.EndScreenDuration ?? 10f;
            var autoReturn = rm?.StateConfig?.AutoReturnToLobby ?? true;

            var old = UnityEngine.GUI.color;
            UnityEngine.GUI.color = Color.red;
            GUILayout.Label("GAME OVER", _headerStyle);
            UnityEngine.GUI.color = old;

            if (autoReturn && duration > 0f)
            {
                var rem = Mathf.Max(0f, duration - _stateElapsedTime);
                GUILayout.Label($"Returning to lobby in {rem:F0}s");
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Return to Lobby"))
                SendStateAction(RoomStateAction.RestartGame);
        }

        private void SendStateAction(RoomStateAction action)
        {
            var rm = RoomManagerBase.Instance;
            var room = rm?.GetCurrentRoomInfo();
            if (room == null || string.IsNullOrEmpty(room.Value.RoomName)) return;
            NetworkClient.Send(new RoomStateActionMessage(room.Value.ID, action, null));
        }

        #endregion

        #region Team Tab

        private void DrawTeamTab(RoomInfo room)
        {
            // My team
            GUILayout.BeginHorizontal();
            GUILayout.Label("Your Team:", GUILayout.Width(70));
            if (_currentTeamId != 0)
            {
                var old = UnityEngine.GUI.color;
                UnityEngine.GUI.color = _currentTeamColor;
                GUILayout.Label(_currentTeamName ?? "Unknown");
                UnityEngine.GUI.color = old;
            }
            else
            {
                GUILayout.Label("None");
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            if (_allTeams == null || _allTeams.Length == 0)
            {
                GUILayout.Label("No teams available");
                return;
            }

            GUILayout.Label("All Teams:");
            _teamScroll = GUILayout.BeginScrollView(_teamScroll, GUILayout.Height(180));

            foreach (var team in _allTeams)
            {
                var old = UnityEngine.GUI.color;
                UnityEngine.GUI.color = team.TeamColor;

                GUILayout.BeginHorizontal();
                var isMine = team.TeamID == _currentTeamId;
                var mark = isMine ? "★ " : "";
                var memberCount = team.Members?.Length ?? 0;
                GUILayout.Label($"{mark}{team.TeamName} ({memberCount}/{team.MaxSize})", GUILayout.Width(150));

                UnityEngine.GUI.color = old;

                if (!isMine && GUILayout.Button("Join", GUILayout.Width(45)))
                    NetworkClient.Send(new TeamSwapRequestMessage { TargetTeamID = team.TeamID });

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }

        #endregion

        #region Roles Tab

        private void DrawRolesTab(RoomInfo room)
        {
            // My role summary
            GUILayout.BeginHorizontal();
            GUILayout.Label("Role:", GUILayout.Width(40));
            var old = UnityEngine.GUI.color;
            UnityEngine.GUI.color = GetRoleColor(_myRole);
            GUILayout.Label($"{_myRole}", GUILayout.Width(80));
            UnityEngine.GUI.color = old;
            GUILayout.Label($"({GetPermissionSummary(_myRole)})");
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            // Player list
            GUILayout.Label("Players:");
            _roleScroll = GUILayout.BeginScrollView(_roleScroll, GUILayout.Height(100));

            var localId = GetLocalConnectionId();
            foreach (var kvp in _playerRoles)
            {
                var e = kvp.Value;
                var isMe = localId >= 0 && localId == (int)e.ConnectionID;
                var mark = isMe ? " ★" : "";

                GUILayout.BeginHorizontal();
                old = UnityEngine.GUI.color;
                UnityEngine.GUI.color = GetRoleColor(e.Role);
                GUILayout.Label($"#{e.ConnectionID}{mark}: {e.Role}");
                UnityEngine.GUI.color = old;
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();

            // Role assignment (only for Moderator+)
            if (_myRole >= RoomRole.Moderator)
            {
                GUILayout.Space(5);
                GUILayout.Label("Assign Role:", _headerStyle);

                GUILayout.BeginHorizontal();
                GUILayout.Label("ID:", GUILayout.Width(25));
                _roleTargetId = GUILayout.TextField(_roleTargetId, GUILayout.Width(35));

                var roles = new[] { "G", "M", "Mod", "Adm", "Own" };
                _selectedRoleIndex = GUILayout.SelectionGrid(_selectedRoleIndex, roles, 5);
                GUILayout.EndHorizontal();

                if (GUILayout.Button("Assign"))
                {
                    if (uint.TryParse(_roleTargetId, out var targetId))
                    {
                        NetworkClient.Send(new RoleAssignmentRequest
                        {
                            RoomID = room.ID,
                            TargetConnectionID = targetId,
                            Role = (RoomRole)_selectedRoleIndex
                        });
                    }
                }
            }
        }

        #endregion

        #region Party Panel

        private void DrawPartyPanel()
        {
            var w = 220f;
            var h = 280f;

            GUILayout.BeginArea(new Rect(Screen.width - w - 10 + offsetX, 10 + offsetY, w, h), _boxStyle);

            GUILayout.BeginHorizontal();
            GUILayout.Label("PARTY", _headerStyle);
            if (GUILayout.Button("X", GUILayout.Width(25)))
                _showingPartyPanel = false;
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            if (_currentPartyId == 0)
            {
                GUILayout.Label("Party Name:");
                _partyNameField = GUILayout.TextField(_partyNameField);

                if (GUILayout.Button("Create Party"))
                {
                    NetworkClient.Send(new PartyCreateMessage
                    {
                        PartyName = _partyNameField,
                        MaxSize = 4,
                        IsPublic = false,
                        AutoAcceptFriends = false,
                        AllowVoiceChat = false
                    });
                }
            }
            else
            {
                GUILayout.Label($"{_currentPartyName} ({_partyMembers.Count})");

                _partyScroll = GUILayout.BeginScrollView(_partyScroll, GUILayout.Height(100));
                foreach (var m in _partyMembers)
                {
                    var leader = m.ConnectionID == _currentPartyLeaderId ? " ★" : "";
                    var ready = m.IsReady ? " ✓" : "";
                    GUILayout.Label($"  {m.PlayerName}{leader}{ready}");
                }
                GUILayout.EndScrollView();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Invite ID:", GUILayout.Width(60));
                _inviteTargetId = GUILayout.TextField(_inviteTargetId, GUILayout.Width(40));
                if (GUILayout.Button("Send"))
                {
                    if (int.TryParse(_inviteTargetId, out var id))
                    {
                        NetworkClient.Send(new PartyInviteMessage
                        {
                            PartyID = _currentPartyId,
                            TargetConnectionID = id
                        });
                    }
                }
                GUILayout.EndHorizontal();

                if (GUILayout.Button("Leave Party"))
                {
                    NetworkClient.Send(new PartyLeaveMessage { PartyID = _currentPartyId });
                    _currentPartyId = 0;
                    _currentPartyName = null;
                    _partyMembers.Clear();
                }
            }

            GUILayout.EndArea();
        }

        private void DrawPendingInvites()
        {
            var y = 10f + offsetY;

            for (var i = _pendingInvites.Count - 1; i >= 0; i--)
            {
                var invite = _pendingInvites[i];

                GUILayout.BeginArea(new Rect(10 + offsetX, y, 200f, 50f), _boxStyle);
                GUILayout.Label($"Invite: {invite.InviterName}");

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Accept"))
                {
                    NetworkClient.Send(new PartyInviteResponseMessage { PartyID = invite.PartyId, Accepted = true });
                    _pendingInvites.RemoveAt(i);
                }
                if (GUILayout.Button("Decline"))
                {
                    NetworkClient.Send(new PartyInviteResponseMessage { PartyID = invite.PartyId, Accepted = false });
                    _pendingInvites.RemoveAt(i);
                }
                GUILayout.EndHorizontal();

                GUILayout.EndArea();
                y += 55f;
            }
        }

        #endregion

        #region Helpers

        private static int GetLocalConnectionId()
        {
            if (_cachedLocalConnectionId >= 0) return _cachedLocalConnectionId;
            if (NetworkServer.localConnection != null)
            {
                _cachedLocalConnectionId = NetworkServer.localConnection.connectionId;
                return _cachedLocalConnectionId;
            }
            return -1;
        }

        public static void SetLocalConnectionId(int id) => _cachedLocalConnectionId = id;

        private static Color GetRoleColor(RoomRole role) => role switch
        {
            RoomRole.Guest => Color.gray,
            RoomRole.Member => Color.white,
            RoomRole.Moderator => Color.cyan,
            RoomRole.Admin => Color.yellow,
            RoomRole.Owner => new Color(1f, 0.6f, 0.2f),
            _ => Color.white
        };

        private static string GetPermissionSummary(RoomRole role) => role switch
        {
            RoomRole.Owner => "All",
            RoomRole.Admin => "Mod+Settings",
            RoomRole.Moderator => "Kick/Mute",
            RoomRole.Member => "Chat",
            RoomRole.Guest => "View",
            _ => "?"
        };

        private static string GetStateName(byte id) => id switch
        {
            0 => "Lobby",
            1 => "Starting",
            2 => "Playing",
            3 => "Paused",
            4 => "Ended",
            _ => $"State {id}"
        };

        private static Color GetStateColor(byte id) => id switch
        {
            0 => Color.white,
            1 => Color.cyan,
            2 => Color.green,
            3 => Color.yellow,
            4 => Color.red,
            _ => Color.gray
        };

        private static string FormatTime(float sec)
        {
            var m = Mathf.FloorToInt(sec / 60f);
            var s = Mathf.FloorToInt(sec % 60f);
            return $"{m:D2}:{s:D2}";
        }

        private static Texture2D MakeTex(int w, int h, Color c)
        {
            var pix = new Color[w * h];
            for (var i = 0; i < pix.Length; i++) pix[i] = c;
            var tex = new Texture2D(w, h);
            tex.SetPixels(pix);
            tex.Apply();
            return tex;
        }

        #endregion
    }
}

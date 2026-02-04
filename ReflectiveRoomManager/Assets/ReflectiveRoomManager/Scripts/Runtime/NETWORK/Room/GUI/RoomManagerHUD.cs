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
    using REFLECTIVE.Runtime.NETWORK.Party;
    using REFLECTIVE.Runtime.NETWORK.Party.Messages;
    using REFLECTIVE.Runtime.NETWORK.Team;
    using REFLECTIVE.Runtime.NETWORK.Team.Messages;

    [AddComponentMenu("REFLECTIVE/Network Room Manager HUD")]
    public class RoomManagerHUD : MonoBehaviour
    {
        [SerializeField] private float offsetX;
        [SerializeField] private float offsetY;

        private static string _roomNameField = "Room Name";
        private static string _maxPlayers = "Max Player";

        private static bool _isServer;

        private static bool _showingRoomList;
        private static bool _showingPartyPanel;
        private static bool _showingTeamPanel;

        private static Vector2 _scrollPosition;
        private static Vector2 _partyScrollPosition;
        private static Vector2 _teamScrollPosition;

        private static GUIStyle backgroundStyle;

        // Party fields
        private static string _partyNameField = "My Party";
        private static string _inviteTargetId = "0";

        // Client-side cached data
        private static uint _currentPartyId;
        private static string _currentPartyName;
        private static int _currentPartyLeaderId;
        private static List<PartyMemberData> _partyMembers = new();
        private static List<PendingInvite> _pendingInvites = new();

        // Team client-side cached data
        private static uint _currentTeamId;
        private static string _currentTeamName;
        private static Color _currentTeamColor = Color.white;
        private static TeamData[] _allTeams;

        // State Machine client-side cached data
        private static byte _currentStateId;
        private static float _stateElapsedTime;
        private static Dictionary<string, string> _stateData = new();
        private static bool _isReady;
        private static bool _showingStatePanel = true;

        private struct PendingInvite
        {
            public uint PartyId;
            public string InviterName;
        }

        protected virtual void Start()
        {
            RegisterClientEventHandlers();
        }

        protected virtual void OnDestroy()
        {
            UnregisterClientEventHandlers();
        }

        private void RegisterClientEventHandlers()
        {
            var roomManager = RoomManagerBase.Instance;
            if (roomManager == null) return;

            // Party events
            if (roomManager.EnablePartySystem && roomManager.ClientPartyEvents != null)
            {
                roomManager.ClientPartyEvents.OnClientPartySync += OnPartySync;
                roomManager.ClientPartyEvents.OnClientInviteReceived += OnInviteReceived;
                roomManager.ClientPartyEvents.OnClientPartyLeft += OnPartyLeft;
            }

            // Team events
            if (roomManager.EnableTeamSystem && roomManager.ClientTeamEvents != null)
            {
                roomManager.ClientTeamEvents.OnClientTeamAssigned += OnTeamAssigned;
                roomManager.ClientTeamEvents.OnClientTeamsUpdated += OnTeamsUpdated;
                roomManager.ClientTeamEvents.OnClientTeamLeft += OnTeamLeft;
            }

            // State Machine events
            if (roomManager.EnableStateMachine)
            {
                RoomStateNetworkHandlers.OnClientRoomStateChanged += OnStateChanged;
                RoomStateNetworkHandlers.OnClientRoomStateSync += OnStateSync;
            }
        }

        private void UnregisterClientEventHandlers()
        {
            var roomManager = RoomManagerBase.Instance;
            if (roomManager == null) return;

            // Party events
            if (roomManager.EnablePartySystem && roomManager.ClientPartyEvents != null)
            {
                roomManager.ClientPartyEvents.OnClientPartySync -= OnPartySync;
                roomManager.ClientPartyEvents.OnClientInviteReceived -= OnInviteReceived;
                roomManager.ClientPartyEvents.OnClientPartyLeft -= OnPartyLeft;
            }

            // Team events
            if (roomManager.EnableTeamSystem && roomManager.ClientTeamEvents != null)
            {
                roomManager.ClientTeamEvents.OnClientTeamAssigned -= OnTeamAssigned;
                roomManager.ClientTeamEvents.OnClientTeamsUpdated -= OnTeamsUpdated;
                roomManager.ClientTeamEvents.OnClientTeamLeft -= OnTeamLeft;
            }

            // State Machine events
            if (roomManager.EnableStateMachine)
            {
                RoomStateNetworkHandlers.OnClientRoomStateChanged -= OnStateChanged;
                RoomStateNetworkHandlers.OnClientRoomStateSync -= OnStateSync;
            }
        }

        #region Client Event Handlers

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

            // Update current team color
            if (_currentTeamId != 0 && teams != null)
            {
                var currentTeam = teams.FirstOrDefault(t => t.TeamID == _currentTeamId);
                if (currentTeam.TeamID != 0)
                {
                    _currentTeamColor = currentTeam.TeamColor;
                }
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

        private void OnStateChanged(uint roomId, RoomStateData stateData)
        {
            _currentStateId = stateData.StateTypeID;
            _stateElapsedTime = stateData.ElapsedTime;
            _stateData = stateData.Data ?? new Dictionary<string, string>();

            // Reset ready state when transitioning out of lobby
            if (_currentStateId != 0)
                _isReady = false;
        }

        private void OnStateSync(uint roomId, byte stateTypeId, float elapsedTime, Dictionary<string, string> stateData)
        {
            _currentStateId = stateTypeId;
            _stateElapsedTime = elapsedTime;
            if (stateData != null)
                _stateData = stateData;
        }

        #endregion

        protected virtual void OnGUI()
        {
            if (!NetworkClient.active && !NetworkServer.active) return;

            _isServer = !NetworkClient.isConnected && NetworkServer.active;

            var roomManager = RoomManagerBase.Instance;

            if (!roomManager) return;

            // Show pending invites (always visible)
            if (!_isServer && roomManager.EnablePartySystem)
            {
                ShowPendingInvites();
            }

            if (!_isServer)
            {
                var currentRoom = roomManager.GetCurrentRoomInfo();

                if (!string.IsNullOrEmpty(currentRoom.RoomName))
                {
                    ShowCurrentRoom(currentRoom);

                    // Show State Machine panel when in room
                    if (roomManager.EnableStateMachine)
                    {
                        ShowStatePanel(currentRoom);
                    }

                    // Show Team panel when in room
                    if (roomManager.EnableTeamSystem)
                    {
                        ShowTeamPanel();
                    }
                    return;
                }
            }

            if (_showingRoomList)
            {
                ShowRoomList();
                return;
            }

            if (_showingPartyPanel && !_isServer)
            {
                ShowPartyPanel();
                return;
            }

            ShowRoomButtons();

            // Show Party button when not in room (client only)
            if (!_isServer && roomManager.EnablePartySystem)
            {
                ShowPartyButton();
            }
        }

        private void ShowRoomButtons()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 230f + offsetX, 30 + offsetY, 200f, 100f));

            GUILayout.BeginVertical();

            _roomNameField = GUILayout.TextField(_roomNameField,
                GUILayout.MinWidth(20));
            _maxPlayers = GUILayout.TextField(_maxPlayers,
                GUILayout.MinWidth(2));

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Create Room"))
            {
                var roomInfo = new RoomInfo
                {
                    RoomName = _roomNameField,
                    SceneName = RoomManagerBase.Instance.RoomScene,
                    MaxPlayers = int.TryParse(_maxPlayers, out var result) ? result : 2,
                    CustomData = new Dictionary<string, string>()
                };

                if (_isServer)
                    RoomServer.CreateRoom(roomInfo);
                else
                    RoomClient.CreateRoom(roomInfo);
            }

            if (!_isServer)
            {
                if (GUILayout.Button("Join Room"))
                {
                    RoomClient.JoinRoom(_roomNameField);
                }

                GUILayout.EndHorizontal();

                if (GUILayout.Button("Show Rooms"))
                {
                    _showingRoomList = true;
                }
            }
            else
            {
                if (GUILayout.Button("Show Rooms"))
                {
                    _showingRoomList = true;
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();

            GUILayout.EndArea();
        }

        private void ShowCurrentRoom(RoomInfo roomInfo)
        {
            GUILayout.BeginArea(new Rect(Screen.width - 230f + offsetX, 30 + offsetY, 200f, 200f));

            GUILayout.Label($"Room Name : {roomInfo.RoomName}");
            GUILayout.Label($"Max Player Count : {roomInfo.MaxPlayers}");
            GUILayout.Label($"Current Player Count : {roomInfo.CurrentPlayers}");

            if (GUILayout.Button("Exit Room"))
            {
                RoomClient.ExitRoom();
                // Clear team data on exit
                _currentTeamId = 0;
                _currentTeamName = null;
                _allTeams = null;
                // Clear state data on exit
                _currentStateId = 0;
                _stateElapsedTime = 0f;
                _stateData.Clear();
                _isReady = false;
            }

            GUILayout.EndArea();
        }

        private void ShowRoomList()
        {
            backgroundStyle ??= new GUIStyle
            {
                normal =
                {
                    background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.5f))
                }
            };

            GUILayout.BeginArea(new Rect(Screen.width - 230f + offsetX, 30 + offsetY, 200f, Screen.height - 30));

            GUILayout.BeginVertical();

            if (GUILayout.Button("Close Rooms"))
                _showingRoomList = false;

            if (_isServer)
            {
                var rooms = RoomManagerBase.Instance.GetRooms().ToList();

                var height = Mathf.Min(rooms.Count * 25f, Screen.height - 25);

                UnityEngine.GUI.Box(new Rect(0, 25, 200f, height), "", backgroundStyle);
                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

                foreach (var room in rooms.Where(room => GUILayout.Button($"{room.Name} - {room.CurrentPlayers}/{room.MaxPlayers}")))
                {
                    RoomServer.RemoveRoom(room.Name, forced: true);
                }
            }
            else
            {
                var rooms = RoomManagerBase.Instance.GetRoomInfos().ToList();

                var height = Mathf.Min(rooms.Count * 25f, Screen.height - 25);

                UnityEngine.GUI.Box(new Rect(0, 25, 200f, height), "", backgroundStyle);
                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

                foreach (var room in rooms.Where(room => GUILayout.Button($"{room.RoomName} - {room.CurrentPlayers}/{room.MaxPlayers}")))
                {
                    RoomClient.JoinRoom(room.RoomName);
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.EndArea();
        }

        #region Party GUI

        private void ShowPartyButton()
        {
            var yOffset = 140f;
            GUILayout.BeginArea(new Rect(Screen.width - 230f + offsetX, yOffset + offsetY, 200f, 30f));

            var buttonText = _currentPartyId != 0 ? $"Party: {_currentPartyName}" : "Party";
            if (GUILayout.Button(buttonText))
            {
                _showingPartyPanel = true;
            }

            GUILayout.EndArea();
        }

        private void ShowPartyPanel()
        {
            backgroundStyle ??= new GUIStyle
            {
                normal =
                {
                    background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.5f))
                }
            };

            GUILayout.BeginArea(new Rect(Screen.width - 230f + offsetX, 30 + offsetY, 200f, 300f));
            UnityEngine.GUI.Box(new Rect(0, 0, 200f, 300f), "", backgroundStyle);

            GUILayout.BeginVertical();

            GUILayout.Label("--- PARTY ---");

            if (GUILayout.Button("Close"))
            {
                _showingPartyPanel = false;
            }

            GUILayout.Space(10);

            if (_currentPartyId == 0)
            {
                // Not in a party - show create options
                GUILayout.Label("Party Name:");
                _partyNameField = GUILayout.TextField(_partyNameField);

                if (GUILayout.Button("Create Party"))
                {
                    var msg = new PartyCreateMessage
                    {
                        PartyName = _partyNameField,
                        MaxSize = 4,
                        IsPublic = false,
                        AutoAcceptFriends = false,
                        AllowVoiceChat = false
                    };
                    NetworkClient.Send(msg);
                }
            }
            else
            {
                // In a party - show party info
                GUILayout.Label($"Party: {_currentPartyName}");
                GUILayout.Label($"Members: {_partyMembers.Count}");

                GUILayout.Space(5);

                // Show members
                _partyScrollPosition = GUILayout.BeginScrollView(_partyScrollPosition, GUILayout.Height(80));
                foreach (var member in _partyMembers)
                {
                    var leaderMark = member.ConnectionID == _currentPartyLeaderId ? " [L]" : "";
                    var readyMark = member.IsReady ? " [R]" : "";
                    GUILayout.Label($"  {member.PlayerName}{leaderMark}{readyMark}");
                }
                GUILayout.EndScrollView();

                GUILayout.Space(5);

                // Invite player
                GUILayout.Label("Invite (Conn ID):");
                _inviteTargetId = GUILayout.TextField(_inviteTargetId);

                if (GUILayout.Button("Send Invite"))
                {
                    if (int.TryParse(_inviteTargetId, out var targetId))
                    {
                        var msg = new PartyInviteMessage
                        {
                            PartyID = _currentPartyId,
                            TargetConnectionID = targetId
                        };
                        NetworkClient.Send(msg);
                    }
                }

                GUILayout.Space(5);

                if (GUILayout.Button("Leave Party"))
                {
                    var msg = new PartyLeaveMessage { PartyID = _currentPartyId };
                    NetworkClient.Send(msg);
                    _currentPartyId = 0;
                    _currentPartyName = null;
                    _partyMembers.Clear();
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void ShowPendingInvites()
        {
            if (_pendingInvites.Count == 0) return;

            var yOffset = 30f;
            GUILayout.BeginArea(new Rect(10 + offsetX, yOffset + offsetY, 250f, _pendingInvites.Count * 60f));

            for (var i = _pendingInvites.Count - 1; i >= 0; i--)
            {
                var invite = _pendingInvites[i];

                GUILayout.BeginVertical("box");
                GUILayout.Label($"Party invite from: {invite.InviterName}");

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Accept"))
                {
                    var msg = new PartyInviteResponseMessage
                    {
                        PartyID = invite.PartyId,
                        Accepted = true
                    };
                    NetworkClient.Send(msg);
                    _pendingInvites.RemoveAt(i);
                }

                if (GUILayout.Button("Decline"))
                {
                    var msg = new PartyInviteResponseMessage
                    {
                        PartyID = invite.PartyId,
                        Accepted = false
                    };
                    NetworkClient.Send(msg);
                    _pendingInvites.RemoveAt(i);
                }
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
            }

            GUILayout.EndArea();
        }

        #endregion

        #region Team GUI

        private void ShowTeamPanel()
        {
            backgroundStyle ??= new GUIStyle
            {
                normal =
                {
                    background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.5f))
                }
            };

            // Position below the room info
            var yOffset = 240f;
            GUILayout.BeginArea(new Rect(Screen.width - 230f + offsetX, yOffset + offsetY, 200f, 250f));
            UnityEngine.GUI.Box(new Rect(0, 0, 200f, 250f), "", backgroundStyle);

            GUILayout.BeginVertical();

            GUILayout.Label("--- TEAM ---");

            if (_currentTeamId != 0)
            {
                // Draw colored team name
                var originalColor = UnityEngine.GUI.color;
                UnityEngine.GUI.color = _currentTeamColor;
                GUILayout.Label($"Your Team: {_currentTeamName}");
                UnityEngine.GUI.color = originalColor;
            }
            else
            {
                GUILayout.Label("Not assigned to team");
            }

            GUILayout.Space(10);

            // Show all teams
            if (_allTeams != null && _allTeams.Length > 0)
            {
                GUILayout.Label("All Teams:");

                _teamScrollPosition = GUILayout.BeginScrollView(_teamScrollPosition, GUILayout.Height(120));

                foreach (var team in _allTeams)
                {
                    var originalColor = UnityEngine.GUI.color;
                    UnityEngine.GUI.color = team.TeamColor;

                    var memberCount = team.Members?.Length ?? 0;
                    var isMyTeam = team.TeamID == _currentTeamId ? " *" : "";

                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"{team.TeamName}{isMyTeam} ({memberCount}/{team.MaxSize})");

                    // Swap button (only if not current team)
                    if (team.TeamID != _currentTeamId)
                    {
                        UnityEngine.GUI.color = originalColor;
                        if (GUILayout.Button("Join", GUILayout.Width(40)))
                        {
                            var msg = new TeamSwapRequestMessage { TargetTeamID = team.TeamID };
                            NetworkClient.Send(msg);
                        }
                    }

                    UnityEngine.GUI.color = originalColor;
                    GUILayout.EndHorizontal();

                    // Show team stats
                    GUILayout.Label($"  Score: {team.Stats.TotalScore} | K/D: {team.Stats.TotalKills}/{team.Stats.TotalDeaths}");
                }

                GUILayout.EndScrollView();
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        #endregion

        #region State Machine GUI

        private void ShowStatePanel(RoomInfo roomInfo)
        {
            backgroundStyle ??= new GUIStyle
            {
                normal =
                {
                    background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.5f))
                }
            };

            // Position on the left side of the screen (below potential pending invites)
            var inviteOffset = _pendingInvites.Count > 0 ? _pendingInvites.Count * 60f + 10f : 0f;
            GUILayout.BeginArea(new Rect(10 + offsetX, 30 + offsetY + inviteOffset, 220f, 280f));
            UnityEngine.GUI.Box(new Rect(0, 0, 220f, 280f), "", backgroundStyle);

            GUILayout.BeginVertical();

            GUILayout.Label("--- GAME STATE ---");

            // Current state with color coding
            var stateColor = GetStateColor(_currentStateId);
            var stateName = GetStateName(_currentStateId);
            var originalColor = UnityEngine.GUI.color;
            UnityEngine.GUI.color = stateColor;
            GUILayout.Label($"State: {stateName}");
            UnityEngine.GUI.color = originalColor;

            // Elapsed time
            GUILayout.Label($"Time: {_stateElapsedTime:F1}s");

            GUILayout.Space(10);

            // State-specific UI
            switch (_currentStateId)
            {
                case 0: // Lobby
                    ShowLobbyStateUI(roomInfo);
                    break;
                case 1: // Starting
                    ShowStartingStateUI();
                    break;
                case 2: // Playing
                    ShowPlayingStateUI();
                    break;
                case 3: // Paused
                    ShowPausedStateUI();
                    break;
                case 4: // Ended
                    ShowEndedStateUI();
                    break;
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void ShowLobbyStateUI(RoomInfo roomInfo)
        {
            // Ready players count
            var readyCount = 0;
            if (_stateData.TryGetValue("ReadyPlayers", out var readyStr))
                int.TryParse(readyStr, out readyCount);

            GUILayout.Label($"Ready: {readyCount}/{roomInfo.CurrentPlayers}");

            // Countdown active
            var countdownActive = false;
            if (_stateData.TryGetValue("CountdownActive", out var countdownStr))
                bool.TryParse(countdownStr, out countdownActive);

            if (countdownActive)
            {
                var countdownRemaining = 0f;
                if (_stateData.TryGetValue("CountdownRemaining", out var remainingStr))
                    float.TryParse(remainingStr, out countdownRemaining);

                var originalColor = UnityEngine.GUI.color;
                UnityEngine.GUI.color = Color.yellow;
                GUILayout.Label($"Starting in: {countdownRemaining:F1}s");
                UnityEngine.GUI.color = originalColor;
            }

            GUILayout.Space(10);

            // Ready/Unready button
            if (_isReady)
            {
                var originalColor = UnityEngine.GUI.color;
                UnityEngine.GUI.color = Color.green;
                if (GUILayout.Button("READY (Click to cancel)"))
                {
                    SendStateAction(RoomStateAction.UnmarkReady);
                    _isReady = false;
                }
                UnityEngine.GUI.color = originalColor;
            }
            else
            {
                if (GUILayout.Button("Ready Up"))
                {
                    SendStateAction(RoomStateAction.MarkReady);
                    _isReady = true;
                }
            }
        }

        private void ShowStartingStateUI()
        {
            var roomManager = RoomManagerBase.Instance;
            var countdownDuration = roomManager?.StateConfig?.StartingCountdownDuration ?? 3f;
            var remaining = Mathf.Max(0f, countdownDuration - _stateElapsedTime);

            var originalColor = UnityEngine.GUI.color;
            UnityEngine.GUI.color = Color.cyan;
            GUILayout.Label($"Game starting in: {remaining:F1}s");
            UnityEngine.GUI.color = originalColor;

            // Big countdown display
            var bigStyle = new GUIStyle(UnityEngine.GUI.skin.label)
            {
                fontSize = 48,
                alignment = TextAnchor.MiddleCenter
            };
            bigStyle.normal.textColor = Color.white;
            GUILayout.Label(Mathf.CeilToInt(remaining).ToString(), bigStyle, GUILayout.Height(60));
        }

        private void ShowPlayingStateUI()
        {
            var roomManager = RoomManagerBase.Instance;
            var maxDuration = roomManager?.StateConfig?.MaxGameDuration ?? 0f;

            if (maxDuration > 0f)
            {
                var remaining = Mathf.Max(0f, maxDuration - _stateElapsedTime);
                GUILayout.Label($"Time remaining: {FormatTime(remaining)}");
            }
            else
            {
                GUILayout.Label($"Game time: {FormatTime(_stateElapsedTime)}");
            }

            GUILayout.Space(10);

            var allowPausing = roomManager?.StateConfig?.AllowPausing ?? false;
            if (allowPausing)
            {
                if (GUILayout.Button("Pause Game"))
                {
                    SendStateAction(RoomStateAction.PauseGame);
                }
            }

            GUILayout.Space(5);

            if (GUILayout.Button("End Game"))
            {
                SendStateAction(RoomStateAction.EndGame);
            }
        }

        private void ShowPausedStateUI()
        {
            var roomManager = RoomManagerBase.Instance;
            var pauseTimeout = roomManager?.StateConfig?.PauseTimeout ?? 30f;

            var originalColor = UnityEngine.GUI.color;
            UnityEngine.GUI.color = Color.yellow;
            GUILayout.Label("--- GAME PAUSED ---");
            UnityEngine.GUI.color = originalColor;

            if (pauseTimeout > 0f)
            {
                var remaining = Mathf.Max(0f, pauseTimeout - _stateElapsedTime);
                GUILayout.Label($"Auto-resume in: {remaining:F1}s");
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Resume Game"))
            {
                SendStateAction(RoomStateAction.ResumeGame);
            }

            GUILayout.Space(5);

            if (GUILayout.Button("End Game"))
            {
                SendStateAction(RoomStateAction.EndGame);
            }
        }

        private void ShowEndedStateUI()
        {
            var roomManager = RoomManagerBase.Instance;
            var endScreenDuration = roomManager?.StateConfig?.EndScreenDuration ?? 10f;
            var autoReturn = roomManager?.StateConfig?.AutoReturnToLobby ?? true;

            var originalColor = UnityEngine.GUI.color;
            UnityEngine.GUI.color = Color.red;
            GUILayout.Label("--- GAME ENDED ---");
            UnityEngine.GUI.color = originalColor;

            if (autoReturn && endScreenDuration > 0f)
            {
                var remaining = Mathf.Max(0f, endScreenDuration - _stateElapsedTime);
                GUILayout.Label($"Returning to lobby in: {remaining:F1}s");
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Return to Lobby"))
            {
                SendStateAction(RoomStateAction.RestartGame);
            }
        }

        private void SendStateAction(RoomStateAction action, string payload = null)
        {
            var roomManager = RoomManagerBase.Instance;
            if (roomManager == null) return;

            var currentRoom = roomManager.GetCurrentRoomInfo();
            if (string.IsNullOrEmpty(currentRoom.RoomName)) return;

            var msg = new RoomStateActionMessage(currentRoom.ID, action, payload);
            NetworkClient.Send(msg);
        }

        private static string GetStateName(byte stateId)
        {
            return stateId switch
            {
                0 => "Lobby",
                1 => "Starting",
                2 => "Playing",
                3 => "Paused",
                4 => "Ended",
                _ => $"Unknown ({stateId})"
            };
        }

        private static Color GetStateColor(byte stateId)
        {
            return stateId switch
            {
                0 => Color.white,        // Lobby - neutral
                1 => Color.cyan,         // Starting - attention
                2 => Color.green,        // Playing - active
                3 => Color.yellow,       // Paused - warning
                4 => Color.red,          // Ended - stopped
                _ => Color.gray
            };
        }

        private static string FormatTime(float seconds)
        {
            var minutes = Mathf.FloorToInt(seconds / 60f);
            var secs = Mathf.FloorToInt(seconds % 60f);
            return $"{minutes:D2}:{secs:D2}";
        }

        #endregion

        private static Texture2D MakeTex(int width, int height, Color color)
        {
            var pix = new Color[width * height];

            for (var i = 0; i < pix.Length; ++i)
            {
                pix[i] = color;
            }

            var result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();

            return result;
        }
    }
}

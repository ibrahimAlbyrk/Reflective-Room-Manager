using Mirror;
using UnityEngine;
using System.Collections.Generic;

namespace REFLECTIVE.Runtime.NETWORK.Room.GUI
{
    using Service;
    using Structs;
    using Roles;
    using Modules;

    /// <summary>
    /// Main HUD for Room Manager.
    /// Toggle modules from inspector, instances created at runtime.
    /// </summary>
    [AddComponentMenu("REFLECTIVE/Network Room Manager HUD")]
    public class RoomManagerHUD : MonoBehaviour
    {
        [Header("Offset")]
        [SerializeField] private float offsetX;
        [SerializeField] private float offsetY;

        [Header("HUD Modules")]
        [SerializeField] private bool _enableStateHUD = true;
        [SerializeField] private bool _enableTeamHUD = true;
        [SerializeField] private bool _enableRolesHUD = true;
        [SerializeField] private bool _enablePartyHUD = true;
        [SerializeField] private bool _enableDiscoveryHUD = true;
        [SerializeField] private bool _enableChatHUD = true;

        // Module instances
        private StateHUDModule _stateModule;
        private TeamHUDModule _teamModule;
        private RolesHUDModule _rolesModule;
        private PartyHUDModule _partyModule;
        private DiscoveryHUDModule _discoveryModule;
        private ChatHUDModule _chatModule;

        // Active tabs
        private List<IHUDModule> _activeTabs = new();
        private int _selectedTab;

        // Room UI state
        private string _roomName = "Room";
        private string _maxPlayers = "4";
        private bool _isServer;
        private bool _showingRoomList;
        private Vector2 _roomListScroll;

        #region Lifecycle

        private void Start()
        {
            var rm = RoomManagerBase.Instance;
            if (rm == null) return;

            // State
            if (_enableStateHUD && rm.EnableStateMachine)
            {
                _stateModule = new StateHUDModule();
                _stateModule.RegisterEvents();
                _activeTabs.Add(_stateModule);
            }

            // Team
            if (_enableTeamHUD && rm.EnableTeamSystem)
            {
                _teamModule = new TeamHUDModule();
                _teamModule.RegisterEvents();
                _activeTabs.Add(_teamModule);
            }

            // Roles
            if (_enableRolesHUD && rm.EnableRoleSystem)
            {
                _rolesModule = new RolesHUDModule();
                _rolesModule.RegisterEvents();
                _activeTabs.Add(_rolesModule);
            }

            // Party
            if (_enablePartyHUD && rm.EnablePartySystem)
            {
                _partyModule = new PartyHUDModule();
                _partyModule.RegisterEvents();
            }

            // Discovery
            if (_enableDiscoveryHUD && rm.EnableRoomDiscovery)
            {
                _discoveryModule = new DiscoveryHUDModule();
                _discoveryModule.RegisterEvents();
            }

            // Chat
            if (_enableChatHUD && rm.EnableChatSystem)
            {
                _chatModule = new ChatHUDModule();
                _chatModule.RegisterEvents();
                _activeTabs.Add(_chatModule);
            }
        }

        private void OnDestroy()
        {
            _stateModule?.UnregisterEvents();
            _teamModule?.UnregisterEvents();
            _rolesModule?.UnregisterEvents();
            _partyModule?.UnregisterEvents();
            _discoveryModule?.UnregisterEvents();
            _chatModule?.UnregisterEvents();
        }

        #endregion

        #region GUI

        private void OnGUI()
        {
            if (!NetworkClient.active && !NetworkServer.active) return;

            HUDStyles.Init();
            _isServer = !NetworkClient.isConnected && NetworkServer.active;

            var rm = RoomManagerBase.Instance;
            if (!rm) return;

            // Party invites (top-left)
            if (!_isServer && _partyModule != null && _partyModule.HasInvites)
                _partyModule.DrawInvites(10 + offsetX, 10 + offsetY);

            // In room?
            if (!_isServer)
            {
                var room = rm.GetCurrentRoomInfo();
                if (!string.IsNullOrEmpty(room.RoomName))
                {
                    DrawInRoomUI(rm, room);
                    return;
                }
            }

            // Lobby
            if (_showingRoomList)
                DrawRoomList(rm);
            else if (_partyModule != null && _partyModule.ShowingPanel && !_isServer)
                _partyModule.DrawPanel(Screen.width - 230 + offsetX, 10 + offsetY);
            else
                DrawLobbyUI(rm);
        }

        #endregion

        #region Lobby

        private void DrawLobbyUI(RoomManagerBase rm)
        {
            var w = 220f;
            var h = _isServer ? 100f : 130f;
            if (!_isServer && _partyModule != null) h += 25f;

            GUILayout.BeginArea(new Rect(Screen.width - w - 10 + offsetX, 10 + offsetY, w, h), HUDStyles.BoxStyle);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Name:", GUILayout.Width(45));
            _roomName = GUILayout.TextField(_roomName);
            GUILayout.Label("Max:", GUILayout.Width(30));
            _maxPlayers = GUILayout.TextField(_maxPlayers, GUILayout.Width(25));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Create"))
            {
                var info = new RoomInfo
                {
                    RoomName = _roomName,
                    SceneName = rm.RoomScene,
                    MaxPlayers = int.TryParse(_maxPlayers, out var mp) ? mp : 4,
                    CustomData = new Dictionary<string, string>()
                };
                if (_isServer) RoomServer.CreateRoom(info);
                else RoomClient.CreateRoom(info);
            }

            if (!_isServer && GUILayout.Button("Join"))
                RoomClient.JoinRoom(_roomName);

            if (GUILayout.Button("Rooms"))
            {
                _showingRoomList = true;
                _discoveryModule?.SendQuery();
            }
            GUILayout.EndHorizontal();

            if (!_isServer && _partyModule != null)
            {
                var label = _partyModule.HasParty ? $"Party ({_partyModule.MemberCount})" : "Party";
                if (GUILayout.Button(label))
                    _partyModule.ShowingPanel = true;
            }

            GUILayout.EndArea();
        }

        private void DrawRoomList(RoomManagerBase rm)
        {
            // Use advanced discovery panel if available
            if (!_isServer && _discoveryModule != null)
            {
                DrawDiscoveryPanel();
                return;
            }

            // Fallback: simple room list
            var w = 250f;
            var h = 300f;

            GUILayout.BeginArea(new Rect(Screen.width - w - 10 + offsetX, 10 + offsetY, w, h), HUDStyles.BoxStyle);

            GUILayout.BeginHorizontal();
            GUILayout.Label("ROOMS", HUDStyles.HeaderStyle);
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

        private void DrawDiscoveryPanel()
        {
            var w = 380f;
            var h = 400f;

            // Close button area
            GUILayout.BeginArea(new Rect(Screen.width - w - 10 + offsetX, 10 + offsetY, w, 30));
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("X", GUILayout.Width(25)))
                _showingRoomList = false;
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            // Discovery panel
            _discoveryModule.DrawPanel(Screen.width - w - 10 + offsetX, 35 + offsetY, w, h - 30);
        }

        #endregion

        #region In-Room

        private void DrawInRoomUI(RoomManagerBase rm, RoomInfo room)
        {
            DrawRoomInfoBar(room);

            if (_activeTabs.Count == 0) return;

            var panelW = 280f;
            var panelH = 320f;

            GUILayout.BeginArea(new Rect(Screen.width - panelW - 10 + offsetX, 55 + offsetY, panelW, panelH), HUDStyles.BoxStyle);

            // Tabs
            GUILayout.BeginHorizontal();
            for (var i = 0; i < _activeTabs.Count; i++)
            {
                var style = _selectedTab == i ? HUDStyles.TabActiveStyle : HUDStyles.TabStyle;
                if (GUILayout.Button(_activeTabs[i].TabName, style))
                    _selectedTab = i;
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            // Content
            _selectedTab = Mathf.Clamp(_selectedTab, 0, _activeTabs.Count - 1);
            _activeTabs[_selectedTab].DrawTab(room);

            GUILayout.EndArea();
        }

        private void DrawRoomInfoBar(RoomInfo room)
        {
            var barW = 350f;
            var barH = 35f;

            GUILayout.BeginArea(new Rect(Screen.width - barW - 10 + offsetX, 10 + offsetY, barW, barH), HUDStyles.BoxStyle);
            GUILayout.BeginHorizontal();

            GUILayout.Label($"{room.RoomName}", GUILayout.Width(100));
            GUILayout.Label($"{room.CurrentPlayers}/{room.MaxPlayers}", GUILayout.Width(40));

            // Role indicator
            var myRole = _rolesModule?.MyRole ?? RoomRole.Member;
            var roleColor = RolesHUDModule.GetColor(myRole);
            var old = UnityEngine.GUI.color;
            UnityEngine.GUI.color = roleColor;
            GUILayout.Label($"[{myRole}]", GUILayout.Width(70));
            UnityEngine.GUI.color = old;

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Exit", GUILayout.Width(50)))
            {
                RoomClient.ExitRoom();
                ClearAllData();
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void ClearAllData()
        {
            _stateModule?.ClearData();
            _teamModule?.ClearData();
            _rolesModule?.ClearData();
            _partyModule?.ClearData();
            _discoveryModule?.ClearData();
            _chatModule?.ClearData();
        }

        #endregion
    }
}

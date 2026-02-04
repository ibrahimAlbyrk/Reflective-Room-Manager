using Mirror;
using System.Linq;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.GUI.Modules
{
    using Structs;
    using REFLECTIVE.Runtime.NETWORK.Team;
    using REFLECTIVE.Runtime.NETWORK.Team.Messages;

    public class TeamHUDModule : IHUDModule
    {
        public string TabName => "Team";

        private uint _teamId;
        private string _teamName;
        private Color _teamColor = Color.white;
        private TeamData[] _allTeams;
        private Vector2 _scroll;

        public void RegisterEvents()
        {
            var rm = RoomManagerBase.Instance;
            if (rm?.ClientTeamEvents == null) return;
            rm.ClientTeamEvents.OnClientTeamAssigned += OnAssigned;
            rm.ClientTeamEvents.OnClientTeamsUpdated += OnUpdated;
            rm.ClientTeamEvents.OnClientTeamLeft += OnLeft;
        }

        public void UnregisterEvents()
        {
            var rm = RoomManagerBase.Instance;
            if (rm?.ClientTeamEvents == null) return;
            rm.ClientTeamEvents.OnClientTeamAssigned -= OnAssigned;
            rm.ClientTeamEvents.OnClientTeamsUpdated -= OnUpdated;
            rm.ClientTeamEvents.OnClientTeamLeft -= OnLeft;
        }

        private void OnAssigned(uint id, string name)
        {
            _teamId = id;
            _teamName = name;
        }

        private void OnUpdated(TeamData[] teams)
        {
            _allTeams = teams;
            if (_teamId != 0 && teams != null)
            {
                var t = teams.FirstOrDefault(x => x.TeamID == _teamId);
                if (t.TeamID != 0) _teamColor = t.TeamColor;
            }
        }

        private void OnLeft(uint id)
        {
            if (_teamId == id)
            {
                _teamId = 0;
                _teamName = null;
            }
        }

        public void DrawTab(RoomInfo room)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Your Team:", GUILayout.Width(70));
            if (_teamId != 0)
            {
                var old = UnityEngine.GUI.color;
                UnityEngine.GUI.color = _teamColor;
                GUILayout.Label(_teamName ?? "Unknown");
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
            _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(180));

            foreach (var team in _allTeams)
            {
                var old = UnityEngine.GUI.color;
                UnityEngine.GUI.color = team.TeamColor;

                GUILayout.BeginHorizontal();
                var isMine = team.TeamID == _teamId;
                var mark = isMine ? "★ " : "";
                var count = team.Members?.Length ?? 0;
                GUILayout.Label($"{mark}{team.TeamName} ({count}/{team.MaxSize})", GUILayout.Width(150));

                UnityEngine.GUI.color = old;

                if (!isMine && GUILayout.Button("Join", GUILayout.Width(45)))
                    NetworkClient.Send(new TeamSwapRequestMessage { TargetTeamID = team.TeamID });

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }

        public void ClearData()
        {
            _teamId = 0;
            _teamName = null;
            _allTeams = null;
        }
    }
}

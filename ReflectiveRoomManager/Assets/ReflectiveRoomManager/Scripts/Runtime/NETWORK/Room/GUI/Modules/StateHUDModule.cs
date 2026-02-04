using Mirror;
using UnityEngine;
using System.Collections.Generic;

namespace REFLECTIVE.Runtime.NETWORK.Room.GUI.Modules
{
    using Structs;
    using State;
    using State.Messages;
    using State.Handlers;

    public class StateHUDModule : IHUDModule
    {
        public string TabName => "State";

        private byte _stateId;
        private float _elapsed;
        private Dictionary<string, string> _data = new();
        private bool _isReady;

        public void RegisterEvents()
        {
            RoomStateNetworkHandlers.OnClientRoomStateChanged += OnStateChanged;
            RoomStateNetworkHandlers.OnClientRoomStateSync += OnStateSync;
        }

        public void UnregisterEvents()
        {
            RoomStateNetworkHandlers.OnClientRoomStateChanged -= OnStateChanged;
            RoomStateNetworkHandlers.OnClientRoomStateSync -= OnStateSync;
        }

        private void OnStateChanged(uint roomId, RoomStateData data)
        {
            _stateId = data.StateTypeID;
            _elapsed = data.ElapsedTime;
            _data = data.Data ?? new Dictionary<string, string>();
            if (_stateId != 0) _isReady = false;
        }

        private void OnStateSync(uint roomId, byte stateId, float elapsed, Dictionary<string, string> data)
        {
            _stateId = stateId;
            _elapsed = elapsed;
            if (data != null) _data = data;
        }

        public void DrawTab(RoomInfo room)
        {
            var rm = RoomManagerBase.Instance;
            var stateName = GetStateName(_stateId);
            var stateColor = GetStateColor(_stateId);

            GUILayout.BeginHorizontal();
            var old = UnityEngine.GUI.color;
            UnityEngine.GUI.color = stateColor;
            GUILayout.Label($"● {stateName}", GUILayout.Width(100));
            UnityEngine.GUI.color = old;
            GUILayout.Label($"Time: {HUDStyles.FormatTime(_elapsed)}");
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            switch (_stateId)
            {
                case 0: DrawLobby(rm, room); break;
                case 1: DrawStarting(rm); break;
                case 2: DrawPlaying(rm); break;
                case 3: DrawPaused(rm); break;
                case 4: DrawEnded(rm); break;
            }
        }

        private void DrawLobby(RoomManagerBase rm, RoomInfo room)
        {
            _data.TryGetValue("ReadyPlayers", out var readyStr);
            int.TryParse(readyStr ?? "0", out var readyCount);
            GUILayout.Label($"Ready: {readyCount}/{room.CurrentPlayers}");

            if (_data.TryGetValue("CountdownActive", out var cd) && cd == "True")
            {
                _data.TryGetValue("CountdownRemaining", out var rem);
                float.TryParse(rem ?? "0", out var remaining);
                var old = UnityEngine.GUI.color;
                UnityEngine.GUI.color = Color.yellow;
                GUILayout.Label($"Starting in {remaining:F0}s...");
                UnityEngine.GUI.color = old;
            }

            GUILayout.Space(10);

            var btnText = _isReady ? "✓ READY" : "Ready Up";
            var old2 = UnityEngine.GUI.color;
            UnityEngine.GUI.color = _isReady ? Color.green : Color.white;

            if (GUILayout.Button(btnText, GUILayout.Height(35)))
            {
                SendAction(_isReady ? RoomStateAction.UnmarkReady : RoomStateAction.MarkReady);
                _isReady = !_isReady;
            }
            UnityEngine.GUI.color = old2;
        }

        private void DrawStarting(RoomManagerBase rm)
        {
            var duration = rm?.StateConfig?.StartingCountdownDuration ?? 3f;
            var remaining = Mathf.Max(0f, duration - _elapsed);

            var style = new GUIStyle(UnityEngine.GUI.skin.label)
            {
                fontSize = 64, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold
            };
            style.normal.textColor = Color.cyan;

            GUILayout.FlexibleSpace();
            GUILayout.Label(Mathf.CeilToInt(remaining).ToString(), style, GUILayout.Height(80));
            GUILayout.Label("GET READY!", HUDStyles.HeaderStyle);
            GUILayout.FlexibleSpace();
        }

        private void DrawPlaying(RoomManagerBase rm)
        {
            var maxDur = rm?.StateConfig?.MaxGameDuration ?? 0f;
            if (maxDur > 0f)
            {
                var rem = Mathf.Max(0f, maxDur - _elapsed);
                GUILayout.Label($"Remaining: {HUDStyles.FormatTime(rem)}");
            }

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if (rm?.StateConfig?.AllowPausing == true && GUILayout.Button("Pause"))
                SendAction(RoomStateAction.PauseGame);
            if (GUILayout.Button("End Game"))
                SendAction(RoomStateAction.EndGame);
            GUILayout.EndHorizontal();
        }

        private void DrawPaused(RoomManagerBase rm)
        {
            var timeout = rm?.StateConfig?.PauseTimeout ?? 30f;
            var old = UnityEngine.GUI.color;
            UnityEngine.GUI.color = Color.yellow;
            GUILayout.Label("PAUSED", HUDStyles.HeaderStyle);
            UnityEngine.GUI.color = old;

            if (timeout > 0f)
                GUILayout.Label($"Auto-resume in {Mathf.Max(0f, timeout - _elapsed):F0}s");

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Resume")) SendAction(RoomStateAction.ResumeGame);
            if (GUILayout.Button("End")) SendAction(RoomStateAction.EndGame);
            GUILayout.EndHorizontal();
        }

        private void DrawEnded(RoomManagerBase rm)
        {
            var duration = rm?.StateConfig?.EndScreenDuration ?? 10f;
            var autoReturn = rm?.StateConfig?.AutoReturnToLobby ?? true;

            var old = UnityEngine.GUI.color;
            UnityEngine.GUI.color = Color.red;
            GUILayout.Label("GAME OVER", HUDStyles.HeaderStyle);
            UnityEngine.GUI.color = old;

            if (autoReturn && duration > 0f)
                GUILayout.Label($"Returning to lobby in {Mathf.Max(0f, duration - _elapsed):F0}s");

            GUILayout.Space(10);
            if (GUILayout.Button("Return to Lobby"))
                SendAction(RoomStateAction.RestartGame);
        }

        private void SendAction(RoomStateAction action)
        {
            var rm = RoomManagerBase.Instance;
            var room = rm?.GetCurrentRoomInfo();
            if (room == null || string.IsNullOrEmpty(room.Value.RoomName)) return;
            NetworkClient.Send(new RoomStateActionMessage(room.Value.ID, action, null));
        }

        public void ClearData()
        {
            _stateId = 0;
            _elapsed = 0f;
            _data.Clear();
            _isReady = false;
        }

        private static string GetStateName(byte id) => id switch
        {
            0 => "Lobby", 1 => "Starting", 2 => "Playing",
            3 => "Paused", 4 => "Ended", _ => $"State {id}"
        };

        private static Color GetStateColor(byte id) => id switch
        {
            0 => Color.white, 1 => Color.cyan, 2 => Color.green,
            3 => Color.yellow, 4 => Color.red, _ => Color.gray
        };
    }
}

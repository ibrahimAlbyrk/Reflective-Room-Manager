using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.GUI.Modules
{
    using Structs;
    using Voting;
    using Voting.Messages;
    using Voting.Handlers;

    public class VotingHUDModule : IHUDModule
    {
        public string TabName => "Vote";

        private const float ResultDisplayDuration = 5f;

        private const string VoteTypeKick = "kick_player";
        private const string VoteTypeSkip = "skip_round";
        private const string VoteTypeRestart = "restart_match";
        private const string VoteTypeEnd = "end_match";

        // Active vote state
        private bool _voteActive;
        private string _voteID;
        private string _voteTypeID;
        private string _question;
        private string[] _options;
        private int[] _voteCounts;
        private float _duration;
        private float _localStartTime;
        private bool _hasVoted;
        private int _myVoteIndex = -1;

        // Result display
        private bool _lastResultPassed;
        private string _lastResultText;
        private float _resultShowTime;

        // Start vote UI
        private string _kickTargetId = "0";
        private Vector2 _scroll;

        // Cached progress bar textures
        private Texture2D _barTex;
        private Color _barTexColor;

        public void RegisterEvents()
        {
#if REFLECTIVE_CLIENT
            VoteNetworkHandlers.OnClientVoteStartedEvent += OnVoteStarted;
            VoteNetworkHandlers.OnClientVoteUpdatedEvent += OnVoteUpdated;
            VoteNetworkHandlers.OnClientVoteEndedEvent += OnVoteEnded;
#endif
        }

        public void UnregisterEvents()
        {
#if REFLECTIVE_CLIENT
            VoteNetworkHandlers.OnClientVoteStartedEvent -= OnVoteStarted;
            VoteNetworkHandlers.OnClientVoteUpdatedEvent -= OnVoteUpdated;
            VoteNetworkHandlers.OnClientVoteEndedEvent -= OnVoteEnded;
#endif
        }

        private void OnVoteStarted(VoteStartedMessage msg)
        {
            _voteActive = true;
            _voteID = msg.VoteID;
            _voteTypeID = msg.VoteTypeID;
            _question = msg.Question;
            _options = msg.Options ?? System.Array.Empty<string>();
            _voteCounts = new int[_options.Length];
            _duration = msg.Duration;
            _localStartTime = Time.time;
            _hasVoted = false;
            _myVoteIndex = -1;
        }

        private void OnVoteUpdated(VoteUpdateMessage msg)
        {
            if (msg.VoteID != _voteID) return;

            _voteCounts = msg.VoteCounts ?? _voteCounts;
        }

        private void OnVoteEnded(VoteEndedMessage msg)
        {
            if (msg.VoteID != _voteID) return;

            _voteActive = false;
            _lastResultPassed = msg.Passed;

            var reasonText = msg.Reason switch
            {
                VoteEndReason.TimerExpired => "Time expired",
                VoteEndReason.AllVoted => "All voted",
                VoteEndReason.Cancelled => "Cancelled",
                VoteEndReason.StateChanged => "State changed",
                VoteEndReason.InitiatorLeft => "Initiator left",
                _ => "Unknown"
            };

            var resultLabel = msg.Passed ? "PASSED" : "FAILED";
            _lastResultText = $"{resultLabel} ({reasonText}) - {msg.ParticipationRate:P0} voted";
            _resultShowTime = Time.time + ResultDisplayDuration;
        }

        public void DrawTab(RoomInfo room)
        {
            _scroll = GUILayout.BeginScrollView(_scroll);

            if (_voteActive)
                DrawActiveVote(room);
            else
                DrawNoVote(room);

            GUILayout.EndScrollView();
        }

        #region Active Vote

        private void DrawActiveVote(RoomInfo room)
        {
            // Question
            GUILayout.Label(_question, HUDStyles.HeaderStyle);
            GUILayout.Space(5);

            // Timer
            DrawTimer();
            GUILayout.Space(5);

            // Options
            if (_options != null)
            {
                for (var i = 0; i < _options.Length; i++)
                    DrawOptionRow(room, i);
            }

            GUILayout.Space(10);

            // Cancel button (server validates permissions)
            if (GUILayout.Button("Cancel Vote"))
            {
                NetworkClient.Send(new CancelVoteRequest
                {
                    RoomID = room.ID,
                    VoteID = _voteID
                });
            }
        }

        private float RemainingTime => Mathf.Max(0f, _duration - (Time.time - _localStartTime));

        private void DrawTimer()
        {
            var remaining = RemainingTime;
            var ratio = _duration > 0f ? Mathf.Clamp01(remaining / _duration) : 0f;

            var timerColor = ratio > 0.5f ? Color.green
                : ratio > 0.25f ? Color.yellow
                : Color.red;

            var old = UnityEngine.GUI.color;
            UnityEngine.GUI.color = timerColor;
            GUILayout.Label($"Time: {remaining:F0}s");
            UnityEngine.GUI.color = old;

            // Progress bar
            var rect = GUILayoutUtility.GetRect(0, 12, GUILayout.ExpandWidth(true));
            UnityEngine.GUI.Box(rect, GUIContent.none);

            var fillRect = new Rect(rect.x, rect.y, rect.width * ratio, rect.height);
            if (_barTex == null || _barTexColor != timerColor)
            {
                _barTex = HUDStyles.MakeColorTex(timerColor);
                _barTexColor = timerColor;
            }
            UnityEngine.GUI.DrawTexture(fillRect, _barTex);
        }

        private void DrawOptionRow(RoomInfo room, int index)
        {
            var count = _voteCounts != null && index < _voteCounts.Length ? _voteCounts[index] : 0;
            var isMyChoice = _myVoteIndex == index;

            GUILayout.BeginHorizontal();

            // Option label + count
            var label = $"{_options[index]} ({count})";
            var old = UnityEngine.GUI.color;
            if (isMyChoice) UnityEngine.GUI.color = Color.green;
            GUILayout.Label(label, GUILayout.Width(150));
            UnityEngine.GUI.color = old;

            // Vote button
            UnityEngine.GUI.enabled = !_hasVoted;
            if (GUILayout.Button("Vote", GUILayout.Width(50)))
            {
                NetworkClient.Send(new CastVoteRequest
                {
                    RoomID = room.ID,
                    VoteID = _voteID,
                    OptionIndex = index
                });
                _hasVoted = true;
                _myVoteIndex = index;
            }
            UnityEngine.GUI.enabled = true;

            GUILayout.EndHorizontal();
        }

        #endregion

        #region No Vote

        private void DrawNoVote(RoomInfo room)
        {
            // Show last result briefly
            if (!string.IsNullOrEmpty(_lastResultText) && Time.time < _resultShowTime)
            {
                var old = UnityEngine.GUI.color;
                UnityEngine.GUI.color = _lastResultPassed ? Color.green : Color.red;
                GUILayout.Label(_lastResultText, HUDStyles.HeaderStyle);
                UnityEngine.GUI.color = old;
                GUILayout.Space(10);
            }

            GUILayout.Label("Start Vote:", HUDStyles.HeaderStyle);
            GUILayout.Space(5);

            // Kick
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Kick Player"))
            {
                var json = $"{{\"targetId\":\"{_kickTargetId}\"}}";
                SendStartVote(room, VoteTypeKick, json);
            }
            GUILayout.Label("ID:", GUILayout.Width(20));
            _kickTargetId = GUILayout.TextField(_kickTargetId, GUILayout.Width(35));
            GUILayout.EndHorizontal();

            // Other vote types
            if (GUILayout.Button("Skip Round"))
                SendStartVote(room, VoteTypeSkip);

            if (GUILayout.Button("Restart Match"))
                SendStartVote(room, VoteTypeRestart);

            if (GUILayout.Button("End Match"))
                SendStartVote(room, VoteTypeEnd);
        }

        private static void SendStartVote(RoomInfo room, string voteTypeID, string customDataJson = null)
        {
            NetworkClient.Send(new StartVoteRequest
            {
                RoomID = room.ID,
                VoteTypeID = voteTypeID,
                CustomDataJson = customDataJson
            });
        }

        #endregion

        public void ClearData()
        {
            _voteActive = false;
            _voteID = null;
            _voteTypeID = null;
            _question = null;
            _options = null;
            _voteCounts = null;
            _duration = 0f;
            _localStartTime = 0f;
            _hasVoted = false;
            _myVoteIndex = -1;
            _lastResultPassed = false;
            _lastResultText = null;
            _resultShowTime = 0f;
            _barTex = null;
        }
    }
}
